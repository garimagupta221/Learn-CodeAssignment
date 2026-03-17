using DataStructure_object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure_object
{
    internal class Paperboy
    {
        public void CollectPayment(Customer customer, double paymentAmount)
        {
            if (customer.Pay(paymentAmount))
            {
                Console.WriteLine("Payment collected successfully");
            }
            else
            {
                Console.WriteLine("Not enough balance");
            }
        }
    }
}