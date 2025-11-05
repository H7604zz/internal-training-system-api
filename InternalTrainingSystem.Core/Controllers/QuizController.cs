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
		private readonly IQuizService _service;
		private readonly ICourseService _courseService;
		public QuizController(IQuizService service, ICourseService courseService)
		{
			_service = service;
			_courseService = courseService;
		}

		private string GetUserId()
		{
			var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(uid)) throw new UnauthorizedAccessException("User not authenticated.");
			return uid;
		}

		[HttpGet("{quizId:int}/attempt/{attemptId:int}")]
		public async Task<ActionResult<QuizDetailDto>> GetQuizForAttempt(int quizId, int attemptId, [FromQuery] bool shuffleQuestions = true, [FromQuery] bool shuffleAnswers = true, CancellationToken ct = default)
		{
			var result = await _service.GetQuizForAttemptAsync(quizId, attemptId, GetUserId(), shuffleQuestions, shuffleAnswers, ct);
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpPost("{quizId:int}/start")]
		public async Task<ActionResult<StartQuizResponse>> Start(int quizId, CancellationToken ct)
		{
			var res = await _service.StartAttemptAsync(quizId, GetUserId(), ct);
			return Ok(res);
		}

		[HttpPost("attempt/{attemptId:int}/submit")]
		public async Task<ActionResult<AttemptResultDto>> Submit(int attemptId, [FromBody] SubmitAttemptRequest req, CancellationToken ct)
		{
			var res = await _service.SubmitAttemptAsync(attemptId, GetUserId(), req, ct);
			return Ok(res);
		}

		[HttpGet("attempt/{attemptId:int}/result")]
		public async Task<ActionResult<AttemptResultDto>> Result(int attemptId, CancellationToken ct)
		{
			var res = await _service.GetAttemptResultAsync(attemptId, GetUserId(), ct);
			return Ok(res);
		}

		[HttpGet("{quizId:int}/history")]
		public async Task<ActionResult<PagedResult<AttemptHistoryItem>>> History(int quizId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
		{
			var res = await _service.GetAttemptHistoryAsync(quizId, GetUserId(), page, pageSize, ct);
			return Ok(res);
		}
		[HttpPost("~/api/lesson/{lessonId:int}/quiz/start")]
		public async Task<ActionResult<StartQuizResponse>> StartByLesson(int lessonId, CancellationToken ct)
		{
			var res = await _service.StartAttemptByLessonAsync(lessonId, GetUserId(), ct);
			return Ok(res);
		}

		[HttpPost("~/api/lesson/{lessonId:int}/quiz/attempt/{attemptId:int}/submit")]
		public async Task<ActionResult<AttemptResultDto>> SubmitByLesson(int lessonId, int attemptId, [FromBody] SubmitAttemptRequest req, CancellationToken ct)
		{
			var res = await _service.SubmitAttemptByLessonAsync(lessonId, attemptId, GetUserId(), req, ct);
			return Ok(res);
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
	}
}
