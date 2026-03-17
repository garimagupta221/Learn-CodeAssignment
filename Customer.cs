using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure_object
{
    internal class Customer
    {
        public Customer(string firstName, string lastName, Wallet wallet)
        {
            _firstName = firstName;
            _lastName = lastName;
            _myWallet = wallet;
        }

        public string GetFirstName()
        {
            return _firstName;
        }

        public string GetLastName()
        {
            return _lastName;
        }

        public bool Pay(double amount)
        {
            return _myWallet.Pay(amount);
        }

        private string _firstName;
        private string _lastName;
        private Wallet _myWallet;
    }
}
