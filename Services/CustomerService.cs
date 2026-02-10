using BankingSystem.Interfaces;
using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Services
{
    internal class CustomerService : ICustomerService
    {
        public CustomerService()
        {
            customers = new List<Customer>();
        }

        public Customer CreateCustomer(int id, string name)
        {
            for (int index = 0; index < customers.Count; index++)
            {
                if (customers[index].CustomerId == id)
                {
                    return null;
                }
            }

            Customer customer = new Customer(id, name);
            customers.Add(customer);
            return customer;
        }

        public Customer GetCustomerById(int customerId)
        {
            for (int index = 0; index < customers.Count; index++)
            {
                if (customers[index].CustomerId == customerId)
                {
                    return customers[index];
                }
            }

            return null;
        }

        private List<Customer> customers;
    }
}