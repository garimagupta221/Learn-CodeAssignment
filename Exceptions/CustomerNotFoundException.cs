namespace BankingSystem.Exceptions
{
    public class CustomerNotFoundException : BankingException
    {
        public int CustomerId { get; }

        public CustomerNotFoundException(int customerId)
            : base($"Customer not found: {customerId}")
        {
            CustomerId = customerId;
        }
    }
}