using System;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class LectureQuizDetailsDto
    {
        public int LectureQuizId { get; set; }
        public string Title { get; set; }
        public int LectureId { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }

        // قائمة تحتوي على كل تفاصيل الأسئلة المرتبطة بهذا الاختبار
        public List<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
    }
}