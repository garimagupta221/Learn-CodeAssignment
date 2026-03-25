using System;

namespace BankingSystem.Exceptions
{
    public class BankingException : Exception
    {
        public BankingException(string message) : base(message) { }
    }
}