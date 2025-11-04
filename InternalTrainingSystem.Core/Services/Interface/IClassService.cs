using InternalTrainingSystem.Core.Configuration;
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
        Task<List<ClassEmployeeAttendanceDto>> GetUserByClassAsync(int classId);
        Task<ClassDto?> GetClassDetailAsync(int classId);
        Task<List<ClassDto>> GetClassesByCourseAsync(int courseId);
        Task<(bool Success, string Message)> CreateClassSwapRequestAsync(SwapClassRequest request);
        Task<(bool Success, string Message)> RespondToClassSwapAsync(RespondSwapRequest request, string responderId);
        Task<PagedResult<ClassDto>> GetClassesAsync(int page, int pageSize);

        // schedule service
        Task<(bool Success, string Message)> RescheduleAsync(int scheduleId, RescheduleRequest request);
        Task<Schedule?> GetClassScheduleByIdAsync(int scheduleId);
        Task<(bool Success, string Message)> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request);
        Task<ClassScheduleResponse> GetClassScheduleAsync(int classId);
        Task<UserScheduleResponse> GetUserScheduleAsync(string staffId);
    }
}
