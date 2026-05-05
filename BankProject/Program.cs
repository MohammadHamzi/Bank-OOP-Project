using BankCore.Core;
using BankCore.Lib;
using System;
using System.Collections.Generic;

namespace BankApp
{

    class Program
    {
        // The currently logged-in user (null when no one is logged in)
        // المستخدم الحالي المسجّل دخوله، وتكون القيمة null عند عدم وجود مستخدم نشط
        private static clsUser _currentUser = null;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Subscribe event handlers
            // ربط معالجات الأحداث بالأحداث الخاصة بعمليات التحويل والسحب الكبير
            clsBankClient.OnTransferCompleted += clsBankClient.LogTransfer;
            clsBankClient.OnTransferCompleted += _OnTransferDone;
            clsBankClient.OnLargeWithdrawal += _OnLargeWithdrawal;

            // Ensure data files exist on startup
            // التأكد من وجود ملفات البيانات الأساسية عند بدء تشغيل البرنامج
            clsUser.EnsureUsersFileExists();
            clsUser.EnsureLoginLogExists();

            // Outer loop: keeps returning to the Login screen after logout
            // حلقة خارجية تعيد المستخدم إلى شاشة الدخول بعد تسجيل الخروج
            bool exitApp = false;
            while (!exitApp)
            {
                exitApp = _ShowLoginScreen();
            }

            _PrintLine();
            Console.WriteLine("  Goodbye!");
            _PrintLine();
        }

