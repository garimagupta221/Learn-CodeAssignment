namespace BankingSystem.Exceptions
{
    public class AccountReferenceException : BankingException
    {
        public AccountReferenceException(string message)
            : base(message) { }
    }
}