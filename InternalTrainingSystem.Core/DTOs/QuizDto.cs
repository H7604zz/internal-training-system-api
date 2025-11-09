using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.DTOs
{
	public class QuizDetailDto
	{
		public int QuizId { get; set; }
		public string Title { get; set; } = "";
		public string? Description { get; set; }
		public int TimeLimit { get; set; } // minutes
		public int MaxAttempts { get; set; }
		public int PassingScore { get; set; } // percentage
		public IReadOnlyList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
	}

	public class QuestionDto
	{
		public int QuestionId { get; set; }
		public string QuestionText { get; set; } = "";
		public string QuestionType { get; set; } = QuizConstants.QuestionTypes.MultipleChoice;
		public int Points { get; set; }
		public int OrderIndex { get; set; }
		public IReadOnlyList<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
	}

	public class AnswerDto
	{
		public int AnswerId { get; set; }
		public string AnswerText { get; set; } = "";
		public int OrderIndex { get; set; }
	}
	public sealed class QuizDetailDto2
	{
		public int QuizId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int TimeLimit { get; set; }
		public int MaxAttempts { get; set; }
		public int PassingScore { get; set; }
		public int CourseId { get; set; }
		public string CourseName { get; set; } = string.Empty;

		public int AttemptCount { get; set; }

		public List<QuizQuestionDto> Questions { get; set; } = new();
	}

	public sealed class QuizQuestionDto
	{
		public int QuestionId { get; set; }
		public string QuestionText { get; set; } = string.Empty;
		public string QuestionType { get; set; } = string.Empty;
		public int Points { get; set; }
		public int OrderIndex { get; set; }
		public List<QuizAnswerDto> Answers { get; set; } = new();
	}
	public sealed class QuizAnswerDto
	{
		public int AnswerId { get; set; }
		public string AnswerText { get; set; } = string.Empty;
		public bool IsCorrect { get; set; }
		public int OrderIndex { get; set; }
	}

	public class StartQuizResponse
	{
		public int AttemptId { get; set; }
		public int AttemptNumber { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public DateTime? EndTimeUtc { get; set; } // = Start + TimeLimit
		public int TimeLimitMinutes { get; set; }
	}

	public class SubmitAttemptRequest
	{
		// MultipleChoice/TrueFalse: list answerid 
		// Essay: gửi text
		public List<UserAnswerSubmitItem> Answers { get; set; } = new();
	}

	public class UserAnswerSubmitItem
	{
		public int QuestionId { get; set; }
		public List<int>? SelectedAnswerIds { get; set; }
		public string? EssayText { get; set; }
	}

	public class AttemptResultDto
	{
		public int AttemptId { get; set; }
		public string Status { get; set; } = QuizConstants.Status.Completed;
		public int Score { get; set; }
		public int MaxScore { get; set; }
		public double Percentage { get; set; }
		public bool IsPassed { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public DateTime? EndTimeUtc { get; set; }
		public IReadOnlyList<QuestionResultDto> Questions { get; set; } = new List<QuestionResultDto>();
	}

	public class QuestionResultDto
	{
		public int QuestionId { get; set; }
		public string QuestionType { get; set; } = QuizConstants.QuestionTypes.MultipleChoice;
		public int Points { get; set; }
		public int EarnedPoints { get; set; }
		public List<int>? SelectedAnswerIds { get; set; }
		public string? EssayText { get; set; }
		public List<int>? CorrectAnswerIds { get; set; }
	}

	public class AttemptHistoryItem
	{
		public int AttemptId { get; set; }
		public int AttemptNumber { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public DateTime? EndTimeUtc { get; set; }
		public string Status { get; set; } = "";
		public int Score { get; set; }
		public int MaxScore { get; set; }
		public double Percentage { get; set; }
		public bool IsPassed { get; set; }
	}
	public class UserQuizHistoryResponse
	{
		public int QuizId { get; set; }
		public CourseAction Action { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? SubmissionTime { get; set; }
		public double? Score { get; set; }
	}
    public class QuizInfoDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimit { get; set; }     
        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }  
        public int UserAttemptCount { get; set; }
        public int RemainingAttempts => Math.Max(0, MaxAttempts - UserAttemptCount);
        public bool IsLocked => RemainingAttempts <= 0;
        public bool HasPassed { get; set; }
        public int? BestScore { get; set; }
    }
}
