using BankingSystem.Models;
using BankingSystem.Services;
using BankingSystem.Enums;
using System;

namespace BankingSystem.Ui
{
    internal class CustomerSelector
    {
        public CustomerSelector(Bank bank)
        {
            _bank = bank;
        }

        public Customer SelectCustomer()
        {
            while (true)
            {
                int id = InputValidator.GetValidPositiveInt("Enter Customer Id: ");

                Customer customer = _bank.GetCustomerById(id);
                if (customer != null)
                {
                    return customer;
                }

                string name = InputValidator.GetValidUsername();

                customer = _bank.CreateCustomer(id, name);
                Console.WriteLine("New customer created");
                var accountMenu = new AccountMenu(_bank, customer);
                accountMenu.HandleInitialAccountCreation();

                return customer;
            }
        }

        private readonly Bank _bank;
    }
}