using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IClassRepository
    {
        Task<bool> CreateClassesAsync(CreateClassRequestDto request,
                List<StaffConfirmCourseResponse> confirmedUsers, string createdById);
        Task<bool> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request);
        Task<List<ScheduleItemResponseDto>> GetClassScheduleAsync(int classId);
        Task<List<ScheduleItemResponseDto>> GetUserScheduleAsync(string userId);
        Task<ClassDto?> GetClassDetailAsync(int classId);
        Task<List<ClassListDto>> GetClassesByCourseAsync(int courseId);
        Task<bool> CreateClassSwapRequestAsync(SwapClassRequest request);
        Task<bool> RespondToClassSwapAsync(RespondSwapRequest request, string responderId);
        Task<PagedResult<ClassDto>> GetClassesAsync(int page, int pageSize);
        Task<bool> RescheduleAsync(int scheduleId, RescheduleRequest request);
        Task<Schedule?> GetClassScheduleByIdAsync(int scheduleId);

        Task<List<ClassSwapDto>> GetSwapClassRequestAsync(string userId);
        Task<bool> UpdateScoresAsync(string mentorId, ScoreFinalRequest request);
        Task<bool> IsMentorOfClassAsync(int classId, string mentorId, CancellationToken ct = default);
        Task<bool> IsInClassAsync(int classId, string userId, CancellationToken ct = default);
        Task<List<Class>> GetClassesForMentorAsync(string userId, CancellationToken ct);
        Task<List<Class>> GetClassesForStaffAsync(string userId, CancellationToken ct);
        Task<List<MyClassDto>> GetClassesOfUserAsync(string userId, CancellationToken ct);
        Task<List<ApplicationUser>> GetStaffInClassAsync(int classId,CancellationToken ct );
    }
}
