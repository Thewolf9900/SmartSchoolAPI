 using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Challenge
{
    public class SubmitChallengeDto
    {
        public List<ChallengeAnswerDto> Answers { get; set; } = new List<ChallengeAnswerDto>();
         public int TimeTakenSeconds { get; set; }
    }
}