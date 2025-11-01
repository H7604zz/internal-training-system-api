using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Configuration;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IClassService
    {
        Task<(bool Success, List<ClassDto>? Data)> CreateClassesAsync(CreateClassRequestDto request,
             List<StaffConfirmCourseResponse> confirmedUsers);
        Task<(bool Success, string Message, int Count)> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request);

        Task<ClassScheduleResponse> GetClassScheduleAsync(int classId);
        Task<UserScheduleResponse> GetUserScheduleAsync(string staffId);
        Task<List<ClassEmployeeDto>> GetUserByClassAsync(int classId);
        Task<ClassDto?> GetClassDetailAsync(int classId);
        Task<List<ClassDto>> GetClassesByCourseAsync(int courseId);
        Task<(bool Success, string Message)> SwapClassesAsync(SwapClassRequest request);
    }
}
