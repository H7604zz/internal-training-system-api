using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepo;
        private readonly IQuizAttemptRepository _attemptRepo;
        private readonly IUserAnswerRepository _userAnswerRepo;
        private readonly ICourseMaterialRepository _lessonRepo;
        private readonly ICourseHistoryRepository _historyRepo;
        private readonly ILessonProgressRepository _lessonProgressRepo;
        private readonly IQuizAttemptRepository _quizAttemptRepo;
        private readonly IUnitOfWork _uow;

        public QuizService(
            IQuizRepository quizRepo,
            IQuizAttemptRepository attemptRepo,
            IUserAnswerRepository userAnswerRepo,
            ICourseMaterialRepository lessonRepo,
            ILessonProgressRepository lessonProgressRepo,
            ICourseHistoryRepository historyRepo,
            IQuizAttemptRepository quizAttemptRepo,
            IUnitOfWork uow)
        {
            _quizRepo = quizRepo;
            _attemptRepo = attemptRepo;
            _userAnswerRepo = userAnswerRepo;
            _lessonRepo = lessonRepo;
            _lessonProgressRepo = lessonProgressRepo;
            _historyRepo = historyRepo;
            _quizAttemptRepo = quizAttemptRepo;
            _uow = uow;
        }

        public async Task<QuizDetailDto?> GetQuizForAttemptAsync(int quizId, int attemptId, string userId, bool shuffleQuestions = false, bool shuffleAnswers = false, CancellationToken ct = default)
        {
            var quiz = await _quizRepo.GetActiveQuizWithQuestionsAsync(quizId, ct);
            if (quiz == null) return null;

            var attempt = await _attemptRepo.GetAttemptAsync(attemptId, userId, ct)
                         ?? throw new InvalidOperationException("Lượt làm bài không hợp lệ.");
            if (attempt.Status == QuizConstants.Status.InProgress
                && attempt.EndTime.HasValue
                && DateTime.UtcNow >= attempt.EndTime.Value)
            {
                throw new InvalidOperationException("Bài làm đã hết thời gian. Hãy submit các câu trả lời đang lưu ở FE.");
            }
            var baseQuestions = quiz.Questions.Where(q => q.IsActive).ToList();

            List<Question> finalQuestions;
            if (shuffleQuestions)
            {
                var rng = new Random(attemptId);
                finalQuestions = baseQuestions.OrderBy(_ => rng.Next()).ToList();
            }
            else
            {
                finalQuestions = baseQuestions
                    .OrderBy(q => q.OrderIndex)
                    .ThenBy(q => q.QuestionId)
                    .ToList();
            }
            int? remainingSeconds = null;
            if (attempt.EndTime.HasValue)
            {
                var diff = (int)(attempt.EndTime.Value - DateTime.UtcNow).TotalSeconds;
                remainingSeconds = diff > 0 ? diff : 0;
            }

            return new QuizDetailDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimit = quiz.TimeLimit,
                MaxAttempts = quiz.MaxAttempts,
                PassingScore = quiz.PassingScore,
                RemainingSeconds = remainingSeconds,
                Questions = finalQuestions.Select(q =>
                {
                    var baseAnswers = q.Answers.Where(a => a.IsActive).ToList();

                    List<Answer> finalAnswers;
                    if (q.QuestionType == QuizConstants.QuestionTypes.Essay)
                    {
                        finalAnswers = new List<Answer>();
                    }
                    else if (shuffleAnswers)
                    {
                        var rng = new Random(attemptId + q.QuestionId);
                        finalAnswers = baseAnswers.OrderBy(_ => rng.Next()).ToList();
                    }
                    else
                    {
                        finalAnswers = baseAnswers
                            .OrderBy(a => a.OrderIndex)
                            .ThenBy(a => a.AnswerId)
                            .ToList();
                    }

                    return new QuestionDto
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        OrderIndex = q.OrderIndex,
                        Answers = finalAnswers.Select(a => new AnswerDto
                        {
                            AnswerId = a.AnswerId,
                            AnswerText = a.AnswerText,
                            OrderIndex = a.OrderIndex
                        }).ToList()
                    };
                }).ToList()
            };
        }

        public async Task<StartQuizResponse> StartAttemptAsync(int quizId, string userId, CancellationToken ct = default)
        {
            var quiz = await _quizRepo.GetActiveQuizWithQuestionsAsync(quizId, ct);
            if (quiz == null) throw new InvalidOperationException("Không tìm thấy bài kiểm tra hoặc bài kiểm tra đã bị vô hiệu hóa.");

            var currentCount = await _attemptRepo.CountAttemptsAsync(quizId, userId, ct);
            var nextAttemptNumber = currentCount + 1;

            if (nextAttemptNumber > quiz.MaxAttempts)
                throw new InvalidOperationException("Bạn đã vượt quá số lần làm bài tối đa.");

            var maxScore = quiz.Questions.Where(x => x.IsActive).Sum(x => x.Points);

            var now = DateTime.UtcNow;
            var end = quiz.TimeLimit > 0 ? now.AddMinutes(quiz.TimeLimit) : (DateTime?)null;

            var attempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = userId,
                AttemptNumber = nextAttemptNumber,
                StartTime = now,
                EndTime = end,
                Status = QuizConstants.Status.InProgress,
                Score = 0,
                MaxScore = maxScore,
                Percentage = 0,
                IsPassed = false
            };

            attempt = await _attemptRepo.AddAttemptAsync(attempt, ct);

            await _historyRepo.AddHistoryAsync(new CourseHistory
            {
                Action = CourseAction.QuizStarted,
                ActionDate = DateTime.UtcNow,
                UserId = userId,
                QuizId = quiz.QuizId,
                QuizAttemptId = attempt.AttemptId,
                Description = $"Bắt đầu lượt làm bài #{nextAttemptNumber} cho bài kiểm tra '{quiz.Title}'."
            }, ct);
            await _uow.SaveChangesAsync(ct);

            return new StartQuizResponse
            {
                AttemptId = attempt.AttemptId,
                AttemptNumber = nextAttemptNumber,
                StartTimeUtc = attempt.StartTime,
                EndTimeUtc = end,
                TimeLimitMinutes = quiz.TimeLimit
            };
        }

        public async Task<AttemptResultDto> SubmitAttemptAsync(int attemptId, string userId, SubmitAttemptRequest req, CancellationToken ct = default)
        {
            var attempt = await _attemptRepo.GetAttemptAsync(attemptId, userId, ct)
                         ?? throw new InvalidOperationException("Không tìm thấy lượt làm bài.");
            if (attempt.Status != QuizConstants.Status.InProgress)
                throw new InvalidOperationException("Lượt làm bài này đã được nộp hoặc đã kết thúc.");

            var quiz = await _quizRepo.GetActiveQuizWithQuestionsAsync(attempt.QuizId, ct)
                       ?? throw new InvalidOperationException("Không tìm thấy bài kiểm tra hoặc đã bị vô hiệu hóa.");

            var now = DateTime.UtcNow;
            var isTimedOut = attempt.EndTime.HasValue && now >= attempt.EndTime.Value;

            var questions = quiz.Questions.Where(q => q.IsActive).OrderBy(q => q.OrderIndex).ToList();
            var qMap = questions.ToDictionary(q => q.QuestionId);

            int totalScore = 0;
            var toInsert = new List<UserAnswer>();

            foreach (var item in req.Answers)
            {
                if (!qMap.TryGetValue(item.QuestionId, out var q)) continue;

                if (q.QuestionType == QuizConstants.QuestionTypes.Essay)
                {
                    toInsert.Add(new UserAnswer
                    {
                        AttemptId = attempt.AttemptId,
                        QuestionId = q.QuestionId,
                        AnswerId = null,
                        AnswerText = item.EssayText
                    });
                }
                else
                {
                    var selected = (item.SelectedAnswerIds ?? new List<int>()).Distinct().ToList();
                    foreach (var ansId in selected)
                    {
                        toInsert.Add(new UserAnswer
                        {
                            AttemptId = attempt.AttemptId,
                            QuestionId = q.QuestionId,
                            AnswerId = ansId
                        });
                    }

                    var correctIds = q.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerId).OrderBy(x => x).ToList();
                    var selectedSorted = selected.OrderBy(x => x).ToList();
                    if (selectedSorted.SequenceEqual(correctIds))
                        totalScore += q.Points;
                }
            }

            if (toInsert.Count > 0)
                await _userAnswerRepo.AddRangeAsync(toInsert, ct);

            // Nếu quá giờ -> vẫn chấm điểm, nhưng Status = TimedOut.
            var status = isTimedOut ? QuizConstants.Status.TimedOut : QuizConstants.Status.Completed;

            // EndTime: nếu đã có EndTime (deadline) thì giữ lại; nếu chưa có thì set now
            attempt.EndTime = attempt.EndTime ?? now;
            attempt.Score = totalScore;
            attempt.Percentage = attempt.MaxScore > 0 ? (totalScore * 100.0 / attempt.MaxScore) : 0;
            attempt.IsPassed = attempt.Percentage >= quiz.PassingScore;
            attempt.Status = status;

            await _uow.SaveChangesAsync(ct);

            await _historyRepo.AddHistoryAsync(new CourseHistory
            {
                Action = CourseAction.QuizCompleted,
                ActionDate = DateTime.UtcNow,
                UserId = userId,
                CourseId = quiz.CourseId,
                QuizId = quiz.QuizId,
                QuizAttemptId = attempt.AttemptId,
                Description = isTimedOut
                    ? $"(Timed out) Nộp bài với {attempt.Score}/{attempt.MaxScore} điểm ({attempt.Percentage:F1}%)."
                    : $"Hoàn thành lượt làm bài #{attempt.AttemptNumber}: {attempt.Score}/{attempt.MaxScore} điểm ({attempt.Percentage:F1}%)."
            }, ct);

            await _historyRepo.AddHistoryAsync(new CourseHistory
            {
                Action = attempt.IsPassed ? CourseAction.QuizPassed : CourseAction.QuizFailed,
                ActionDate = DateTime.UtcNow,
                UserId = userId,
                CourseId = quiz.CourseId,
                QuizId = quiz.QuizId,
                QuizAttemptId = attempt.AttemptId,
                Description = attempt.IsPassed
                    ? (isTimedOut ? $"(Timed out) Đạt bài kiểm tra '{quiz.Title}' với {attempt.Percentage:F1}%."
                                  : $"Đạt bài kiểm tra '{quiz.Title}' với {attempt.Percentage:F1}%.")
                    : (isTimedOut ? $"(Timed out) Không đạt bài kiểm tra '{quiz.Title}' với {attempt.Percentage:F1}%."
                                  : $"Không đạt bài kiểm tra '{quiz.Title}' với {attempt.Percentage:F1}%.")
            }, ct);

            await _uow.SaveChangesAsync(ct);

            var resultQuestions = questions.Select(q =>
            {
                if (q.QuestionType == QuizConstants.QuestionTypes.Essay)
                {
                    var essay = toInsert.FirstOrDefault(x => x.QuestionId == q.QuestionId)?.AnswerText;
                    return new QuestionResultDto
                    {
                        QuestionId = q.QuestionId,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        EarnedPoints = 0,
                        EssayText = essay
                    };
                }
                else
                {
                    var selected = toInsert.Where(x => x.QuestionId == q.QuestionId && x.AnswerId.HasValue)
                                           .Select(x => x.AnswerId!.Value)
                                           .Distinct().OrderBy(x => x).ToList();
                    var correct = q.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerId).OrderBy(x => x).ToList();
                    int earned = selected.SequenceEqual(correct) ? q.Points : 0;

                    return new QuestionResultDto
                    {
                        QuestionId = q.QuestionId,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        EarnedPoints = earned,
                        SelectedAnswerIds = selected,
                        CorrectAnswerIds = correct
                    };
                }
            }).ToList();

            return new AttemptResultDto
            {
                AttemptId = attempt.AttemptId,
                Status = attempt.Status,
                Score = attempt.Score,
                MaxScore = attempt.MaxScore,
                Percentage = attempt.Percentage,
                IsPassed = attempt.IsPassed,
                StartTimeUtc = attempt.StartTime,
                EndTimeUtc = attempt.EndTime,
                Questions = resultQuestions
            };
        }


        public async Task<AttemptResultDto> GetAttemptResultAsync(int attemptId, string userId, CancellationToken ct = default)
        {
            var attempt = await _attemptRepo.GetAttemptAsync(attemptId, userId, ct)
                          ?? throw new InvalidOperationException("Không tìm thấy lượt làm bài.");

            var quiz = await _quizRepo.GetActiveQuizWithQuestionsAsync(attempt.QuizId, ct)
                       ?? throw new InvalidOperationException("Không tìm thấy bài kiểm tra.");

            var userAnswers = await _userAnswerRepo.GetByAttemptAsync(attemptId, ct);

            var qRes = quiz.Questions.Where(q => q.IsActive)
                .OrderBy(q => q.OrderIndex)
                .Select(q =>
                {
                    if (q.QuestionType == QuizConstants.QuestionTypes.Essay)
                    {
                        var essay = userAnswers.FirstOrDefault(ua => ua.QuestionId == q.QuestionId)?.AnswerText;
                        return new QuestionResultDto
                        {
                            QuestionId = q.QuestionId,
                            QuestionType = q.QuestionType,
                            Points = q.Points,
                            EarnedPoints = 0,
                            EssayText = essay
                        };
                    }
                    else
                    {
                        var selected = userAnswers.Where(ua => ua.QuestionId == q.QuestionId && ua.AnswerId.HasValue)
                                                  .Select(ua => ua.AnswerId!.Value)
                                                  .Distinct().OrderBy(x => x).ToList();
                        var correct = q.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerId).OrderBy(x => x).ToList();
                        int earned = selected.SequenceEqual(correct) ? q.Points : 0;

                        return new QuestionResultDto
                        {
                            QuestionId = q.QuestionId,
                            QuestionType = q.QuestionType,
                            Points = q.Points,
                            EarnedPoints = earned,
                            SelectedAnswerIds = selected,
                            CorrectAnswerIds = correct
                        };
                    }
                }).ToList();

            return new AttemptResultDto
            {
                AttemptId = attempt.AttemptId,
                Status = attempt.Status,
                Score = attempt.Score,
                MaxScore = attempt.MaxScore,
                Percentage = attempt.Percentage,
                IsPassed = attempt.IsPassed,
                StartTimeUtc = attempt.StartTime,
                EndTimeUtc = attempt.EndTime,
                Questions = qRes
            };
        }

        public async Task<PagedResult<AttemptHistoryItem>> GetAttemptHistoryAsync(
            int quizId, string userId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            var (items, total) = await _attemptRepo.GetAttemptHistoryAsync(quizId, userId, page, pageSize, ct);

            var results = items.Select(a => new AttemptHistoryItem
            {
                AttemptId = a.AttemptId,
                AttemptNumber = a.AttemptNumber,
                StartTimeUtc = a.StartTime,
                EndTimeUtc = a.EndTime,
                Status = a.Status,
                Score = a.Score,
                MaxScore = a.MaxScore,
                Percentage = a.Percentage,
                IsPassed = a.IsPassed
            }).ToList();

            return new PagedResult<AttemptHistoryItem>
            {
                Items = results,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<StartQuizResponse> StartAttemptByLessonAsync(int lessonId, string userId, CancellationToken ct = default)
        {
            var lesson = await _lessonRepo.GetWithModuleAsync(lessonId, ct)
                         ?? throw new InvalidOperationException("Không tìm thấy bài học.");
            if (lesson.Type != LessonType.Quiz || lesson.QuizId == null)
                throw new InvalidOperationException("Bài học này không phải là bài kiểm tra.");

            await _lessonProgressRepo.EnsureStartedAsync(userId, lessonId, ct);

            var quiz = await _quizRepo.GetActiveQuizWithQuestionsAsync(lesson.QuizId.Value, ct)
                       ?? throw new InvalidOperationException("Không tìm thấy bài kiểm tra hoặc bài kiểm tra đã bị vô hiệu hóa.");

            var currentCount = await _attemptRepo.CountAttemptsAsync(quiz.QuizId, userId, ct);
            var nextAttemptNumber = currentCount + 1;
            if (nextAttemptNumber > quiz.MaxAttempts)
                throw new InvalidOperationException("Bạn đã vượt quá số lần làm bài tối đa.");

            var maxScore = quiz.Questions.Where(q => q.IsActive).Sum(q => q.Points);
            var now = DateTime.UtcNow;
            var end = quiz.TimeLimit > 0 ? now.AddMinutes(quiz.TimeLimit) : (DateTime?)null;

            var attempt = new QuizAttempt
            {
                QuizId = quiz.QuizId,
                UserId = userId,
                AttemptNumber = nextAttemptNumber,
                StartTime = now,
                EndTime = end,
                Status = QuizConstants.Status.InProgress,
                Score = 0,
                MaxScore = maxScore,
                Percentage = 0,
                IsPassed = false
            };

            attempt = await _attemptRepo.AddAttemptAsync(attempt, ct);
            await _uow.SaveChangesAsync(ct);

            await _historyRepo.AddHistoryAsync(new CourseHistory
            {
                Action = CourseAction.QuizStarted,
                ActionDate = DateTime.UtcNow,
                UserId = userId,
                CourseId = lesson.Module.CourseId,
                QuizId = quiz.QuizId,
                QuizAttemptId = attempt.AttemptId,
                Description = $"Bắt đầu lượt làm bài #{nextAttemptNumber} cho bài kiểm tra '{quiz.Title}'."
            }, ct);
            await _uow.SaveChangesAsync(ct);

            return new StartQuizResponse
            {
                AttemptId = attempt.AttemptId,
                AttemptNumber = nextAttemptNumber,
                StartTimeUtc = now,
                EndTimeUtc = end,
                TimeLimitMinutes = quiz.TimeLimit
            };
        }

        public async Task<AttemptResultDto> SubmitAttemptByLessonAsync(int lessonId, int attemptId, string userId, SubmitAttemptRequest req, CancellationToken ct = default)
        {
            var lesson = await _lessonRepo.GetByIdAsync(lessonId, ct)
                         ?? throw new InvalidOperationException("Không tìm thấy bài học.");
            if (lesson.Type != LessonType.Quiz || lesson.QuizId == null)
                throw new InvalidOperationException("Bài học này không phải là bài kiểm tra.");

            var result = await SubmitAttemptAsync(attemptId, userId, req, ct);

            if (result.IsPassed)
            {
                await _lessonProgressRepo.MarkDoneAsync(userId, lessonId, ct);
                await _uow.SaveChangesAsync(ct);
            }

            return result;
        }

        public async Task<QuizDetailDto2> GetDetailAsync(int quizId, CancellationToken ct)
        {
            var dto = await _quizRepo.GetDetailAsync(quizId, ct);
            if (dto is null)
                throw new KeyNotFoundException($"Không tìm thấy bài kiểm tra với ID = {quizId}.");
            return dto;
        }

        public async Task<QuizInfoDto?> GetQuizInfoAsync(int quizId, string userId, CancellationToken ct = default)
        {
            var quiz = await _quizRepo.GetActiveQuizAsync(quizId, ct);
            if (quiz == null) return null;

            var attempts = await _quizAttemptRepo.GetUserAttemptsAsync(quiz.QuizId, userId, ct);

            var submittedAttempts = attempts.Where(a => a.Status == QuizConstants.Status.Completed);

            var info = new QuizInfoDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                TimeLimit = quiz.TimeLimit,
                MaxAttempts = quiz.MaxAttempts,
                PassingScore = quiz.PassingScore,
                UserAttemptCount = attempts.Count,
                HasPassed = submittedAttempts.Any(a => a.IsPassed),
            };

            return info;
        }
    }
}
