using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class QuizController : ControllerBase
	{
		private readonly IQuizService _quizService;
		private readonly ICourseService _courseService;
		public QuizController(IQuizService service, ICourseService courseService)
		{
			_quizService = service;
			_courseService = courseService;
		}

		private string GetUserId()
		{
			var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(uid)) throw new UnauthorizedAccessException("User not authenticated.");
			return uid;
		}
        /// <summary>
        /// Lấy thông tin quiz cho 1 attempt cụ thể (dùng khi đang làm bài)
        /// </summary>
        [HttpGet("{quizId:int}/attempt/{attemptId:int}")]
		public async Task<ActionResult<QuizDetailDto>> GetQuizForAttempt(int quizId, int attemptId, [FromQuery] bool shuffleQuestions = true, [FromQuery] bool shuffleAnswers = true, CancellationToken ct = default)
		{
			var result = await _quizService.GetQuizForAttemptAsync(quizId, attemptId, GetUserId(), shuffleQuestions, shuffleAnswers, ct);
			if (result == null) return NotFound();
			return Ok(result);
		}
        /// <summary>
        /// Bắt đầu làm quiz (theo quizId)
        /// </summary>
        [HttpPost("{quizId:int}/start")]
		public async Task<ActionResult<StartQuizResponse>> Start(int quizId, CancellationToken ct)
		{
			var res = await _quizService.StartAttemptAsync(quizId, GetUserId(), ct);
			return Ok(res);
		}
        /// <summary>
        /// Nộp bài làm quiz (theo attemptId)
        /// </summary>
        [HttpPost("attempt/{attemptId:int}/submit")]
		public async Task<ActionResult<AttemptResultDto>> Submit(int attemptId, [FromBody] SubmitAttemptRequest req, CancellationToken ct)
		{
			var res = await _quizService.SubmitAttemptAsync(attemptId, GetUserId(), req, ct);
			return Ok(res);
		}
        /// <summary>
        /// Lấy kết quả attempt (bao gồm Score, MaxScore, Percentage, Status, IsPassed…)
        /// </summary>
        [HttpGet("attempt/{attemptId:int}/result")]
		public async Task<ActionResult<AttemptResultDto>> Result(int attemptId, CancellationToken ct)
		{
			var res = await _quizService.GetAttemptResultAsync(attemptId, GetUserId(), ct);
			return Ok(res);
		}
        /// <summary>
        /// Lấy lịch sử làm quiz của user cho 1 quiz 
        /// </summary>
        [HttpGet("{quizId:int}/history")]
		public async Task<ActionResult<PagedResult<AttemptHistoryItem>>> History(int quizId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
		{
			var res = await _quizService.GetAttemptHistoryAsync(quizId, GetUserId(), page, pageSize, ct);
			return Ok(res);
		}
        /// <summary>
        /// Bắt đầu làm quiz theo lesson (lesson.Type = Quiz)
        /// </summary>
        [HttpPost("~/api/lesson/{lessonId:int}/quiz/start")]
		public async Task<ActionResult<StartQuizResponse>> StartByLesson(int lessonId, CancellationToken ct)
		{
			var res = await _quizService.StartAttemptByLessonAsync(lessonId, GetUserId(), ct);
			return Ok(res);
		}
        /// <summary>
        /// Nộp bài làm quiz theo lesson
        /// </summary>
        [HttpPost("~/api/lesson/{lessonId:int}/quiz/attempt/{attemptId:int}/submit")]
		public async Task<ActionResult<AttemptResultDto>> SubmitByLesson(int lessonId, int attemptId, [FromBody] SubmitAttemptRequest req, CancellationToken ct)
		{
			var res = await _quizService.SubmitAttemptByLessonAsync(lessonId, attemptId, GetUserId(), req, ct);
			return Ok(res);
		}

		/// <summary>
		/// Lấy info quiz theo lesson để Staff xem trước khi làm (MaxAttempts, PassingScore, TimeLimit, RemainingAttempts, IsLocked, HasPassed, BestScore…)
		/// </summary>
		[HttpGet("{quizId:int}/info")]
		public async Task<ActionResult<QuizInfoDto>> GetQuizInfo(int quizId, CancellationToken ct = default)
		{
			var result = await _quizService.GetQuizInfoAsync(quizId, GetUserId(), ct);
			if (result == null) return NotFound();
			return Ok(result);
		}

        /// <summary>
        /// lay ra lich su lam quiz
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="quizId"></param>
        /// <returns></returns>
        [HttpGet("{courseId}/{quizId}/history")]
		public async Task<IActionResult> GetUserQuizHistory(int courseId, int quizId)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			var result = await _courseService.GetUserQuizHistoryAsync(userId, courseId, quizId);
			return Ok(result);
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetDetail(
			[FromRoute] int id,
			CancellationToken ct = default)
		{
			try
			{
				var dto = await _quizService.GetDetailAsync(id, ct);
				return Ok(dto);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}
	}
}
