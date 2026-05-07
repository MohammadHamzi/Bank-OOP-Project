using BankCore.Lib;
using System;
using System.Collections.Generic;
using System.IO;

namespace BankCore.Core
{
    public class clsUser : clsPerson
    {
        // ======== Private Fields - Encapsulation ========
        // الحقول الخاصة لتحقيق مبدأ التغليف ومنع الوصول المباشر للبيانات
        private readonly string _userName; // read-only after creation
                                           // اسم المستخدم لا يمكن تغييره بعد إنشاء الكائن
        private string _password;
        private int _permission;

        // ======== Static Property: total user count ========
        // خاصية ساكنة لحساب عدد المستخدمين
        private static int _totalUsersCount = 0;

        public static int TotalUsersCount => _totalUsersCount;

        // ======== Enums ========
        // تعدادات لتحديد وضع الكائن والصلاحيات
        public enum enMode { EmptyMode = 0, UpdateMode = 1, AddNewMode = 2 }

        [Flags]
        public enum enPermissions
        {
            None = 0,
            ListClients = 1,
            AddNewClient = 2,
            DeleteClient = 4,
            UpdateClients = 8,
            FindClient = 16,
            Transactions = 32,
            ManageUsers = 64,
            ShowLoginLog = 128,
            All = -1
        }

        public enMode Mode { get; set; }

        // ======== Constructor ========
        // المُنشئ لتهيئة بيانات مستخدم النظام
        public clsUser(
            enMode mode,
            string firstName, string lastName,
            string email, string phone,
            string userName, string password,
            int permission)
            : base(firstName, lastName, email, phone)
        {
            Mode = mode;
            _userName = userName;
            _password = password;
            _permission = permission;
        }

        // ======== Properties ========
        // الخصائص للتحكم بالوصول إلى بيانات المستخدم

        public string UserName => _userName;

        public string Password
        {
            get { return _password; }
            set
            {
                if (clsValidation.IsStrongPassword(value))
                    _password = value;
                else
                    throw new ArgumentException("Password must be at least 8 characters long.");
            }
        }

        public int Permission
        {
            get { return _permission; }
            set { _permission = value; }
        }

        // ======== Abstract Method Implementations (from clsPerson) ========
        // تنفيذ الدوال المجردة القادمة من الفئة الأساسية clsPerson

        public override string GetEntityId() => _userName;

        public override string GetEntityType() => "SystemUser";

        public override string GetSummary()
            => $"User: {FullName} | Username: {_userName} | Permission: {_permission}";

        // Returns readable user information
        // إرجاع معلومات المستخدم بشكل مقروء

        public string GetUserInfo()
        {
            return $"User: {FullName} | Username: {_userName} | Permissions: {_permission}";
        }

        public override bool Validate()
        {
            return base.Validate()
                && clsValidation.IsValidUsername(_userName)
                && clsValidation.IsStrongPassword(_password);
        }

        // ======== Public Methods ========
        // الدوال العامة الخاصة بالمستخدم

        public bool IsEmpty() => Mode == enMode.EmptyMode;

        public enum enSaveResult
        {
            svFailedEmptyObject = 0,
            svSucceeded = 1,
            svFailedUserExists = 2,
            svFailedUnknownMode = 3
        }

        // Saves the current user depending on object mode
        // حفظ المستخدم الحالي حسب وضع الكائن: إضافة أو تعديل
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
                    if (IsUserExist(_userName))
                        return enSaveResult.svFailedUserExists;

                    _AddNew();
                    Mode = enMode.UpdateMode;
                    return enSaveResult.svSucceeded;

