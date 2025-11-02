using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IClassService
    {
        Task<bool> CreateClassesAsync(CreateClassRequestDto request,
             List<StaffConfirmCourseResponse> confirmedUsers);
        Task<(bool Success, string Message, int Count)> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request);

        Task<ClassScheduleResponse> GetClassScheduleAsync(int classId);
        Task<UserScheduleResponse> GetUserScheduleAsync(string staffId);
        Task<List<ClassEmployeeAttendanceDto>> GetUserByClassAsync(int classId);
        Task<ClassDto?> GetClassDetailAsync(int classId);
        Task<List<ClassDto>> GetClassesByCourseAsync(int courseId);
        Task<(bool Success, string Message)> SwapClassesAsync(SwapClassRequest request);
        Task<PagedResult<ClassDto>> GetClassesAsync(int page, int pageSize);
    }
}
