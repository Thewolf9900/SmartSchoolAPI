namespace SmartSchoolAPI.Enums
{
    public enum RegistrationStatus
    {
        PendingReview,    // قيد المراجعة (الحالة الأولية)
        AwaitingPayment,  // في انتظار الدفع (بعد موافقة المدير المبدئية)
        PaymentSubmitted, // تم إرسال إشعار الدفع (بانتظار تدقيق المدير)
        Approved,         // تمت الموافقة (تم إنشاء حساب الطالب)
        ReceiptRejected,  // الإيصال مرفوض 

        Rejected          // تم الرفض
    }
}