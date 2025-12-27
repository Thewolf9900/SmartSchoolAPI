using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class ReviewQuestionDto
    {
        [Required(ErrorMessage = "الحالة الجديدة للسؤال مطلوبة.")]
        // نتحقق من أن القيمة المدخلة هي إما Approved أو Rejected فقط
        [EnumDataType(typeof(QuestionStatus), ErrorMessage = "قيمة الحالة غير صالحة.")]
        [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "يمكن فقط الموافقة على السؤال أو رفضه.")]
        public QuestionStatus NewStatus { get; set; }
    }
}