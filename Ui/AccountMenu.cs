using BankingSystem.Enums;
using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Services;
using System;

namespace BankingSystem.Ui
{
    internal class AccountMenu : BaseMenu
    {
        public AccountMenu(Bank bank, Customer customer) : base(bank, customer) { }
        
        public void HandleInitialAccountCreation()
        {
            Console.WriteLine("Please select account type to open:");

            while (true)
            {
                Console.WriteLine("1. Savings Account");
                Console.WriteLine("2. Current Account");

                int input = InputValidator.GetValidChoiceInRange(1, 2);
                AccountType typeChoice = (AccountType)input;
                string type = typeChoice.ToString();

                if (_bank.CreateAccount(_customer, type))
                {
                    Console.WriteLine($"{type} account created successfully");
                    return;
                }

                Console.WriteLine("Account creation failed. Try again");
            }
        }

        public override void Start()
        {
            while (true)
            {
                ShowMenu();
                int input = InputValidator.GetValidChoiceInRange(1, 4);
                AccountMenuChoice choice = (AccountMenuChoice)input;
                if (choice == AccountMenuChoice.Back)
                {
                    Console.WriteLine("Showing Main Menu");
                    return;
                }

                switch (choice)
                {
                    case AccountMenuChoice.CreateAccount:
                        HandleCreateAccount();
                        break;

                    case AccountMenuChoice.DeleteAccount:
                        HandleDeleteAccount();
                        break;

                    case AccountMenuChoice.ViewAccountDetails:
                        ShowCustomerAccounts();
                        break;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        protected override void ShowMenu()
        {
            Console.WriteLine("\n1. Create Account");
            Console.WriteLine("2. Delete Account");
            Console.WriteLine("3. View Accounts");
            Console.WriteLine("4. Back");
        }

        private void HandleCreateAccount()
        {
            while (true)
            {
                Console.WriteLine("\nSelect Account Type");
                Console.WriteLine("1. Savings Account");
                Console.WriteLine("2. Current Account");
                Console.WriteLine("3. Back");

                int input = InputValidator.GetValidChoiceInRange(1, 3);
                AccountType typeChoice = (AccountType)input;

                if (typeChoice == AccountType.Back)
                {
                    return;
                }

                string type = typeChoice.ToString();
                bool created = _bank.CreateAccount(_customer, type);

                if (created)
                {
                    Console.WriteLine($"{type} Account created successfully");
                    return;
                }

                Console.WriteLine("Account creation failed. Try again.");
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

        private void ShowCustomerAccounts()
        {
            if (_customer.AccountCount == 0)
            {
                Console.WriteLine("No accounts available.");
                return;
            }
            Console.WriteLine("Your Accounts:");
            foreach (var acc in _customer.Accounts)
            {
                Console.WriteLine($"Acc No: {acc.AccountNumber} | Type: {acc.AccountType} | Balance: {acc.Balance}");
            }
        }
        
        private IAccount SelectAccount()
        {
            int accountNo = InputValidator.GetValidPositiveInt("Enter account number: ");
            var account = _customer.GetAccountByNumber(accountNo);
            if (account == null)
            {
                Console.WriteLine("Account doesn't exists");
                return null;
            }

            return account;
        }
    }
}