using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure_object
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Wallet wallet = new Wallet();
            wallet.SetTotalMoney(100);

            Customer customer = new Customer("Rahul", "Sharma", wallet);
            Paperboy paperboy = new Paperboy();

            paperboy.CollectPayment(customer, 22);
            paperboy.CollectPayment(customer, 12);
        }
    }
}
