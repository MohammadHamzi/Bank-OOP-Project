using BankCore.Lib;
using System;
using System.Collections.Generic;
using System.IO;

namespace BankCore.Core
{
    public class clsBankClient : clsPerson
    {
        // ======== Private Fields - Encapsulation ========
        // الحقول الخاصة لتحقيق مبدأ التغليف ومنع الوصول المباشر للبيانات
        private readonly string _accountNumber; // read-only after creation
                                                // رقم الحساب لا يمكن تغييره بعد إنشاء الكائن
        private string _pinCode;
        private float _accountBalance;

        // ======== Static Property: total client count ========
        // خاصية ساكنة لحساب عدد العملاء
        private static int _totalClientsCount = 0;

        public static int TotalClientsCount => _totalClientsCount;

        // ======== Delegates & Events ========
        // التفويضات والأحداث المستخدمة للتبليغ عن عمليات معينة في النظام

        // Delegate for transfer completion event
        // تفويض خاص بحدث اكتمال عملية التحويل
        public delegate void TransferCompletedHandler(
            clsBankClient sourceClient,
            clsBankClient destinationClient,
            float amount,
            string performedByUser);

        public static event TransferCompletedHandler OnTransferCompleted;

        // Delegate for large withdrawal event
        // تفويض خاص بحدث السحب الكبير
        public delegate void LargeWithdrawalHandler(
            clsBankClient client,
            float amount,
            float balanceBefore);

        public static event LargeWithdrawalHandler OnLargeWithdrawal;

        // ======== Constructor ========
        // المُنشئ وأنماط عمل الكائن
        public enum enMode { EmptyMode = 0, UpdateMode = 1, AddNewMode = 2 }

        public enMode Mode { get; set; }

        public clsBankClient(
            enMode mode,
            string firstName, string lastName,
            string email, string phone,
            string accountNumber, string pinCode,
            float accountBalance)
            : base(firstName, lastName, email, phone)
        {
            Mode = mode;
            _accountNumber = accountNumber; // cannot be changed after creation
                                            // لا يمكن تغيير رقم الحساب بعد إنشاء الكائن
            _pinCode = pinCode;
            _accountBalance = accountBalance;
        }

        // ======== Public Properties ========
        // الخصائص العامة للتحكم بالوصول إلى بيانات الحساب

        // Account number is read-only from outside the class
        // رقم الحساب للقراءة فقط من خارج الفئة
        public string AccountNumber => _accountNumber;

        public string PinCode
        {
            get { return _pinCode; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _pinCode = value;
            }
        }

        // Balance can be read publicly but modified only through class operations
        // يمكن قراءة الرصيد من الخارج، لكن تعديله يتم فقط من خلال عمليات الفئة
        public float AccountBalance
        {
            get { return _accountBalance; }
            private set
            {
                if (value >= 0)
                    _accountBalance = value;
            }
        }

        // ======== Abstract Method Implementations (from clsPerson) ========
        // تنفيذ الدوال المجردة القادمة من الفئة الأساسية clsPerson

        public override string GetEntityId() => _accountNumber;

        public override string GetEntityType() => "BankClient";

        public override string GetSummary()
        {
            return $"Client: {FullName} | Account: {_accountNumber} | Balance: {_accountBalance:F2} USD";
        }

        public override bool Validate()
        {
            return base.Validate()
                && clsValidation.IsValidAccountNumber(_accountNumber)
                && !string.IsNullOrWhiteSpace(_pinCode);
        }

        // ======== Banking Operations ========
        // العمليات البنكية الأساسية على الحساب

        // Deposits a positive amount and saves the updated balance
        // إيداع مبلغ موجب وحفظ الرصيد بعد التعديل
        public void Deposit(float amount)
        {
            if (!clsValidation.IsPositiveAmount(amount))
                throw new ArgumentException("Amount must be positive.");

            AccountBalance += amount;
            Save();
        }

        // Withdraws a valid amount and fires an event for large withdrawals
        // سحب مبلغ صالح وإطلاق حدث عند السحب الكبير
        public bool Withdraw(float amount)
        {
            if (!clsValidation.IsPositiveAmount(amount))
                return false;

            if (amount > _accountBalance)
                return false;

            float balanceBefore = _accountBalance;
            AccountBalance -= amount;
            Save();

            // Fire the large-withdrawal event if amount exceeds 10,000
            // إطلاق حدث السحب الكبير إذا تجاوز المبلغ 10000
            if (amount > 10000f)
                OnLargeWithdrawal?.Invoke(this, amount, balanceBefore);

            return true;
        }

        // Transfers money between two clients and fires a completion event
        // تحويل الأموال بين عميلين وإطلاق حدث عند اكتمال التحويل
        public bool Transfer(float amount, ref clsBankClient destinationClient, string performedByUser)
        {
            if (!clsValidation.IsPositiveAmount(amount))
                return false;

            if (amount > _accountBalance)
                return false;

            Withdraw(amount);
            destinationClient.Deposit(amount);

            // Fire the event after the transfer completes
            // إطلاق الحدث بعد اكتمال عملية التحويل
            OnTransferCompleted?.Invoke(this, destinationClient, amount, performedByUser);

            return true;
        }

        // ======== Data Management ========
        // إدارة بيانات العملاء

        public bool IsEmpty() => Mode == enMode.EmptyMode;

        public enum enSaveResult
        {
            svFailedEmptyObject = 0,
            svSucceeded = 1,
            svFailedAccountNumberExists = 2,
            svFailedUnknownMode = 3
        }

