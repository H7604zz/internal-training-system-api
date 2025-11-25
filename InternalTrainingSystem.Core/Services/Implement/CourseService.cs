using ClosedXML.Excel;
using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Implement;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly ICourseHistoryRepository _courseHistoryRepository;
        private readonly ILessonProgressRepository _lessonProgressRepo;
        private readonly ICertificateService _certificateService;
        private readonly ICourseEnrollmentRepository _courseEnrollmentRepo;

        public CourseService(ICourseRepository courseRepo, ICourseHistoryRepository courseHistoryRepository, ILessonProgressRepository lessonProgressRepository, 
            ICertificateService certificateService, ICourseEnrollmentRepository courseEnrollmentRepo)
        {
            _courseRepo = courseRepo;
            _courseHistoryRepository = courseHistoryRepository;
            _lessonProgressRepo = lessonProgressRepository;
            _certificateService = certificateService;
            _courseEnrollmentRepo = courseEnrollmentRepo;
        }

        public async Task<Course> GetCourseByCourseCodeAsync(string courseCode)
        {
            return await _courseRepo.GetCourseByCourseCodeAsync(courseCode);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            return await _courseRepo.DeleteCourseAsync(id);
        }

        public async Task<Course> UpdateCourseAsync(int courseId, UpdateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string updatedByUserId, CancellationToken ct = default)
        {
            return await _courseRepo.UpdateCourseAsync(courseId, meta, lessonFiles, updatedByUserId, ct = default);
        }

        public bool ToggleStatus(int id, string status)
        {
            return _courseRepo.ToggleStatus(id, status);
        }

        public async Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default)
        {
            return await _courseRepo.SearchAsync(req, ct);
        }

        public async Task<Course?> GetCourseByCourseIdAsync(int? couseId)
        {
            return await _courseRepo.GetCourseByCourseIdAsync(couseId);
        }

        public async Task<PagedResult<CourseListItemDto>> GetAllCoursesPagedAsync(GetAllCoursesRequest request)
        {
            return await _courseRepo.GetAllCoursesPagedAsync(request);
        }

        public async Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, CancellationToken ct = default)
        {
            return await _courseRepo.GetCourseDetailAsync(courseId, ct);
        }

        // Duyệt khóa học - ban giám đốc
        public async Task<bool> UpdatePendingCourseStatusAsync(
            string userId, int courseId, string newStatus, string? rejectReason = null)
        {
            return await _courseRepo.UpdatePendingCourseStatusAsync(userId, courseId, newStatus, rejectReason);
        }

        // Ban giám đốc xóa khóa học đã duyệt
        public async Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason)
        {
            return await _courseRepo.DeleteActiveCourseAsync(courseId, rejectReason);
        }

        public async Task<Course> CreateCourseAsync(CreateCourseMetadataDto meta,
                                                        IList<IFormFile> lessonFiles, string createdByUserId, CancellationToken ct = default)
        {
            return await _courseRepo.CreateCourseAsync(meta, lessonFiles, createdByUserId, ct);
        }

        public async Task<Course> UpdateAndResubmitToPendingAsync(int courseId, UpdateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string updatedByUserId,
                                                                                                                            string? resubmitNote = null, CancellationToken ct = default)
        {
            return await _courseRepo.UpdateAndResubmitToPendingAsync(courseId, meta, lessonFiles, updatedByUserId, resubmitNote = null, ct = default);
        }

        // Staff học course online
        public async Task<CourseOutlineDto> GetOutlineAsync(int courseId, string userId, CancellationToken ct = default)
        {
            var course = await _lessonProgressRepo.GetCourseWithStructureAsync(courseId, ct)
                ?? throw new ArgumentException("Không tìm thấy khóa học.");

            var enrolled = await _lessonProgressRepo.IsEnrolledAsync(courseId, userId, ct);
            if (!enrolled) throw new UnauthorizedAccessException("Bạn chưa được ghi danh vào khóa học này.");

            var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
            var map = await _lessonProgressRepo.GetProgressMapAsync(userId, lessonIds, ct);

            return new CourseOutlineDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Modules = course.Modules.Select(m => new ModuleOutlineDto
                {
                    ModuleId = m.Id,
                    Title = m.Title,
                    OrderIndex = m.OrderIndex,
                    Lessons = m.Lessons.Select(l =>
                    {
                        map.TryGetValue(l.Id, out var lp);
                        return new LessonOutlineDto
                        {
                            LessonId = l.Id,
                            Title = l.Title,
                            OrderIndex = l.OrderIndex,
                            Type = l.Type,
                            IsDone = lp?.IsDone ?? false,
                            CompletedAt = lp?.CompletedAt
                        };
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<CourseProgressDto> GetCourseProgressAsync(int courseId, string userId, CancellationToken ct = default)
        {
            var course = await _lessonProgressRepo.GetCourseWithStructureAsync(courseId, ct)
                ?? throw new ArgumentException("Không tìm thấy khóa học.");

            var enrolled = await _lessonProgressRepo.IsEnrolledAsync(courseId, userId, ct);
            if (!enrolled) throw new UnauthorizedAccessException("Bạn chưa được ghi danh vào khóa học này.");

            var total = await _lessonProgressRepo.CountCourseTotalLessonsAsync(courseId, ct);
            var completed = await _lessonProgressRepo.CountCourseCompletedLessonsAsync(userId, courseId, ct);

            // CompletedAt = max CompletedAt của các lesson đã done
            DateTime? courseCompletedAt = null;
            if (total > 0 && completed >= total)
            {
                var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
                var map = await _lessonProgressRepo.GetProgressMapAsync(userId, lessonIds, ct);
                courseCompletedAt = map.Values.Where(p => p.IsDone).Select(p => p.CompletedAt).Where(d => d.HasValue).DefaultIfEmpty().Max();
            }

            var modules = new List<ModuleProgressDto>();
            foreach (var m in course.Modules)
            {
                var ids = m.Lessons.Select(l => l.Id).ToList();
                var map = await _lessonProgressRepo.GetProgressMapAsync(userId, ids, ct);
                var done = map.Values.Count(p => p.IsDone);
                DateTime? modDoneAt = null;
                if (ids.Count > 0 && done >= ids.Count)
                {
                    modDoneAt = map.Values.Where(p => p.IsDone).Select(p => p.CompletedAt).Where(d => d.HasValue).DefaultIfEmpty().Max();
                }

                modules.Add(new ModuleProgressDto
                {
                    ModuleId = m.Id,
                    ModuleTitle = m.Title,
                    CompletedLessons = done,
                    TotalLessons = ids.Count,
                    CompletedAt = modDoneAt
                });
            }

            return new CourseProgressDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CompletedLessons = completed,
                TotalLessons = total,
                CompletedAt = courseCompletedAt,
                Modules = modules
            };
        }

        public async Task CompleteLessonAsync(int lessonId, string userId, CancellationToken ct = default)
        {
            var lesson = await _lessonProgressRepo.GetLessonWithModuleCourseAsync(lessonId, ct)
                ?? throw new ArgumentException("Không tìm thấy bài học.");

            var enrolled = await _lessonProgressRepo.IsEnrolledAsync(lesson.Module.CourseId, userId, ct);
            if (!enrolled) throw new UnauthorizedAccessException("Bạn chưa được ghi danh vào khóa học này.");

            if (lesson.Type == LessonType.Quiz)
            {
                if (lesson.QuizId is null || lesson.QuizId <= 0)
                    throw new InvalidOperationException("Bài học này là Quiz nhưng chưa được cấu hình Quiz.");

                var passed = await _lessonProgressRepo.HasUserPassedQuizAsync(lesson.QuizId.Value, userId, ct);

                if (!passed)
                    throw new InvalidOperationException(
                        "Bạn cần hoàn thành bài Quiz và đạt yêu cầu (PASS) trước khi đánh dấu hoàn thành."
                    );
            }

            await _lessonProgressRepo.UpsertDoneAsync(userId, lessonId, done: true, ct);
            await _lessonProgressRepo.SaveChangesAsync(ct);

            // Nếu hoàn tất cả lessons -> ghi Completed và cấp chứng chỉ
            var total = await _lessonProgressRepo.CountCourseTotalLessonsAsync(lesson.Module.CourseId, ct);
            var completed = await _lessonProgressRepo.CountCourseCompletedLessonsAsync(userId, lesson.Module.CourseId, ct);
            if (total > 0 && completed >= total)
            {
                // Cập nhật trạng thái CourseEnrollment thành Completed
                var enrollment = await _courseEnrollmentRepo.GetCourseEnrollment(lesson.Module.CourseId, userId);
                if (enrollment != null)
                {
                    enrollment.Status = EnrollmentConstants.Status.Completed;
                    enrollment.CompletionDate = DateTime.UtcNow;
                    await _courseEnrollmentRepo.UpdateCourseEnrollment(enrollment);
                }

                // Tự động cấp chứng chỉ khi hoàn thành 100% khóa học
                try
                {
                    await _certificateService.IssueCertificateAsync(userId, lesson.Module.CourseId);
                }
                catch (Exception)
                {
                    
                }
            }
        }

        public async Task UndoCompleteLessonAsync(int lessonId, string userId, CancellationToken ct = default)
        {
            var lesson = await _lessonProgressRepo.GetLessonWithModuleCourseAsync(lessonId, ct)
                ?? throw new ArgumentException("Không tìm thấy bài học.");

            var enrolled = await _lessonProgressRepo.IsEnrolledAsync(lesson.Module.CourseId, userId, ct);
            if (!enrolled) throw new UnauthorizedAccessException("Bạn chưa được ghi danh vào khóa học này.");

            await _lessonProgressRepo.UpsertDoneAsync(userId, lessonId, done: false, ct);
            await _lessonProgressRepo.SaveChangesAsync(ct);

            await _courseHistoryRepository.AddHistoryAsync(new CourseHistory
            {
                Action = CourseAction.ProgressUpdated,
                ActionDate = DateTime.UtcNow,
                UserId = userId,
                CourseId = lesson.Module.CourseId,
                Description = $"Hủy đánh dấu hoàn thành bài học '{lesson.Title}'."
            }, ct);
            await _lessonProgressRepo.SaveChangesAsync(ct);
        }

        public async Task<CourseLearningDto> GetCourseLearningAsync(
                                                                    int courseId,
                                                                    string userId,
                                                                    CancellationToken ct = default)
        {
            // 1) Load course + module + lesson
            var course = await _lessonProgressRepo.GetCourseWithStructureAsync(courseId, ct)
                ?? throw new ArgumentException("Không tìm thấy khóa học.");

            // 2) Kiểm tra đã ghi danh chưa
            var enrolled = await _lessonProgressRepo.IsEnrolledAsync(courseId, userId, ct);
            if (!enrolled)
                throw new UnauthorizedAccessException("Bạn chưa được ghi danh vào khóa học này.");

            // 3) Lấy progress của từng bài học
            var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
            var progressMap = await _lessonProgressRepo.GetProgressMapAsync(userId, lessonIds, ct);

            // 4) Tính tổng progress khóa học
            var totalLessons = lessonIds.Count;
            var completedLessons = progressMap.Values.Count(p => p.IsDone);
            var progressPercent = totalLessons == 0 ? 0 : completedLessons * 100 / totalLessons;

            // 5) Khởi tạo DTO khóa học
            var dto = new CourseLearningDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseCode = course.Code,
                Description = course.Description,
                Duration = course.Duration,
                Level = course.Level,
                CategoryName = course.CourseCategory?.CategoryName ?? "",
                Progress = progressPercent,
                Modules = new()
            };

            // 6) Module + Lesson
            foreach (var module in course.Modules.OrderBy(m => m.OrderIndex))
            {
                var moduleDto = new ModuleLearningDto
                {
                    ModuleId = module.Id,
                    Title = module.Title,
                    Description = module.Description,
                    OrderIndex = module.OrderIndex,
                    Lessons = new(),
                };

                foreach (var lesson in module.Lessons.OrderBy(l => l.OrderIndex))
                {
                    progressMap.TryGetValue(lesson.Id, out var lp);

                    moduleDto.Lessons.Add(new LessonLearningDto
                    {
                        LessonId = lesson.Id,
                        ModuleId = module.Id,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        Type = lesson.Type.ToString(),
                        OrderIndex = lesson.OrderIndex,
                        ContentUrl = lesson.ContentUrl,
                        AttachmentUrl = lesson.AttachmentUrl,
                        QuizId = lesson.QuizId,
                        IsCompleted = lp?.IsDone ?? false,
                        CompletedDate = lp?.CompletedAt
                    });
                }

                moduleDto.CompletedLessons = moduleDto.Lessons.Count(l => l.IsCompleted);

                dto.Modules.Add(moduleDto);
            }
            return dto;
        }
    }
}
