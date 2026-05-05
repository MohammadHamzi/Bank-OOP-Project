using System;
using System.Text.RegularExpressions;

namespace BankCore.Lib
{
    public static class clsValidation
    {
        // ======== Static Property: validation call counter ========
        // خاصية ساكنة لحساب عدد مرات استدعاء دوال التحقق
        private static int _validationCallCount = 0;

        public static int ValidationCallCount => _validationCallCount;

        // ======== Data Validation Methods ========
        // دوال التحقق من صحة البيانات

        public static bool IsValidEmail(string email)
        {
            _validationCallCount++;

            if (string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(email.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        public static bool IsValidAccountNumber(string accountNumber)
        {
            _validationCallCount++;

            if (string.IsNullOrWhiteSpace(accountNumber))
                return false;

            return Regex.IsMatch(accountNumber.Trim(), @"^A\d{3,10}$");
        }

        public static bool IsPositiveAmount(float amount)
        {
            _validationCallCount++;
            return amount > 0;
        }

        public static bool IsValidUsername(string username)
        {
            _validationCallCount++;

            if (string.IsNullOrWhiteSpace(username))
                return false;

            return Regex.IsMatch(username.Trim(), @"^[a-zA-Z0-9_]{4,20}$");
        }

        public static bool IsStrongPassword(string password)
        {
            _validationCallCount++;

            if (string.IsNullOrWhiteSpace(password))
                return false;

            return password.Length >= 8;
        }

        // ======== Optional Helpers ========

        // Reset validation counter
        // إعادة تعيين عداد التحقق
        public static void ResetValidationCount()
        {
            _validationCallCount = 0;
        }
    }
}