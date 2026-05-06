using System;

namespace BankCore.Interfaces
{
    // ======== Common Bank Entity Interface ========
    // واجهة مشتركة تمثل الكيانات الأساسية داخل النظام البنكي
    public interface IBankEntity
    {
        // Returns the unique identifier of the entity
        // يعيد المعرف الفريد للكيان
        string GetEntityId();

        // Returns a readable summary of the entity data
        // يعيد ملخصاً مقروءاً لبيانات الكيان
        string GetSummary();

        // Validates the entity data
        // يتحقق من صحة بيانات الكيان
        bool Validate();

        // Returns the actual entity type
        // يعيد نوع الكيان الحقيقي
        string GetEntityType();

        // Deposits money into the account
        // إيداع مبلغ داخل الحساب
        void Deposit(decimal amount);

        // Withdraws money from the account
        // سحب مبلغ من الحساب
        bool Withdraw(decimal amount);

        // Returns current account balance
        // يعيد الرصيد الحالي للحساب
        decimal GetBalance();
    }
}