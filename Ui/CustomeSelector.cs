using BankingSystem.Models;
using BankingSystem.Services;
using BankingSystem.Enums;
using BankingSystem.Exceptions;
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

                Customer customer;
                try
                {
                    customer = _bank.GetCustomerById(id);
                    return customer;
                }
                catch (CustomerNotFoundException)
                {
                        Console.WriteLine("Customer not found. Creating new customer");
                }

                string name = InputValidator.GetValidUsername();

                try
                {
                    customer = _bank.CreateCustomer(id, name);
                    Console.WriteLine("New customer created");
                    var accountMenu = new AccountMenu(_bank, customer);
                    accountMenu.HandleInitialAccountCreation();

                    return customer;
                }
                catch (DuplicateCustomerException exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        private readonly Bank _bank;
    }
}