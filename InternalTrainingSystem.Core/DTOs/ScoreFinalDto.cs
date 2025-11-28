using InternalTrainingSystem.Core.Common.Constants;

namespace InternalTrainingSystem.Core.DTOs
{
    public class UserScoreDto
    {
        public string UserId { get; set; } = string.Empty;
        public double? Score { get; set; }
    }

    public class ScoreFinalRequest
    {
        public int ClassId { get; set; }
        public bool IsSubmitted { get; set; } = false;
        public List<UserScoreDto> UserScore { get; set; } = new();
    }

    public class UsersInClassResponse
    {
        public List<StaffInClassDto> Users { get; set; } = new();
        public bool IsScoreSubmitted { get; set; }
    }
}
