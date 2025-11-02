using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _service;
        public QuizController(IQuizService service) { _service = service; }

        private string GetUserId()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(uid)) throw new UnauthorizedAccessException("User not authenticated.");
            return uid;
        }

        [HttpGet("{quizId:int}/attempt/{attemptId:int}")]
        public async Task<ActionResult<QuizDetailDto>> GetQuizForAttempt(
    int quizId,
    int attemptId,
    [FromQuery] bool shuffleQuestions = true,
    [FromQuery] bool shuffleAnswers = true,
    CancellationToken ct = default)
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
    }
}
