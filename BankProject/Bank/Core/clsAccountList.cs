using System;
using System.Collections.Generic;

namespace BankCore.Core
{
    public class clsAccountsList
    {
        // TODO: Implement account list logic
        // ======== Main List ========
        // القائمة الرئيسية التي تحتوي على حسابات من النوع الأساسي clsBankClient
        private readonly List<clsBankClient> _accounts;

        // ======== Constructor ========
        // المُنشئ: يقوم بتهيئة قائمة الحسابات
        public clsAccountsList()
        {
            _accounts = new List<clsBankClient>();
        }

        // ======== Add Accounts ========
        // إضافة حساب إلى القائمة بعد التأكد أنه غير فارغ
        public void AddAccount(clsBankClient account)
        {
            if (account != null && !account.IsEmpty())
                _accounts.Add(account);
        }

        // Returns the number of accounts in the list.
        // يعيد عدد الحسابات الموجودة في القائمة.
        public int Count => _accounts.Count;

        // ======================================================
        //  Polymorphism - Uniform Processing
        //  تعدد الأشكال - معالجة موحّدة لأنواع حسابات مختلفة
        // ======================================================

        public void PrintAllSummaries()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("  Accounts List - Polymorphism Demonstration");
            Console.WriteLine(new string('=', 60));

            foreach (clsBankClient account in _accounts)
            {
                // Polymorphism in action: same call produces different output per type.
                // تطبيق تعدد الأشكال: نفس الاستدعاء يعطي نتيجة مختلفة حسب نوع الحساب الحقيقي.
                Console.WriteLine($"\nAccount Type : {account.GetEntityType()}");

                // Each derived class returns its own implementation.
                // كل فئة مشتقة تعيد تنفيذها الخاص لهذه الدالة.
                Console.WriteLine(account.GetSummary());

                Console.WriteLine(new string('-', 60));
            }
        }

        // Calculates the total balance of all accounts.
        // يحسب مجموع أرصدة جميع الحسابات.
        public float GetTotalBalance()
        {
            float total = 0;

            foreach (clsBankClient account in _accounts)
                total += account.AccountBalance;

            return total;
        }

        // Counts how many accounts exist from each account type.
        // يحسب عدد الحسابات الموجودة من كل نوع حساب.
        public Dictionary<string, int> GetAccountTypesSummary()
        {
            var summary = new Dictionary<string, int>();

            foreach (clsBankClient account in _accounts)
            {
                // GetEntityType is overridden by derived classes.
                // هذه الدالة يتم إعادة تعريفها داخل الفئات المشتقة.
                string type = account.GetEntityType();

                if (!summary.ContainsKey(type))
                    summary[type] = 0;

                summary[type]++;
            }

            return summary;
        }

        // Prints a complete report for all account types.
        // يطبع تقريرًا كاملًا عن جميع أنواع الحسابات.
        public void PrintFullReport()
        {
            PrintAllSummaries();

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("  Report Summary:");
            Console.WriteLine($"  Total Accounts : {Count}");
            Console.WriteLine($"  Total Balances : {GetTotalBalance():F2} USD");

            Console.WriteLine("\n  Account Type Breakdown:");

            foreach (var pair in GetAccountTypesSummary())
                Console.WriteLine($"    - {pair.Key}: {pair.Value} account(s)");

            Console.WriteLine(new string('=', 60));
        }

        // Validates all accounts using the same base class reference.
        // يتحقق من جميع الحسابات باستخدام نفس مرجع الفئة الأساسية.
        public void ValidateAll()
        {
            Console.WriteLine("\n--- Validating All Accounts ---");
            int valid = 0, invalid = 0;

            foreach (clsBankClient account in _accounts)
            {
                // Validate is called polymorphically.
                // يتم استدعاء دالة التحقق بأسلوب تعدد الأشكال.
                bool isValid = account.Validate();

                Console.WriteLine($"  {account.AccountNumber} [{account.GetEntityType()}]: " +
                                  (isValid ? "Valid" : "Invalid"));

                if (isValid)
                    valid++;
                else
                    invalid++;
            }

            Console.WriteLine($"  Result: {valid} valid, {invalid} invalid.");
        }
    }
}