namespace BankingSystem.Exceptions
{
    public class DuplicateCustomerException : BankingException
    {
        public int CustomerId { get; }

        public DuplicateCustomerException(int customerId)
            : base($"Customer with id {customerId} already exists")
        {
            CustomerId = customerId;
        }
    }
}