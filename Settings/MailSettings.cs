namespace SmartSchoolAPI.Settings
{
    /// <summary>
    /// يمثل إعدادات خادم SMTP المستخدم لإرسال البريد الإلكتروني.
    /// يتم تعبئة هذه الخصائص من قسم "MailSettings" في ملف appsettings.json.
    /// </summary>
    public class MailSettings
    {
        public string Mail { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}