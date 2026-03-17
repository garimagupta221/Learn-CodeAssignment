using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructure_object
{
    internal class Wallet
    {
        public double GetTotalMoney()
        {
            return _value;
        }

        public void SetTotalMoney(double newValue)
        {
            _value = newValue;
        }

        public void SubtractMoney(double debit)
        {
            _value -= debit;
        }

        public bool Pay(double amount)
        {
            if (_value >= amount)
            {
                _value -= amount;
                return true;
            }
            return false;
        }

        private double _value;
    }
}
