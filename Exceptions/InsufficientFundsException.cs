// namespace BankingSystem.Exceptions
// {
//     internal sealed class InsufficientFundsException : BankingException 
//     {
//         public InsufficientFundsException(int accountNumber, decimal requestedAmount, decimal availableAmount)
//             : base($"Insufficient funds for account {accountNumber}. Requested: {requestedAmount}, Available: {availableAmount}.")
//         {
//             AccountNumber = accountNumber;
//             RequestedAmount = requestedAmount;
//             AvailableAmount = availableAmount;
//         }

//         public int AccountNumber { get; }
//         public decimal RequestedAmount { get; }
//         public decimal AvailableAmount { get; }
//     }
// }

namespace BankingSystem.Exceptions
{
    public class InsufficientFundsException : BankingException
    {
        public int AccountNumber { get; }
        public decimal RequestedAmount { get; }
        public decimal AvailableAmount { get; }

        public InsufficientFundsException(int accountNumber, decimal requested, decimal available)
            : base($"Insufficient funds for account {accountNumber}. Requested: {requested}, Available: {available}")
        {
            AccountNumber = accountNumber;
            RequestedAmount = requested;
            AvailableAmount = available;
        }
    }
}