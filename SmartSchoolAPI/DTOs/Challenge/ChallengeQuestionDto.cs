 using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Challenge
{
    public class ChallengeQuestionDto
    {
        public int QuestionId { get; set; }
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public List<ChallengeQuestionOptionDto> Options { get; set; } = new List<ChallengeQuestionOptionDto>();
    }
}