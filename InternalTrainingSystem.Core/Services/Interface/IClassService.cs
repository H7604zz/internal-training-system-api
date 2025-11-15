using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IClassService
    {
        // class service
        Task<bool> CreateClassesAsync(CreateClassRequestDto request,
             List<StaffConfirmCourseResponse> confirmedUsers, string createdById);
        Task<List<ClassEmployeeRecordDto>> GetUserInClassAsync(int classId);
        Task<ClassDto?> GetClassDetailAsync(int classId);
        Task<List<ClassListDto>> GetClassesByCourseAsync(int courseId);
        Task<PagedResult<ClassDto>> GetClassesAsync(int page, int pageSize);

        // schedule service
        Task<bool> RescheduleAsync(int scheduleId, RescheduleRequest request);
        Task<Schedule?> GetClassScheduleByIdAsync(int scheduleId);
        Task<bool> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request);
        Task<ClassScheduleResponse> GetClassScheduleAsync(int classId);
        Task<List<ScheduleItemResponseDto>> GetUserScheduleAsync(string staffId);

        // Swap class
        Task<bool> CreateClassSwapRequestAsync(SwapClassRequest request);
        Task<bool> RespondToClassSwapAsync(RespondSwapRequest request, string responderId);
        Task<List<ClassSwapDto>> GetSwapClassRequestAsync(string userId, int classSwapId);

        // score-final
        Task<bool> UpdateScoresAsync(string mentorId, ScoreFinalRequest request);
    }
}
