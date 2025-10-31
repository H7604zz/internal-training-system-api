using DocumentFormat.OpenXml.InkML;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepo;
       
        public ClassService(IClassRepository classRepository)
        {
            _classRepo = classRepository;
        }

        public async Task<(bool Success, List<ClassDto>? Data)> CreateClassesAsync(CreateClassRequestDto request, List<StaffConfirmCourseResponse> confirmedUsers)
        {
            return await _classRepo.CreateClassesAsync(request, confirmedUsers);
        }

        public async Task<(bool Success, string Message, int Count)> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request)
        {
            return await _classRepo.CreateWeeklySchedulesAsync(request);
        }

        public Task<ClassScheduleResponse> GetClassScheduleAsync(int classId)
        {
            return _classRepo.GetClassScheduleAsync(classId);
        }

        public async Task<UserScheduleResponse> GetUserScheduleAsync(string staffId)
        {
            return await _classRepo.GetUserScheduleAsync(staffId);
        }

        public async Task<List<ClassEmployeeDto>> GetUserByClassAsync(int classId)
        {
            return await _classRepo.GetUserByClassAsync(classId);
        }

        public async Task<ClassDto?> GetClassDetailAsync(int classId)
        {
            return await _classRepo.GetClassDetailAsync(classId);
        }

        public async Task<List<ClassDto>> GetClassesByCourseAsync(int courseId)
        {
            return await _classRepo.GetClassesByCourseAsync(courseId);
        }
    }
}
