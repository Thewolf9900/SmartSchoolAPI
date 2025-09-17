 namespace SmartSchoolAPI.DTOs.Challenge
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }

        public int Score { get; set; }
        public int TimeTakenSeconds { get; set; }
    }
}