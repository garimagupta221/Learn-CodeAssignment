using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Ui
{
    internal abstract class BaseMenu
    {
        protected readonly Bank _bank;
        protected readonly Customer _customer;

        protected BaseMenu(Bank bank, Customer customer)
        {
            _bank = bank;
            _customer = customer;
        }

        public abstract void Start();
        protected abstract void ShowMenu();
    }
}
