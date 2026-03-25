namespace BankingSystem.Exceptions
{
    public class AccountNotFoundException : BankingException
    {
        public int AccountNumber { get; }

        public AccountNotFoundException(int accountNumber)
            : base($"Account not found: {accountNumber}")
        {
            AccountNumber = accountNumber;
        }
    }
}