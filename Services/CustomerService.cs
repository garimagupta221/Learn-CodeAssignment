using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Exceptions;
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
            if (id <= 0) {
                throw new ArgumentOutOfRangeException("id", "Customer id must be positive.");
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentOutOfRangeException("name", "Customer name cannot be empty.");
            }

            for (int index = 0; index < customers.Count; index++)
            {
                if (customers[index].CustomerId == id)
                {
                    throw new DuplicateCustomerException(id);
                }
            }

            Customer customer = new Customer(id, name);
            customers.Add(customer);
            return customer;
        }

        public Customer GetCustomerById(int customerId)
        {
            if (customerId <= 0) {
                throw new ArgumentOutOfRangeException("customerId", "Customer id must be positive.");
            }

            for (int index = 0; index < customers.Count; index++)
            {
                if (customers[index].CustomerId == customerId)
                {
                    return customers[index];
                }
            }

            throw new CustomerNotFoundException(customerId);
        }

        private List<Customer> customers;
    }
}