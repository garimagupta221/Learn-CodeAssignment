using BankingSystem.Enums;
using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Services;
using System;

namespace BankingSystem.Ui
{
    internal class TransactionMenu : BaseMenu
    {
        public TransactionMenu(Bank bank, Customer customer)
            : base(bank, customer)
        {
        }

        public override void Start()
        {
            if (_customer.AccountCount == 0)
            {
                Console.WriteLine("No accounts available.");
                return;
            }

            while (true)
            {
                ShowMenu();
                int input = InputValidator.GetValidChoiceInRange(1, 4);
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
                }
            }
        }

        protected override void ShowMenu()
        {
            Console.WriteLine("\n1. Deposit");
            Console.WriteLine("2. Withdraw");
            Console.WriteLine("3. Transfer");
            Console.WriteLine("4. Back");
        }
        
        private void HandleDeposit()
        {
            ShowCustomerAccounts();
            var account = SelectAccount();

            if (account == null)
            {
                return;
            }

            decimal amount = InputValidator.GetValidDecimal("Amount: ");
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

            decimal amount = InputValidator.GetValidDecimal("Amount: ");
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

            int receiverAccountNo = InputValidator.GetValidPositiveInt("Enter Receiver account number: ");

            var receiver = _bank.FindAccount(receiverAccountNo);
            if (receiver == null)
            {
                Console.WriteLine("Receiver account not found");
                return;
            }

            decimal amount = InputValidator.GetValidDecimal("Amount: ");
            Console.WriteLine(_bank.Transfer(sender, receiver, amount) ? "Transfer successful" : "Transfer failed");
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
                Console.WriteLine(
                    $"Account No: {acc.AccountNumber} | Type: {acc.AccountType} | Balance: {acc.Balance}"
                );
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