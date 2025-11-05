using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ICourseRepository
    {
        Task<Course?> GetCourseByCourseIdAsync(int? couseId);
        Task<PagedResult<CourseListItemDto>> GetAllCoursesPagedAsync(GetAllCoursesRequest request);
        Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, CancellationToken ct = default);
        Task<bool> DeleteCourseAsync(int id);
        Task<Course> UpdateCourseAsync(int courseId, UpdateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string updatedByUserId, CancellationToken ct = default);
        Task<Course> UpdateAndResubmitToPendingAsync(int courseId, UpdateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string updatedByUserId,
                                                                  string? resubmitNote = null, CancellationToken ct = default);
        public bool ToggleStatus(int id, string status);
        Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default);
        Task<bool> UpdatePendingCourseStatusAsync(string userId,int courseId, string newStatus, string? rejectReason = null);
        Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason);
        Task<Course> GetCourseByCourseCodeAsync(string courseCode);
        Task<Course> CreateCourseAsync(CreateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string createdByUserId, 
            CancellationToken ct = default);
    }
}
