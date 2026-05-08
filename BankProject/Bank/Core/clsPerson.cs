using BankCore.Interfaces;
using BankCore.Lib;

namespace BankCore.Core
{
    public abstract class clsPerson : IBankEntity
    {
        // ======== Private Fields - Encapsulation ========
        // الحقول الخاصة لتحقيق مبدأ التغليف
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _phone;

        // ======== Constructor ========
        // المُنشئ الأساسي للفئة المجردة
        protected clsPerson(string firstName, string lastName, string email, string phone)
        {
            _firstName = firstName;
            _lastName = lastName;
            _email = email;
            _phone = phone;
        }

        // ======== Properties - Controlled Access ========
        // الخصائص للتحكم بالوصول إلى البيانات

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _firstName = value.Trim();
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _lastName = value.Trim();
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (clsValidation.IsValidEmail(value))
                    _email = value.Trim();
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _phone = value.Trim();
            }
        }

        // Read-only calculated property
        // خاصية محسوبة للقراءة فقط
        public string FullName => $"{_firstName} {_lastName}";

        // ======== Abstract Methods - Must be implemented by derived classes ========
        // دوال مجردة يجب تنفيذها داخل الفئات المشتقة

        public abstract string GetSummary();

        public abstract string GetEntityType();

        // ======== Interface Method Implementation ========
        // تنفيذ دوال الواجهة المشتركة

        public abstract string GetEntityId();

        public abstract void Deposit(decimal amount);

        public abstract bool Withdraw(decimal amount);

        public abstract decimal GetBalance();

        // Common validation for person data
        // تحقق مشترك من بيانات الشخص
        public virtual bool Validate()
        {
            return !string.IsNullOrWhiteSpace(_firstName)
                && !string.IsNullOrWhiteSpace(_lastName)
                && clsValidation.IsValidEmail(_email)
                && !string.IsNullOrWhiteSpace(_phone);
        }

        public override string ToString()
        {
            return $"[{GetEntityType()}] {FullName} | Email: {_email} | Phone: {_phone}";
        }
    }
}