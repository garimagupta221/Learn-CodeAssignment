namespace BankingSystem.Exceptions
{
    public class InvalidAmountException : BankingException
    {
        public decimal Amount { get; }

        public InvalidAmountException(decimal amount)
            : base($"Invalid amount: {amount}")
        {
            Amount = amount;
        }
    }
}