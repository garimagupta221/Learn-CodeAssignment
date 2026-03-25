namespace BankingSystem.Exceptions
{
    public class SameAccountTransferException : BankingException
    {
        public int AccountNumber { get; }

        public SameAccountTransferException(int accountNumber)
            : base($"Cannot transfer to same account: {accountNumber}")
        {
            AccountNumber = accountNumber;
        }
    }
}