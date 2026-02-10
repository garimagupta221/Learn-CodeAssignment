using BankingSystem.Enums;
using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Services;
using System;
using System.Linq;
namespace BankingSystem
{
    internal class UserInterface
    {
        public UserInterface(Bank bank)
        {
            _bank = bank;
        }

        public void Show()
        {
            _customer = SelectCustomer();
            StartMenu();
        }

        private Customer SelectCustomer()
        {
            while (true)
            {
                Console.Write("Enter integral Customer Id: ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Invalid customer id");
                    continue;
                }

                Customer customer = _bank.GetCustomerById(id);
                if (customer != null)
                {
                    return customer;
                }

                Console.Write("Enter Customer UserName: ");
                string name = Console.ReadLine();

                customer = _bank.CreateCustomer(id, name);
                _bank.CreateAccount(customer);
                return customer;
            }
        }

        private void StartMenu()
        {
            while (true)
            {
                ShowMainMenu();

                int input = GetValidChoice(1, 4);
                MainMenuChoice choice = (MainMenuChoice)input;
                switch (choice)
                {
                    case MainMenuChoice.AccountManagement:
                        startAccountMenu();
                        break;

                    case MainMenuChoice.Transactions:
                        startTransactionMenu();
                        break;

                    case MainMenuChoice.LoanManagement:
                        startLoanMenu();
                        break;

                    case MainMenuChoice.Exit:
                        Console.WriteLine("Exiting application");
                        return;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        private void ShowMainMenu()
        {
            Console.WriteLine("\nMAIN MENU");
            Console.WriteLine("1. Account Management");
            Console.WriteLine("2. Transactions");
            Console.WriteLine("3. Loan Management");
            Console.WriteLine("4. Exit");
        }

        private void startAccountMenu()
        {
            while (true)
            {
                showAccountMenu();

                int input = GetValidChoice(1, 4);
                AccountMenuChoice choice = (AccountMenuChoice)input;

                if (choice == AccountMenuChoice.Back)
                {
                    Console.WriteLine("Showing Main Menu");
                    return;
                }

                switch (choice)
                {
                    case AccountMenuChoice.CreateAccount:
                        _bank.CreateAccount(_customer);
                        Console.WriteLine("Account created successfully");
                        break;

                    case AccountMenuChoice.DeleteAccount:
                        HandleDeleteAccount();
                        break;

                    case AccountMenuChoice.ViewAccountDetails:
                        HandleViewAccount();
                        break;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        private void showAccountMenu()
        {
            Console.WriteLine("1. Create Account");
            Console.WriteLine("2. Delete Account");
            Console.WriteLine("3. View Accounts");
            Console.WriteLine("4. Back");
        }

        private void startTransactionMenu()
        {
            if (_customer.AccountCount == 0)
            {
                Console.WriteLine("No accounts available");
                return;
            }

            while (true)
            {
                showTransactionMenu();
                int input = GetValidChoice(1, 4);
                TransactionMenuChoice choice = (TransactionMenuChoice)input;

                if (choice == TransactionMenuChoice.Back)
                {
                    return;
                }

                switch (choice)
                {
                    case TransactionMenuChoice.Deposit:
                        HandleDeposit();
                        break;

                    case TransactionMenuChoice.Withdraw:
                        HandleWithdraw();
                        break;

                    case TransactionMenuChoice.Transfer:
                        HandleTransfer();
                        break;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        private void showTransactionMenu()
        {
            Console.WriteLine("1. Deposit");
            Console.WriteLine("2. Withdraw");
            Console.WriteLine("3. Transfer");
            Console.WriteLine("4. Back");
        }

        private void startLoanMenu()
        {
            while (true)
            {
                showLoanMenu();
                int input = GetValidChoice(1, 4);
                LoanMenuChoice choice = (LoanMenuChoice)input;

                if (choice == LoanMenuChoice.Back)
                {
                    return;
                }

                switch (choice)
                {
                    case LoanMenuChoice.ApplyLoan:
                        HandleCreateLoan();
                        break;

                    case LoanMenuChoice.ViewLoans:
                        HandleViewLoans();
                        break;

                    case LoanMenuChoice.ViewLoanDetails:
                        HandleViewLoanDetails();
                        break;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        private void showLoanMenu()
        {
            Console.WriteLine("1. Apply Loan");
            Console.WriteLine("2. View My Loans");
            Console.WriteLine("3. View Loan Details (EMI / Interest)");
            Console.WriteLine("4. Back");
        }

        private int GetValidChoice(int lowerBound, int upperBound)
        {
            while (true)
            {
                Console.Write($"Enter choice ({lowerBound}-{upperBound}): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= lowerBound && choice <= upperBound)
                {
                    return choice;
                }

                Console.WriteLine($"Invalid choice. Please enter {lowerBound}-{upperBound}");
            }
        }

        private void HandleDeleteAccount()
        {
            ShowCustomerAccounts();

            var account = SelectAccount();

            if (account == null)
            {
                return;
            }

            Console.WriteLine(_bank.DeleteAccount(_customer, account.AccountNumber) ? "Account deleted" : "Account not found");
        }

        private void HandleViewAccount()
        {
            ShowCustomerAccounts();
        }

        private void HandleDeposit()
        {
            ShowCustomerAccounts();

            var account = SelectAccount();

            if (account == null)
            {
                return;
            }

            Console.Write("Amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.WriteLine(account.Deposit(amount) ? "Deposit completed" : "Cannot deposit");
        }

        private void HandleWithdraw()
        {
            ShowCustomerAccounts();

            var account = SelectAccount();

            if (account == null)
            {
                return;
            }

            Console.Write("Amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.WriteLine(account.Withdraw(amount) ? "Withdrawal successful" : "Cannot withdraw");
        }

        private void HandleTransfer()
        {
            ShowCustomerAccounts();

            Console.Write("Enter Sender account details: ");
            var sender = SelectAccount();

            if (sender == null)
            {
                return;
            }

            Console.Write("Enter Receiver account number: ");
            if (!int.TryParse(Console.ReadLine(), out int receiverAccountNo))
            {
                Console.WriteLine("Invalid receiver account number");
                return;
            }

            var receiver = _bank.FindAccount(receiverAccountNo);
            if (receiver == null)
            {
                Console.WriteLine("Receiver account not found");
                return;
            }

            Console.Write("Amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.WriteLine(_bank.Transfer(sender, receiver, amount) ? "Transfer successful" : "Transfer failed");
        }

        private void HandleCreateLoan()
        {
            if (_customer.AccountCount == 0)
            {
                Console.WriteLine("Please create an account first.");
                return;
            }

            Console.WriteLine("Select account for EMI deduction:");

            foreach (var acc in _customer.Accounts)
            {
                Console.WriteLine($"Account No: {acc.AccountNumber}");
            }

            Console.Write("Account number: ");
            var account = SelectAccount();

            if (account == null)
            {
                return;
            }

            Console.Write("Loan amount: ");
            decimal principal = decimal.Parse(Console.ReadLine());

            Console.Write("Tenure (years): ");
            int years = int.Parse(Console.ReadLine());
            int loanId = _loanSequence++;
            var info = new LoanInfo
            {
                Principal = principal,
                TenureYears = years,
                LinkedAccountNumber = account.AccountNumber
            };

            Loan loan = new Loan(loanId, info);
            _customer.AddLoan(loan);

            Console.WriteLine("Loan approved successfully");
            Console.WriteLine($"Loan ID: {loan.LoanId}");
            Console.WriteLine($"EMI: {loan.CalculateEMI()}");
        }

        private void HandleViewLoans()
        {
            if (_customer.Loans.Count == 0)
            {
                Console.WriteLine("No loans found");
                return;
            }

            foreach (var loan in _customer.Loans)
            {
                Console.WriteLine($"LoanId: {loan.LoanId}, Principal: {loan.Principal}, EMI: {loan.CalculateEMI()}");
            }
        }

        private void HandleViewLoanDetails()
        {
            if (_customer.Loans.Count == 0)
            {
                Console.WriteLine("No loans found");
                return;
            }

            Console.Write("Enter Loan ID: ");
            int loanId = int.Parse(Console.ReadLine());
            var loan = _customer.Loans.FirstOrDefault(l => l.LoanId == loanId);

            if (loan == null)
            {
                Console.WriteLine("Invalid Loan ID");
                return;
            }

            Console.WriteLine($"Principal: {loan.Principal}");
            Console.WriteLine($"Interest: {loan.CalculateInterest()}");
            Console.WriteLine($"EMI: {loan.CalculateEMI()}");
            Console.WriteLine($"EMI Account: {loan.LinkedAccountNumber}");
        }

        private void ShowCustomerAccounts()
        {
            if (_customer.AccountCount == 0)
            {
                Console.WriteLine("No accounts available");
                return;
            }

            Console.WriteLine("Your Accounts:");
            foreach (var acc in _customer.Accounts)
            {
                Console.WriteLine($"Account No: {acc.AccountNumber} | Balance: {acc.Balance}");
            }
        }

        private IAccount SelectAccount()
        {
            Console.Write("Enter account number: ");
            if (!int.TryParse(Console.ReadLine(), out int accountNo))
            {
                Console.WriteLine("Invalid input");
                return null;
            }

            var account = _customer.GetAccountByNumber(accountNo);
            if (account == null)
            {
                Console.WriteLine("Account doesn't exists");
                return null;
            }

            return account;
        }

        private readonly Bank _bank;
        private Customer _customer;
        private static int _loanSequence = 1;
    }
}