using BankingSystem.Models;
using BankingSystem.Services;
using BankingSystem.Ui;

namespace BankingSystem.Ui
{
    internal class UserInterface
    {
        public UserInterface(Bank bank)
        {
            _bank = bank;
        }

        public void Show()
        {
            var selector = new CustomerSelector(_bank);
            Customer customer = selector.SelectCustomer();

            var mainMenu = new MainMenu(_bank, customer);
            mainMenu.Start();
        }

        private readonly Bank _bank;
    }
}