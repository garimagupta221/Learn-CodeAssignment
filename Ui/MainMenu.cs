using BankingSystem.Enums;
using BankingSystem.Models;
using BankingSystem.Services;
using BankingSystem.Ui;
using System;

namespace BankingSystem.Ui
{
    internal class MainMenu : BaseMenu
    {
        public MainMenu(Bank bank, Customer customer) : base(bank, customer) { }

        public override void Start()
        {
            while (true)
            {
                ShowMenu();
                int input = InputValidator.GetValidChoiceInRange(1, 4);
                MainMenuChoice choice = (MainMenuChoice)input;
                switch (choice)
                {
                    case MainMenuChoice.AccountManagement:
                        new AccountMenu(_bank, _customer).Start();
                        break;

                    case MainMenuChoice.Transactions:
                        new TransactionMenu(_bank, _customer).Start();
                        break;

                    case MainMenuChoice.LoanManagement:
                        new LoanMenu(_bank, _customer).Start();
                        break;

                    case MainMenuChoice.Exit:
                        Console.WriteLine("Exiting application...");
                        return;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        protected override void ShowMenu()
        {
            Console.WriteLine("\nMAIN MENU");
            Console.WriteLine("1. Account Management");
            Console.WriteLine("2. Transactions");
            Console.WriteLine("3. Loan Management");
            Console.WriteLine("4. Exit");
        }
    }
}