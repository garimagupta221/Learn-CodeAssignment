using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Interfaces
{
    internal interface ITransferService
    {
        bool Transfer(IAccount sender, IAccount receiver, double amount);
    }
}
