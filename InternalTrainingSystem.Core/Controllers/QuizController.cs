using InternalTrainingSystem.Core.Common;
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
			if (string.IsNullOrEmpty(uid)) throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
			return uid;
		}
        /// <summary>
        /// Lấy thông tin quiz cho 1 attempt cụ thể (dùng khi đang làm bài)
        /// </summary>
        [HttpGet("{quizId:int}/attempt/{attemptId:int}")]
		public async Task<ActionResult<QuizDetailDto>> GetQuizForAttempt(int quizId, int attemptId, [FromQuery] bool shuffleQuestions = true, [FromQuery] bool shuffleAnswers = true, CancellationToken ct = default)
		{
			try
			{
				var result = await _quizService.GetQuizForAttemptAsync(quizId, attemptId, GetUserId(), shuffleQuestions, shuffleAnswers, ct);
				if (result == null) return NotFound();
				return Ok(result);
			}
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Bài làm đã hết thời gian"))
            {
                return StatusCode(409, new { code = "QUIZ_TIMED_OUT", message = ex.Message });
            }
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
        /// Bắt đầu làm quiz theo lesson (lesson.Type = Quiz)
        /// </summary>
        [HttpPost("start/lesson/{lessonId:int}")]
		public async Task<ActionResult<StartQuizResponse>> StartByLesson(int lessonId, CancellationToken ct)
		{
			var res = await _quizService.StartAttemptByLessonAsync(lessonId, GetUserId(), ct);
			return Ok(res);
		}
        /// <summary>
        /// Nộp bài làm quiz theo lesson
        /// </summary>
        [HttpPost("submit/lesson/{lessonId:int}/attempt/{attemptId:int}")]
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

			var result = await _quizService.GetUserQuizHistoryAsync(userId, courseId, quizId);
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
