namespace BankingSystem.Exceptions
{
    public class InvalidAccountTypeException : BankingException
    {
        public string AccountType { get; }

        public InvalidAccountTypeException(string accountType)
            : base($"Invalid account type: {accountType}")
        {
            AccountType = accountType;
        }
    }
}