                default:
                    return enSaveResult.svFailedUnknownMode;
            }
        }

        // ======== Static Methods ========
        // دوال ساكنة للتعامل مع المستخدمين بدون الحاجة إلى إنشاء كائن مسبق

        public static clsUser Find(string userName)
        {
            if (!File.Exists(_FilePath))
                return _EmptyUser();

            foreach (var line in File.ReadAllLines(_FilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var user = _LineToUser(line);

                if (!user.IsEmpty() && user.UserName == userName)
                    return user;
            }

            return _EmptyUser();
        }

        public static clsUser Find(string userName, string password)
        {
            if (!File.Exists(_FilePath))
                return _EmptyUser();

            foreach (var line in File.ReadAllLines(_FilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var user = _LineToUser(line);

                if (!user.IsEmpty() && user.UserName == userName && user.Password == password)
                    return user;
            }

            return _EmptyUser();
        }

        public static bool IsUserExist(string userName)
            => !Find(userName).IsEmpty();

        public static List<clsUser> GetUsersList()
        {
            var users = _LoadUsersFromFile();
            _totalUsersCount = users.Count;
            return users;
        }

        // ======== Login Log ========
        // سجل عمليات تسجيل الدخول
        public struct LoginRecord
        {
            public string DateTime;
            public string UserName;
            public int Permission;
        }

        public void RegisterLogin()
        {
            EnsureLoginLogExists();

            string sep = "#//#";
            string record = string.Join(sep,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _userName,
                _permission.ToString());

            try
            {
                File.AppendAllText(_LoginLogPath, record + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to register login: {ex.Message}");
            }
        }

        public static List<LoginRecord> GetLoginLog(string filterByUser = "")
        {
            var records = new List<LoginRecord>();

            if (!File.Exists(_LoginLogPath))
                return records;

            foreach (var line in File.ReadAllLines(_LoginLogPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(new[] { "#//#" }, StringSplitOptions.None);

                if (parts.Length < 3)
                    continue;

                var rec = new LoginRecord
                {
                    DateTime = parts[0],
                    UserName = parts[1],
                    Permission = int.TryParse(parts[2], out int p) ? p : 0
                };

                if (string.IsNullOrEmpty(filterByUser) ||
                    rec.UserName.Equals(filterByUser, StringComparison.OrdinalIgnoreCase))
                {
                    records.Add(rec);
                }
            }

            return records;
        }

        // ======== File Existence Helpers ========
        // دوال مساعدة للتأكد من وجود ملفات البيانات

        public static void EnsureUsersFileExists()
        {
            if (!File.Exists(_FilePath))
            {
                File.WriteAllText(_FilePath, "");
                Console.WriteLine("[Info] Users.txt created.");
            }
        }

        public static void EnsureLoginLogExists()
        {
            if (!File.Exists(_LoginLogPath))
            {
                File.WriteAllText(_LoginLogPath, "");
                Console.WriteLine("[Info] LoginRegister.txt created.");
            }
        }

        // ======== File Paths ========
        // مسارات ملفات البيانات الخاصة بالمستخدمين وسجل الدخول
        private static readonly string _FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users.txt");

        private static readonly string _LoginLogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoginRegister.txt");

        // ======== Private Helper Methods ========
        // دوال مساعدة خاصة لمعالجة التحويل بين النصوص والكائنات

        private static clsUser _EmptyUser()
            => new clsUser(enMode.EmptyMode, "", "", "", "", "", "12345678", 0);

        private static clsUser _LineToUser(string line)
        {
            var p = line.Split(new[] { "#//#" }, StringSplitOptions.None);

            if (p.Length < 7)
                return _EmptyUser();

            return new clsUser(
                enMode.UpdateMode,
                p[0], p[1], p[2], p[3],
                p[4],
                clsUtil.DecryptText(p[5]),
                int.TryParse(p[6], out int perm) ? perm : 0);
        }

        private static string _UserToLine(clsUser u)
            => string.Join("#//#",
                u.FirstName, u.LastName, u.Email, u.Phone,
                u._userName,
                clsUtil.EncryptText(u._password),
                u._permission.ToString());

        private static List<clsUser> _LoadUsersFromFile()
        {
            EnsureUsersFileExists();
            var list = new List<clsUser>();

            foreach (var line in File.ReadAllLines(_FilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var user = _LineToUser(line);

                if (!user.IsEmpty())
                    list.Add(user);
            }

            return list;
        }

        private static void _SaveUsersToFile(List<clsUser> users)
        {
            using (var writer = new StreamWriter(_FilePath))
            {
                foreach (var u in users)
                    writer.WriteLine(_UserToLine(u));
            }
        }

        private void _Update()
        {
            var users = _LoadUsersFromFile();

            foreach (var u in users)
            {
                if (u.UserName == _userName)
                {
                    u.FirstName = this.FirstName;
                    u.LastName = this.LastName;
                    u.Email = this.Email;
                    u.Phone = this.Phone;
                    u._password = this._password;
                    u._permission = this._permission;
                    break;
                }
            }

            _SaveUsersToFile(users);
        }

        private void _AddNew()
        {
            _totalUsersCount++;
            EnsureUsersFileExists();

            using (var writer = new StreamWriter(_FilePath, append: true))
                writer.WriteLine(_UserToLine(this));
        }
    }
}