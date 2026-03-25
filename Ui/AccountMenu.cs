using BankingSystem.Enums;
using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Services;
using BankingSystem.Exceptions;
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

                try
                {
                    _bank.CreateAccount(_customer, type);
                    Console.WriteLine($"{type} account created successfully");
                    return;
                }
                catch (BankingException exception)
                {
                    Console.WriteLine(exception.Message);
                }
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
                try
                {
                    _bank.CreateAccount(_customer, type);
                    Console.WriteLine($"{type} Account created successfully");
                    return;
                }
                catch (BankingException exception)
                {
                    Console.WriteLine(exception.Message);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
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

            try
            {
                _bank.DeleteAccount(_customer, account.AccountNumber);
                Console.WriteLine("Account deleted");
            }
            catch (AccountNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }
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
            try
            {
                return _customer.GetAccountByNumber(accountNo);
            }
            catch (AccountNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
        }
    }
}