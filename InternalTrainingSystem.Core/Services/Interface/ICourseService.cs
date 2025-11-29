using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.Core.Common;

namespace InternalTrainingSystem.Core.Services.Interface
{
	public interface ICourseService
	{
		Task<Course?> GetCourseByCourseIdAsync(int? couseId);
		Task<PagedResult<CourseListItemDto>> GetAllCoursesPagedAsync(GetAllCoursesRequest request);
		Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, CancellationToken ct = default);
		Task<bool> DeleteCourseAsync(int id);
		Task<Course> UpdateCourseAsync(int courseId, UpdateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string updatedByUserId, CancellationToken ct = default);
		public bool ToggleStatus(int id, string status);
		Task<bool> UpdatePendingCourseStatusAsync(string userId,int courseId, string newStatus, string? rejectReason = null);
		Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason);
		Task<Course> GetCourseByCourseCodeAsync(string courseCode);
		Task<Course> CreateCourseAsync(CreateCourseMetadataDto meta, IList<IFormFile> lessonFiles,
																				string createdByUserId, CancellationToken ct = default);
		//Staff học course online
        Task<CourseProgressDto> GetCourseProgressAsync(int courseId, string userId, CancellationToken ct = default);
        Task CompleteLessonAsync(int lessonId, string userId, CancellationToken ct = default);
        Task<CourseLearningDto> GetCourseLearningAsync(int courseId,string userId,CancellationToken ct = default);
    }
}
