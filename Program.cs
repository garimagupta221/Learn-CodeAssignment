using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Bank bank = new Bank();
            UserInterface userInterface = new UserInterface(bank);
            userInterface.Show();
        }
    }
}