namespace BankingSystem.Exceptions
{
    public class InvalidLoanException : BankingException
    {
        public InvalidLoanException(string message)
            : base(message) { }
    }
}