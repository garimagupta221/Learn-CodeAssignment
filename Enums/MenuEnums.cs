namespace BankingSystem.Enums
{
    internal enum MainMenuChoice
    {
        AccountManagement = 1,
        Transactions,
        LoanManagement,
        Exit
    }
    internal enum AccountMenuChoice
    {
        CreateAccount = 1,
        DeleteAccount,
        ViewAccountDetails,
        Back
    }
    internal enum TransactionMenuChoice
    {
        Deposit = 1,
        Withdraw,
        Transfer,
        Back
    }
    internal enum LoanMenuChoice
    {
        ApplyLoan = 1,
        ViewLoans,
        ViewLoanDetails,
        Back
    }
}