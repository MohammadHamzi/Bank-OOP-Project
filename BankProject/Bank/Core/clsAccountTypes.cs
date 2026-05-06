using System;

namespace BankCore.Core
{
    // ============================================================
    //  Subclass 1: Savings Account
    //  الفئة الفرعية الأولى: حساب التوفير
    // ============================================================

    public class clsSavingsAccount : clsBankClient
    {
        // Private field for interest rate
        // حقل خاص لنسبة الفائدة
        private readonly float _interestRate;

        // Constructor
        // المُنشئ
        public clsSavingsAccount(
            string firstName, string lastName,
            string email, string phone,
            string accountNumber, string pinCode,
            float accountBalance, float interestRate)
            : base(enMode.AddNewMode, firstName, lastName, email, phone,
                   accountNumber, pinCode, accountBalance)
        {
            _interestRate = interestRate;
        }

        // Return account type (Polymorphism)
        // إرجاع نوع الحساب (تعدد الأشكال)
        public override string GetEntityType() => "SavingsAccount";

        // Return account summary
        // إرجاع ملخص الحساب
        public override string GetSummary()
        {
            float annualInterest = CalculateAnnualInterest();

            return $"[Savings Account] {FullName} | Account: {AccountNumber} " +
                   $"| Balance: {AccountBalance:F2} | Annual Interest: {_interestRate}% " +
                   $"= {annualInterest:F2} USD";
        }

        // Calculate annual interest
        // حساب الفائدة السنوية
        public float CalculateAnnualInterest()
            => AccountBalance * (_interestRate / 100f);
// Override withdraw behavior for savings accounts
// إعادة تعريف السحب لحسابات التوفير

public override bool Withdraw(float amount)
{
    // Savings accounts must keep at least 100 USD
    // حساب التوفير يجب أن يحتفظ بحد أدنى 100 دولار

    if (AccountBalance - amount < 100)
        return false;

    return base.Withdraw(amount);
}

        public override string ToString() => GetSummary();
    }

    // ============================================================
    //  Subclass 2: Loan Account
    //  الفئة الفرعية الثانية: حساب القرض
    // ============================================================

    public class clsLoanAccount : clsBankClient
    {
        private readonly float _loanAmount;
        private readonly float _loanInterestRate;
        private readonly int _remainingMonths;

        // Constructor
        // المُنشئ
        public clsLoanAccount(
            string firstName, string lastName,
            string email, string phone,
            string accountNumber, string pinCode,
            float accountBalance,
            float loanAmount, float loanInterestRate, int remainingMonths)
            : base(enMode.AddNewMode, firstName, lastName, email, phone,
                   accountNumber, pinCode, accountBalance)
        {
            _loanAmount = loanAmount;
            _loanInterestRate = loanInterestRate;
            _remainingMonths = remainingMonths;
        }

        // Return account type
        // إرجاع نوع الحساب
        public override string GetEntityType() => "LoanAccount";

        // Return loan summary
        // إرجاع ملخص القرض
        public override string GetSummary()
        {
            float monthlyPayment = CalculateMonthlyPayment();
            float totalRemaining = monthlyPayment * _remainingMonths;

            return $"[Loan Account] {FullName} | Account: {AccountNumber} " +
                   $"| Loan Amount: {_loanAmount:F2} | Rate: {_loanInterestRate}% " +
                   $"| Monthly Payment: {monthlyPayment:F2} | Months Left: {_remainingMonths} " +
                   $"| Total Remaining: {totalRemaining:F2} USD";
        }

        // Calculate monthly payment
        // حساب القسط الشهري
        public float CalculateMonthlyPayment()
        {
            if (_remainingMonths == 0)
                return 0;

            float monthlyRate = _loanInterestRate / 100f / 12f;

            if (monthlyRate == 0)
                return _loanAmount / _remainingMonths;

            float factor = (float)Math.Pow(1 + monthlyRate, _remainingMonths);
            return _loanAmount * monthlyRate * factor / (factor - 1);
        }

        public override string ToString() => GetSummary();
    }

    // ============================================================
    //  Subclass 3: Premium Account
    //  الفئة الفرعية الثالثة: الحساب المميز
    // ============================================================

    public class clsPremiumAccount : clsBankClient
    {
        private readonly float _creditLimit;
        private readonly string _membershipTier;

        // Constructor
        // المُنشئ
        public clsPremiumAccount(
            string firstName, string lastName,
            string email, string phone,
            string accountNumber, string pinCode,
            float accountBalance,
            float creditLimit, string membershipTier)
            : base(enMode.AddNewMode, firstName, lastName, email, phone,
                   accountNumber, pinCode, accountBalance)
        {
            _creditLimit = creditLimit;
            _membershipTier = membershipTier;
        }

        // Return account type
        // إرجاع نوع الحساب
        public override string GetEntityType() => "PremiumAccount";

        // Return summary
        // إرجاع ملخص الحساب
        public override string GetSummary()
        {
            float availableCredit = _creditLimit + AccountBalance;

            return $"[Premium Account - {_membershipTier}] {FullName} | Account: {AccountNumber} " +
                   $"| Balance: {AccountBalance:F2} | Credit Limit: {_creditLimit:F2} " +
                   $"| Total Available: {availableCredit:F2} USD";
        }

        public override string ToString() => GetSummary();
    }
}