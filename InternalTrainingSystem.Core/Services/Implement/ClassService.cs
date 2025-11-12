using Azure.Core;
using DocumentFormat.OpenXml.InkML;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<bool> CreateClassesAsync(CreateClassRequestDto request,
            List<StaffConfirmCourseResponse> confirmedUsers, string createdById)
        {
            return await _classRepo.CreateClassesAsync(request, confirmedUsers, createdById);
        }

        public async Task<bool> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request)
        {
            return await _classRepo.CreateWeeklySchedulesAsync(request);
        }

        public Task<ClassScheduleResponse> GetClassScheduleAsync(int classId)
        {
            return _classRepo.GetClassScheduleAsync(classId);
        }

        public async Task<List<ScheduleItemResponseDto>> GetUserScheduleAsync(string staffId)
        {
            return await _classRepo.GetUserScheduleAsync(staffId);
        }

        public async Task<List<ClassEmployeeAttendanceDto>> GetUserByClassAsync(int classId)
        {
            return await _classRepo.GetUserByClassAsync(classId);
        }

        public async Task<ClassDto?> GetClassDetailAsync(int classId)
        {
            return await _classRepo.GetClassDetailAsync(classId);
        }

        public async Task<List<ClassListDto>> GetClassesByCourseAsync(int courseId)
        {
            return await _classRepo.GetClassesByCourseAsync(courseId);
        }

        public Task<bool> CreateClassSwapRequestAsync(SwapClassRequest request)
        {
            return _classRepo.CreateClassSwapRequestAsync(request);
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(int page, int pageSize)
        {
            return await _classRepo.GetClassesAsync(page, pageSize);
        }

        public async Task<bool> RespondToClassSwapAsync(RespondSwapRequest request, string responderId)
        {
            return await _classRepo.RespondToClassSwapAsync(request, responderId);
        }

        public async Task<bool> RescheduleAsync(int scheduleId, RescheduleRequest request)
        {
            return await _classRepo.RescheduleAsync(scheduleId, request);
        }

        public async Task<Schedule?> GetClassScheduleByIdAsync(int scheduleId)
        {
            return await _classRepo.GetClassScheduleByIdAsync(scheduleId);
        }

        public async Task<List<ClassSwapDto>> GetSwapClassRequestAsync(string userId, int classSwapId)
        {
            return await _classRepo.GetSwapClassRequestAsync(userId, classSwapId);
        }
    }
}
