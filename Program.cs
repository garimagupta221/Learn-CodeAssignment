using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    enum MenuChoice
    {
        CreateAccount = 1,
        DeleteAccount,
        ViewAccountDetails,
        Deposit,
        Withdraw,
        Transfer,
        TakeLoan,
        CalculateLoan,
        Exit
    }

    internal class Program
    {
        static Customer SelectCustomer(ICustomerService customerService, IAccountService accountService)
        {
            Console.Write("Enter Customer Id: ");
            int id = int.Parse(Console.ReadLine());

            Customer customer = customerService.GetCustomerById(id);
            if (customer == null)
            {
                Console.Write("Customer Name: ");
                string name = Console.ReadLine();
                customer = customerService.CreateCustomer(id, name);
                accountService.CreateAccountForCustomer(customer);
            }

            return customer;
        }
        static void ShowMenu()
        {
            Console.WriteLine("1. Create Account");
            Console.WriteLine("2. Delete Account");
            Console.WriteLine("3. View Account Details");
            Console.WriteLine("4. Deposit");
            Console.WriteLine("5. Withdraw");
            Console.WriteLine("6. Transfer");
            Console.WriteLine("7. Take Loan");
            Console.WriteLine("8. Calculate Loan Interest & EMI");
            Console.WriteLine("9. Exit");
        }

        static void StartMenu(Customer customer, IAccountService accountService, ITransferService transferService)
        {
            while (true)
            {
                ShowMenu();

                int input = GetValidChoice();
                MenuChoice choice = (MenuChoice)input;
                if (choice == MenuChoice.Exit)
                {
                    Console.WriteLine("Exiting application...");
                    return;
                }
                switch (choice)
                {
                    case MenuChoice.CreateAccount:
                        accountService.CreateAccountForCustomer(customer);
                        Console.WriteLine("Account created successfully");
                        break;

                    case MenuChoice.DeleteAccount:
                        HandleDeleteAccount(customer, accountService);
                        break;

                    case MenuChoice.ViewAccountDetails:
                        HandleViewAccount(customer);
                        break;

                    case MenuChoice.Deposit:
                        HandleDeposit(customer);
                        break;

                    case MenuChoice.Withdraw:
                        HandleWithdraw(customer);
                        break;

                    case MenuChoice.Transfer:
                        HandleTransfer(customer, accountService, transferService);
                        break;

                    case MenuChoice.TakeLoan:
                        HandleCreateLoan(customer);
                        break;

                    case MenuChoice.CalculateLoan:
                        HandleLoanCalculation();
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please select 1–9.");
                        break;
                }
            }
        }

        private static int GetValidChoice()
        {
            while (true)
            {
                Console.Write("Enter choice (1-9): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 9)
                {
                    return choice;
                }
                Console.WriteLine("Invalid choice. Please enter 1 to 9");
            }
        }

        static void HandleDeleteAccount(Customer customer, IAccountService accountService)
        {
            Console.Write("Enter account number: ");
            int accountNo = int.Parse(Console.ReadLine());
            bool accountExists = false;
            for (int index = 0; index < customer.AccountCount; index++)
            {
                if (customer.Accounts[index].AccountNumber == accountNo)
                {
                    accountExists = true;
                    break;
                }
            }

            if (!accountExists)
            {
                Console.WriteLine("Invalid account");
                return;
            }
            Console.WriteLine(accountService.DeleteAccount(accountNo) ? "Account deleted" : "Account not found");
        }

        static void HandleViewAccount(Customer customer)
        {
            if (customer.AccountCount == 0)
            {
                Console.WriteLine("No accounts available");
                return;
            }

            Console.WriteLine("Your Accounts:");
            for (int index = 0; index < customer.AccountCount; index++)
            {
                var acc = customer.Accounts[index];
                Console.WriteLine($"Account No: {acc.AccountNumber} | Balance: {acc.Balance}");
            }
        }

        static void HandleDeposit(Customer customer)
        {
            Console.Write("Account number: ");
            int accountNo = int.Parse(Console.ReadLine());

            IAccount account = null;
            for (int index = 0; index < customer.AccountCount; index++) { 
                if (customer.Accounts[index].AccountNumber == accountNo)
                {
                    account = customer.Accounts[index];
                    break;
                }
            }

            if (account == null)
            {
                Console.WriteLine("Invalid account");
                return;
            }

            Console.Write("Amount: ");
            double amount = double.Parse(Console.ReadLine());

            Console.WriteLine(account.Deposit(amount) ? "Deposit completed" : "Cannot deposit");
        }

        static void HandleWithdraw(Customer customer)
        {
            Console.Write("Account number: ");
            int accountNo = int.Parse(Console.ReadLine());

            IAccount account = null;
            for (int index = 0; index < customer.AccountCount; index++)
            {
                if (customer.Accounts[index].AccountNumber == accountNo)
                {
                    account = customer.Accounts[index];
                    break;
                }
            }

            if (account == null)
            {
                Console.WriteLine("Invalid account");
                return;
            }

            Console.Write("Amount: ");
            double amount = double.Parse(Console.ReadLine());

            Console.WriteLine(account.Withdraw(amount)? "Withdrawal successful": "Insufficient balance"
            );
        }

        static void HandleTransfer(Customer customer, IAccountService accountService, ITransferService transferService)
        {
            Console.WriteLine("Your Accounts:");
            foreach (var account in customer.Accounts)
            {
                Console.WriteLine("Account No: " + account.AccountNumber);
            }

            Console.Write("Sender account number: ");
            int senderAccountNo = int.Parse(Console.ReadLine());

            IAccount sender = null;
            foreach (Account account in customer.Accounts)
            {
                if (account.AccountNumber == senderAccountNo)
                {
                    sender = account;
                    break;
                }
            }

            if (sender == null)
            {
                Console.WriteLine("Invalid sender account");
                return;
            }

            Console.Write("Receiver account number: ");
            int receiverAccountNo = int.Parse(Console.ReadLine());

            var receiver = accountService.FindAccountByNumber(receiverAccountNo);

            Console.Write("Amount: ");
            double amount = double.Parse(Console.ReadLine());

            Console.WriteLine(transferService.Transfer(sender, receiver, amount) ? "Transfer successful" : "Transfer failed");
        }
        static void HandleCreateLoan(Customer customer)
        {
            Console.Write("Loan ID: ");
            int loanId = int.Parse(Console.ReadLine());

            Console.Write("Principal: ");
            double principal = double.Parse(Console.ReadLine());

            Console.Write("Interest rate (%): ");
            double rate = double.Parse(Console.ReadLine());

            Console.Write("Tenure (years): ");
            int years = int.Parse(Console.ReadLine());

            Loan loan = new Loan(loanId, principal, rate, years);
            customer.AddLoan(loan);

            Console.WriteLine("Loan created for customer");
        }


        static void HandleLoanCalculation()
        {
            Console.Write("Principal: ");
            double principal = double.Parse(Console.ReadLine());
            Console.Write("Interest rate (%): ");
            double rate = double.Parse(Console.ReadLine());
            Console.Write("Tenure (years): ");
            int years = int.Parse(Console.ReadLine());

            Loan loan = new Loan(1, principal, rate, years);

            Console.WriteLine($"Interest: {loan.CalculateInterest()}");
            Console.WriteLine($"EMI: {loan.CalculateEMI()}");
        }

        static void Main(string[] args)
        {
            Bank bank = new Bank();

            ICustomerService customerService = bank;
            IAccountService accountService = bank;
            ITransferService transferService = new TransferService();
            Customer customer = SelectCustomer(customerService, accountService);
            StartMenu(customer, accountService, transferService);

        }
    }
}