        // ================================================================
        //  LOGIN SCREEN
        //  شاشة تسجيل الدخول
        // ================================================================
        static bool _ShowLoginScreen()
        {
            Console.Clear();
            _PrintHeader("Bank Management System");

            Console.WriteLine("  [1] Login");
            Console.WriteLine("  [2] Register New User");
            Console.WriteLine("  [3] Exit Application");
            _PrintLine();
            Console.Write("  Choose: ");
            string choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    if (_DoLogin())
                        _ShowMainMenu(); // goes to main menu; returns here on logout
                                         // ينتقل إلى القائمة الرئيسية ثم يعود هنا بعد تسجيل الخروج
                    return false;       // stay in login screen
                                        // البقاء ضمن شاشة تسجيل الدخول

                case "2":
                    _DoRegister();
                    return false;

                case "3":
                    return true; // exit the app
                                 // الخروج من التطبيق

                default:
                    _ShowMsg("Invalid choice. Press any key to try again.");
                    return false;
            }
        }

        // ----------------------------------------------------------------
        //  Login logic
        //  منطق تسجيل الدخول
        // ----------------------------------------------------------------
        static bool _DoLogin()
        {
            Console.Clear();
            _PrintHeader("Login");

            Console.Write("  Username : ");
            string userName = Console.ReadLine()?.Trim();

            Console.Write("  Password : ");
            string password = _ReadPassword();

            clsUser user = clsUser.Find(userName, password);

            if (user.IsEmpty())
            {
                _ShowMsg("Invalid username or password. Press any key...");
                return false;
            }

            _currentUser = user;

            // Record this login in LoginRegister.txt
            // تسجيل عملية الدخول الحالية داخل ملف LoginRegister.txt
            _currentUser.RegisterLogin();

            Console.WriteLine($"\n  Welcome, {_currentUser.FullName}!");
            Console.WriteLine("  Press any key to continue...");
            Console.ReadKey();
            return true;
        }

        // ----------------------------------------------------------------
        //  Register logic
        //  منطق إنشاء مستخدم جديد
        // ----------------------------------------------------------------
        static void _DoRegister()
        {
            Console.Clear();
            _PrintHeader("Register New User");

            Console.Write("  First Name : ");
            string firstName = Console.ReadLine()?.Trim();

            Console.Write("  Last Name  : ");
            string lastName = Console.ReadLine()?.Trim();

            Console.Write("  Email      : ");
            string email = Console.ReadLine()?.Trim();

            Console.Write("  Phone      : ");
            string phone = Console.ReadLine()?.Trim();

            Console.Write("  Username   : ");
            string userName = Console.ReadLine()?.Trim();

            if (clsUser.IsUserExist(userName))
            {
                _ShowMsg($"Username '{userName}' already exists. Press any key...");
                return;
            }

            Console.Write("  Password (min 8 chars): ");
            string password = _ReadPassword();

            if (!clsValidation.IsStrongPassword(password))
            {
                _ShowMsg("Password too short (must be at least 8 characters). Press any key...");
                return;
            }

            // Create user with full permissions
            // إنشاء مستخدم جديد بصلاحيات كاملة
            var newUser = new clsUser(
                clsUser.enMode.AddNewMode,
                firstName, lastName, email, phone,
                userName, password,
                (int)clsUser.enPermissions.All);

            var result = newUser.Save();

            if (result == clsUser.enSaveResult.svSucceeded)
                _ShowMsg($"User '{userName}' registered successfully! Press any key...");
            else
                _ShowMsg($"Registration failed ({result}). Press any key...");
        }

        // ================================================================
        //  MAIN MENU  (shown after successful login)
        //  القائمة الرئيسية التي تظهر بعد تسجيل الدخول بنجاح
        // ================================================================
        static void _ShowMainMenu()
        {
            bool logout = false;
            while (!logout)
            {
                Console.Clear();
                _PrintHeader($"Main Menu  —  Logged in as: {_currentUser.UserName}");

                Console.WriteLine("  [1] Client Accounts List");
                Console.WriteLine("  [2] Transfer Funds");
                Console.WriteLine("  [3] My Login History");
                Console.WriteLine("  [4] Account Types Report");
                Console.WriteLine("  [5] Logout");
                _PrintLine();
                Console.Write("  Choose: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": _ShowClientsList(); break;
                    case "2": _ShowTransferScreen(); break;
                    case "3": _ShowLoginLog(); break;
                    case "4": _ShowAccountTypesReport(); break;
                    case "5": logout = true; break;
                    default:
                        _ShowMsg("Invalid choice. Press any key...");
                        break;
                }
            }

            _currentUser = null; // clear session on logout
                                 // مسح بيانات الجلسة عند تسجيل الخروج
            _ShowMsg("You have been logged out. Press any key...");
        }

        // ================================================================
        //  SCREEN 1 — CLIENT ACCOUNTS LIST
        //  الشاشة الأولى — قائمة حسابات العملاء
        // ================================================================
        static void _ShowClientsList()
        {
            Console.Clear();
            _PrintHeader("Client Accounts List");

            List<clsBankClient> clients = clsBankClient.GetClientsList();

            if (clients.Count == 0)
            {
                Console.WriteLine("  No clients found in Clients.txt.");
            }
            else
            {
                // Table header
                // رأس جدول عرض بيانات العملاء
                Console.WriteLine($"  {"#",-4} {"Account",-10} {"Name",-24} {"Balance (USD)",14}");
                _PrintLine();

                int i = 1;
                foreach (clsBankClient c in clients)
                {
                    Console.WriteLine($"  {i,-4} {c.AccountNumber,-10} {c.FullName,-24} {c.AccountBalance,14:F2}");
                    i++;
                }

                _PrintLine();
                Console.WriteLine($"  Total Clients : {clients.Count}");
                Console.WriteLine($"  Total Balances: {clsBankClient.TotalBalances:F2} USD");
            }

            Console.WriteLine("\n  Press any key to go back...");
            Console.ReadKey();
        }

        static void _ShowAccountTypesReport()
        {
            Console.Clear();
            _PrintHeader("Account Types Report - Real Clients");

            List<clsBankClient> clients = clsBankClient.GetClientsList();

            if (clients.Count == 0)
            {
                Console.WriteLine("  No clients found in Clients.txt.");
                Console.WriteLine("\n  Press any key to go back...");
                Console.ReadKey();
                return;
            }

            clsAccountsList accounts = new clsAccountsList();

            // Assign account types cyclically to real clients for demonstrating polymorphism.
            // توزيع أنواع الحسابات بشكل دوري على العملاء الحقيقيين لإظهار مفهوم تعدد الأشكال.
            // This does NOT modify actual client data stored in the system.
            // هذا لا يقوم بتعديل بيانات العملاء الفعلية المخزنة في النظام.
            for (int i = 0; i < clients.Count; i++)
            {
                clsBankClient client = clients[i];

                if (i % 3 == 0)
                {
                    accounts.AddAccount(new clsSavingsAccount(
                        client.FirstName,
                        client.LastName,
                        client.Email,
                        client.Phone,
                        client.AccountNumber,
                        client.PinCode,
                        client.AccountBalance,
                        5));
                }
                else if (i % 3 == 1)
                {
                    accounts.AddAccount(new clsLoanAccount(
                        client.FirstName,
                        client.LastName,
                        client.Email,
                        client.Phone,
                        client.AccountNumber,
                        client.PinCode,
                        client.AccountBalance,
                        10000,
                        8,
                        24));
                }
                else
                {
                    accounts.AddAccount(new clsPremiumAccount(
                        client.FirstName,
                        client.LastName,
                        client.Email,
                        client.Phone,
                        client.AccountNumber,
                        client.PinCode,
                        client.AccountBalance,
                        5000,
                        "Gold"));
                }
            }

            // Print a unified report for all account types using polymorphism.
            // طباعة تقرير موحّد لجميع أنواع الحسابات باستخدام تعدد الأشكال.
            accounts.PrintFullReport();

            // Validate all accounts through the same base type reference.
            // التحقق من جميع الحسابات من خلال نفس النوع الأساسي.
            accounts.ValidateAll();

            Console.WriteLine("\n  Press any key to go back...");
            Console.ReadKey();
        }

        // ================================================================
        //  SCREEN 2 — TRANSFER FUNDS
        //  الشاشة الثانية — تحويل الأموال
        // ================================================================
        static void _ShowTransferScreen()
        {
            Console.Clear();
            _PrintHeader("Transfer Funds");

            Console.Write("  Source Account Number      : ");
            string srcAcc = Console.ReadLine()?.Trim();

            clsBankClient source = clsBankClient.Find(srcAcc);
            if (source.IsEmpty())
            {
                _ShowMsg($"Account '{srcAcc}' not found. Press any key...");
                return;
            }

            Console.Write("  Destination Account Number : ");
            string dstAcc = Console.ReadLine()?.Trim();

            clsBankClient dest = clsBankClient.Find(dstAcc);
            if (dest.IsEmpty())
            {
                _ShowMsg($"Account '{dstAcc}' not found. Press any key...");
                return;
            }

            if (srcAcc == dstAcc)
            {
                _ShowMsg("Source and destination cannot be the same account. Press any key...");
                return;
            }

            Console.WriteLine($"\n  Source      : {source.FullName}  |  Balance: {source.AccountBalance:F2} USD");
            Console.WriteLine($"  Destination : {dest.FullName}");

            Console.Write("\n  Amount to transfer (USD): ");
            string amtStr = Console.ReadLine()?.Trim();

            if (!float.TryParse(amtStr, out float amount) || amount <= 0)
            {
                _ShowMsg("Invalid amount. Press any key...");
                return;
            }

            bool ok = source.Transfer(amount, ref dest, _currentUser.UserName);

            if (ok)
            {
                _PrintLine();
                Console.WriteLine($"  Transfer of {amount:F2} USD completed successfully.");
                Console.WriteLine($"  New Source Balance      : {source.AccountBalance:F2} USD");
                Console.WriteLine($"  New Destination Balance : {dest.AccountBalance:F2} USD");
            }
            else
            {
                Console.WriteLine("  Transfer failed. Insufficient balance.");
            }

            Console.WriteLine("\n  Press any key to go back...");
            Console.ReadKey();
        }

        // ================================================================
        //  SCREEN 3 — LOGIN HISTORY (filtered for current user)
        //  الشاشة الثالثة — سجل تسجيل الدخول الخاص بالمستخدم الحالي
        // ================================================================
        static void _ShowLoginLog()
        {
            Console.Clear();
            _PrintHeader($"Login History for: {_currentUser.UserName}");

            // Pass the current username to get only their entries
            // تمرير اسم المستخدم الحالي لجلب سجلات الدخول الخاصة به فقط
            List<clsUser.LoginRecord> log = clsUser.GetLoginLog(_currentUser.UserName);

            if (log.Count == 0)
            {
                Console.WriteLine("  No login records found.");
            }
            else
            {
                Console.WriteLine($"  {"#",-4} {"Date & Time",-22} {"Username",-18} {"Permission",10}");
                _PrintLine();

                int i = 1;
                foreach (clsUser.LoginRecord rec in log)
                {
                    Console.WriteLine($"  {i,-4} {rec.DateTime,-22} {rec.UserName,-18} {rec.Permission,10}");
                    i++;
                }

                _PrintLine();
                Console.WriteLine($"  Total login entries: {log.Count}");
            }

            Console.WriteLine("\n  Press any key to go back...");
            Console.ReadKey();
        }

        // ================================================================
        //  EVENT HANDLERS
        //  معالجات الأحداث
        // ================================================================

        static void _OnTransferDone(
            clsBankClient src, clsBankClient dst, float amount, string user)
        {
            Console.WriteLine($"\n  ★ [Event] Transfer of {amount:F2} USD by {user}: " +
                              $"{src.AccountNumber} → {dst.AccountNumber}");
        }

        static void _OnLargeWithdrawal(
            clsBankClient client, float amount, float balanceBefore)
        {
            Console.WriteLine($"\n  ⚠ [Event] Large withdrawal on {client.AccountNumber}: " +
                              $"{amount:F2} USD (was {balanceBefore:F2})");
        }

        // ================================================================
        //  HELPERS
        //  دوال مساعدة
        // ================================================================

        static void _PrintLine()
            => Console.WriteLine("  " + new string('─', 58));

        static void _PrintHeader(string title)
        {
            _PrintLine();
            Console.WriteLine($"  {title}");
            _PrintLine();
        }

        static void _ShowMsg(string msg)
        {
            Console.WriteLine($"\n  {msg}");
            Console.ReadKey();
        }

        static string _ReadPassword()
        {
            string pass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(intercept: true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pass;
        }
    }
}