        // Saves the current object depending on its mode
        // حفظ الكائن الحالي حسب وضعه: إضافة جديدة أو تعديل
        public enSaveResult Save()
        {
            switch (Mode)
            {
                case enMode.EmptyMode:
                    return enSaveResult.svFailedEmptyObject;

                case enMode.UpdateMode:
                    _Update();
                    return enSaveResult.svSucceeded;

                case enMode.AddNewMode:
                    if (IsClientExist(_accountNumber))
                        return enSaveResult.svFailedAccountNumberExists;

                    _AddNew();
                    Mode = enMode.UpdateMode;
                    return enSaveResult.svSucceeded;

                default:
                    return enSaveResult.svFailedUnknownMode;
            }
        }

        // ======== Static Methods ========
        // دوال ساكنة للتعامل مع العملاء بدون الحاجة إلى إنشاء كائن مسبق

        // Finds a client by account number
        // البحث عن عميل باستخدام رقم الحساب
        public static clsBankClient Find(string accountNumber)
        {
            if (!File.Exists(_FilePath))
                return _EmptyClient();

            foreach (var line in File.ReadAllLines(_FilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var client = _LineToClient(line);

                if (!client.IsEmpty() && client.AccountNumber == accountNumber)
                    return client;
            }

            return _EmptyClient();
        }

        public static bool IsClientExist(string accountNumber)
            => !Find(accountNumber).IsEmpty();

        // Loads all clients and updates the static client counter
        // تحميل جميع العملاء وتحديث العداد الساكن لعدد العملاء
        public static List<clsBankClient> GetClientsList()
        {
            var clients = _LoadClientsFromFile();
            _totalClientsCount = clients.Count;
            return clients;
        }

        // Calculates the total balance of all saved clients
        // حساب مجموع أرصدة جميع العملاء المخزنين
        public static double TotalBalances
        {
            get
            {
                double total = 0;

                foreach (var c in _LoadClientsFromFile())
                    total += c._accountBalance;

                return total;
            }
        }

        // ======== Transfer Log ========
        // سجل عمليات التحويل

        // Logs a successful transfer to Transferlog.txt
        // تسجيل عملية التحويل الناجحة داخل ملف Transferlog.txt
        public static void LogTransfer(
            clsBankClient src, clsBankClient dest,
            float amount, string user)
        {
            string sep = "#//#";
            string record = string.Join(sep,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                src.AccountNumber,
                dest.AccountNumber,
                amount.ToString("F2"),
                src._accountBalance.ToString("F2"),
                dest._accountBalance.ToString("F2"),
                user);

            try
            {
                File.AppendAllText(_TransferLogPath, record + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to log transfer: {ex.Message}");
            }
        }

        // ======== File Paths ========
        // مسارات ملفات البيانات المستخدمة في النظام
        private static readonly string _FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients.txt");

        private static readonly string _TransferLogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Transferlog.txt");

        // ======== Private Helper Methods ========
        // دوال مساعدة خاصة لمعالجة التحويل بين النصوص والكائنات

        private static clsBankClient _EmptyClient()
            => new clsBankClient(enMode.EmptyMode, "", "", "", "", "", "", 0);

        // Converts a file line into a client object
        // تحويل سطر من الملف إلى كائن عميل
        private static clsBankClient _LineToClient(string line)
        {
            var parts = line.Split(new[] { "#//#" }, StringSplitOptions.None);

            if (parts.Length < 7)
                return _EmptyClient();

            return new clsBankClient(
                enMode.UpdateMode,
                parts[0], parts[1], parts[2], parts[3],
                parts[4],
                clsUtil.DecryptText(parts[5]),
                float.TryParse(parts[6], out float bal) ? bal : 0);
        }

        // Converts a client object into a line suitable for file storage
        // تحويل كائن العميل إلى سطر مناسب للتخزين داخل الملف
        private static string _ClientToLine(clsBankClient c)
            => string.Join("#//#",
                c.FirstName, c.LastName, c.Email, c.Phone,
                c._accountNumber,
                clsUtil.EncryptText(c._pinCode),
                c._accountBalance.ToString("F2"));

        // Loads all clients from Clients.txt
        // تحميل جميع العملاء من ملف Clients.txt
        private static List<clsBankClient> _LoadClientsFromFile()
        {
            var list = new List<clsBankClient>();

            if (!File.Exists(_FilePath))
                return list;

            foreach (var line in File.ReadAllLines(_FilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var client = _LineToClient(line);

                if (!client.IsEmpty())
                    list.Add(client);
            }

            return list;
        }

        // Saves all clients back to Clients.txt
        // حفظ جميع العملاء مرة أخرى داخل ملف Clients.txt
        private static void _SaveClientsToFile(List<clsBankClient> clients)
        {
            using (var writer = new StreamWriter(_FilePath))
            {
                foreach (var c in clients)
                    writer.WriteLine(_ClientToLine(c));
            }
        }

        // Updates the current client data in the file
        // تحديث بيانات العميل الحالي داخل الملف
        private void _Update()
        {
            var clients = _LoadClientsFromFile();

            foreach (var c in clients)
            {
                if (c.AccountNumber == _accountNumber)
                {
                    c.FirstName = this.FirstName;
                    c.LastName = this.LastName;
                    c.Email = this.Email;
                    c.Phone = this.Phone;
                    c._pinCode = this._pinCode;
                    c._accountBalance = this._accountBalance;
                    break;
                }
            }

            _SaveClientsToFile(clients);
        }

        // Adds the current client as a new record
        // إضافة العميل الحالي كسجل جديد
        private void _AddNew()
        {
            _totalClientsCount++;

            using (var writer = new StreamWriter(_FilePath, append: true))
                writer.WriteLine(_ClientToLine(this));
        }
    }
}