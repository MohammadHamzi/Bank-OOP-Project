using System;

namespace BankCore.Lib
{
    public static class clsUtil
    {
        // ======== Encryption / Decryption ========
        // دوال التشفير وفك التشفير

        // NOTE: This is a simple educational encryption method (not secure for real systems)
        // ملاحظة: هذا تشفير تعليمي بسيط وغير مناسب للاستخدام في الأنظمة الحقيقية

        // Simple character-based encryption using key shift
        // تشفير بسيط يعتمد على تحريك الأحرف باستخدام مفتاح
        public static string EncryptText(string text, short key = 2)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)(chars[i] + key);

            return new string(chars);
        }

        // Reverse operation of EncryptText
        // فك التشفير باستخدام نفس المفتاح
        public static string DecryptText(string text, short key = 2)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)(chars[i] - key);

            return new string(chars);
        }

        // Generate random account number
        // توليد رقم حساب عشوائي

        public static string GenerateRandomAccountNumber()
        {
            Random rnd = new Random();

            return "ACC-" + rnd.Next(10000, 99999);
        }
    }
}