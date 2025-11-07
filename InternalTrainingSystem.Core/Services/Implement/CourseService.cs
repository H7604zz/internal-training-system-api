using ClosedXML.Excel;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
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
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly ICourseHistoryRepository _courseHistoryRepository;

        public CourseService(ICourseRepository courseRepo, IUserService userService,INotificationService notificationService, ICourseHistoryRepository courseHistoryRepository)
        {
            _courseRepo = courseRepo;
            _userService = userService;
            _notificationService = notificationService;
            _courseHistoryRepository = courseHistoryRepository;
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
            // 1. Validate tham số
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Trạng thái mới không hợp lệ.", nameof(newStatus));

            var allowedStatuses = new[]
            {
                CourseConstants.Status.Approve,
                CourseConstants.Status.Reject
            };

            if (!allowedStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException(
                    $"Trạng thái '{newStatus}' không hợp lệ. Chỉ chấp nhận Approve hoặc Reject.");

            // 2. Lấy course từ repository
            var course = await _courseRepo.GetCourseWithDepartmentsAsync(courseId);
            if (course == null)
                return false;

            // 3. Chỉ xử lý khi khoá đang Pending
            if (!course.Status.Equals(CourseConstants.Status.Pending, StringComparison.OrdinalIgnoreCase))
                return false;

            var oldStatus = course.Status;

            // 4. Cập nhật trạng thái khoá học
            if (newStatus.Equals(CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase))
            {
                course.Status = CourseConstants.Status.Approve;
                course.RejectionReason = null;
            }
            else if (newStatus.Equals(CourseConstants.Status.Reject, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(rejectReason))
                    throw new ArgumentException(
                        "Phải cung cấp lý do khi từ chối khóa học.", nameof(rejectReason));

                course.Status = CourseConstants.Status.Reject;
                course.RejectionReason = rejectReason.Trim();
            }

            course.UpdatedDate = DateTime.Now;

            // 5. Ghi lịch sử phê duyệt khoá học
            var history = new CourseHistory
            {
                CourseId = course.CourseId,
                UserId = userId,
                Action = newStatus.Equals(CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase)
                            ? CourseAction.CourseApproved
                            : CourseAction.CourseRejected,
                Description = BuildApprovalDescription(oldStatus, course.Status, rejectReason),
                ActionDate = DateTime.UtcNow,
                EnrollmentId = null,
                QuizId = null,
                QuizAttemptId = null,
                ScheduleId = null
            };

            await _courseRepo.AddCourseHistoryAsync(history);

            // 6. Lưu thay đổi Course + History
            await _courseRepo.SaveChangesAsync();

            // 7. Gửi thông báo cho phòng đào tạo
            await NotifyTrainingDepartmentAsync(course);

            return true;
        }

        private async Task NotifyTrainingDepartmentAsync(Course course)
        {
            // Lấy user thuộc phòng đào tạo
            var trainingUsers = _userService.GetUsersByRole(UserRoles.TrainingDepartment);
            if (trainingUsers == null || !trainingUsers.Any())
                return;

            bool isApproved = course.Status.Equals(
                CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase);

            var notification = new Notification
            {
                CourseId = course.CourseId,
                Type = isApproved
                                ? NotificationType.CourseApproved   // enum bạn tự định nghĩa
                                : NotificationType.CourseRejected,
                Message = isApproved
                                ? $"Khóa học \"{course.CourseName}\" đã được giám đốc phê duyệt."
                                : $"Khóa học \"{course.CourseName}\" đã bị từ chối. Lý do: {course.RejectionReason}",
                SentAt = DateTime.Now,
                EntityType = "Course",
                EntityId = course.CourseId
            };

            await _notificationService.SaveNotificationAsync(notification, trainingUsers.Select(u => u.Id).ToList()
            );
        }

        private static string BuildApprovalDescription(string oldStatus, string newStatus, string? reason)
        {
            if (newStatus.Equals(CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase))
                return $"Khóa học chuyển từ '{oldStatus}' sang APPROVE.";

            if (newStatus.Equals(CourseConstants.Status.Reject, StringComparison.OrdinalIgnoreCase))
                return $"Khóa học bị từ chối (từ '{oldStatus}' sang REJECT). Lý do: {reason}";

            return $"Trạng thái khóa học thay đổi từ '{oldStatus}' sang '{newStatus}'.";
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
			return await _courseRepo.UpdateAndResubmitToPendingAsync(courseId, meta, lessonFiles, updatedByUserId,
																															resubmitNote = null, ct = default);
		}
		public Task<IEnumerable<UserQuizHistoryResponse>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId)
		{
			return _courseHistoryRepository.GetUserQuizHistoryAsync(userId, courseId, quizId);
		}
	}
}
