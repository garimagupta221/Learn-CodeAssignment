using BankingSystem.Enums;
using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Exceptions;
using System;
using System.Linq;

namespace BankingSystem.Ui
{
    internal class LoanMenu : BaseMenu
    {
        public LoanMenu(Bank bank, Customer customer) : base(bank, customer) { }

        public override void Start()
        {
            while (true)
            {
                ShowMenu();
                int input = InputValidator.GetValidChoiceInRange(1, 4);
                LoanMenuChoice choice = (LoanMenuChoice)input;

                if (choice == LoanMenuChoice.Back)
                {
                    return;
                }

                switch (choice)
                {
                    case LoanMenuChoice.ApplyLoan:
                        HandleCreateLoan();
                        break;

                    case LoanMenuChoice.ViewLoans:
                        HandleViewLoans();
                        break;

                    case LoanMenuChoice.ViewLoanDetails:
                        HandleViewLoanDetails();
                        break;

                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }

        protected override void ShowMenu()
        {
            Console.WriteLine("\nLOAN MENU");
            Console.WriteLine("1. Apply Loan");
            Console.WriteLine("2. View Loans");
            Console.WriteLine("3. View Loan Details");
            Console.WriteLine("4. Back");
        }

        private void HandleCreateLoan()
        {
            if (_customer.AccountCount == 0)
            {
                Console.WriteLine("Please create an account first.");
                return;
            }

            Console.WriteLine("Select account for EMI deduction:");
            foreach (var acc in _customer.Accounts)
            {
                Console.WriteLine($"Account No: {acc.AccountNumber}");
            }

            var account = SelectAccount();
            if (account == null)
            {
                return;
            }

            decimal principal = InputValidator.GetValidDecimal("Loan amount: ");
            int years = InputValidator.GetValidPositiveInt("Tenure (years): ");
            int loanId = _loanSequence++;
            var info = new LoanInfo
            {
                Principal = principal,
                TenureYears = years,
                LinkedAccountNumber = account.AccountNumber
            };

            ShowLoanTypeMenu();
            int input = InputValidator.GetValidChoiceInRange(1, 3);
            LoanType loanType = (LoanType)input;
            if (loanType == LoanType.Back)
            {
                return;
            }

            ILoan loan;
            try
            {
                if (loanType == LoanType.Home)
                {
                    loan = new HomeLoan(loanId, info);
                }
                else
                {
                    loan = new PersonalLoan(loanId, info);
                }

                _customer.AddLoan(loan);
                account.Deposit(principal);

                Console.WriteLine("Loan approved successfully");
                Console.WriteLine($"Loan ID: {loan.LoanId}");
                Console.WriteLine($"Credited Amount: {principal}");
                Console.WriteLine($"EMI: {loan.CalculateEMI()}");
            }
            catch (BankingException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private IAccount SelectAccount()
        {
            int accountNo = InputValidator.GetValidPositiveInt("Enter account number: ");
            try
            {
                return _customer.GetAccountByNumber(accountNo);
            }
            catch (AccountNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
        }

        private void ShowLoanTypeMenu()
        {
            Console.WriteLine("\nSelect Loan Type");
            Console.WriteLine("1. Home Loan");
            Console.WriteLine("2. Personal Loan");
            Console.WriteLine("3. Back");
        }

        private void HandleViewLoans()
        {
            if (_customer.Loans.Count == 0)
            {
                Console.WriteLine("No loans found");
                return;
            }

            foreach (var loan in _customer.Loans)
            {
                Console.WriteLine(
                    $"LoanId: {loan.LoanId} | Type: {loan.LoanType} | Principal: {loan.Principal} | EMI: {loan.CalculateEMI()}"
                );
            }
        }

        private void HandleViewLoanDetails()
        {
            if (_customer.Loans.Count == 0)
            {
                Console.WriteLine("No loans found");
                return;
            }

            int loanId = InputValidator.GetValidPositiveInt("Enter Loan ID: ");
            var loan = _customer.Loans.FirstOrDefault(currentLoan => currentLoan.LoanId == loanId);

            if (loan == null)
            {
                Console.WriteLine("Invalid Loan ID");
                return;
            }

            Console.WriteLine($"Principal: {loan.Principal}");
            Console.WriteLine($"Interest: {loan.CalculateInterest()}");
            Console.WriteLine($"EMI: {loan.CalculateEMI()}");
            Console.WriteLine($"EMI Account: {loan.LinkedAccountNumber}");
        }

        private static int _loanSequence = 1;
    }
}