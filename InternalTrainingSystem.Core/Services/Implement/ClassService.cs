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
using static InternalTrainingSystem.Core.DTOs.CourseStatisticsDto;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepo;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrackProgressRepository _trackProgressRepository;

        public ClassService(IClassRepository classRepository, ICourseRepository courseRepository,ITrackProgressRepository trackProgressRepository)
        {
            _classRepo = classRepository;
            _courseRepository = courseRepository;
            _trackProgressRepository = trackProgressRepository;
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
        public async Task<TrainingRoleOverviewDto> GetTrainingRoleOverviewAsync(
    TrainingOverviewByMonthFilterDto filter,
    CancellationToken ct = default)
        {
            int year = filter.Year;
            int? month = filter.Month;

            // 1. Th?ng kê theo COURSE
            var courseStats = await _courseRepository
                .GetTrainingOverviewStatsByMonthOrYearAsync(filter, ct);

            // 2. Th?ng kê theo CLASS (l?y luôn danh sách classId + t?ng h?c viên)
            var classStats = await _classRepo
                .GetClassIdsByYearMonthAsync(filter, ct);

            var classIds = classStats.ClassIds;
            int totalClassesOpened = classStats.TotalClassOpened;
            int totalStudentsInClasses = classStats.TotalEmployeesTrained;

            int totalPassedStudents = 0;
            int totalClassesHaveStats = 0;
            double sumPassRate = 0;

            // 2.2. L?p t?ng class, dùng l?i hàm GetClassPassRateAsync
            foreach (var classId in classIds)
            {
                var classPass = await _trackProgressRepository
                    .GetClassPassRateAsync(classId);

                // N?u l?p không có bu?i / không có h?c viên thì b? qua
                if (classPass.TotalStudents == 0)
                    continue;

                totalClassesHaveStats++;
                totalPassedStudents += classPass.PassedStudents;
                sumPassRate += classPass.PassRate; // % pass c?a t?ng l?p
            }

            double avgClassPassRate = 0;
            if (totalClassesHaveStats > 0)
            {
                avgClassPassRate = Math.Round(sumPassRate / totalClassesHaveStats, 2);
            }

            // 3. Tr? v? DTO t?ng h?p
            return new TrainingRoleOverviewDto
            {
                Year = year,
                Month = month ?? 0,

                // theo COURSE
                TotalCoursesOpened = courseStats.TotalCoursesOpened,
                TotalEmployeesTrainedByCourse = courseStats.TotalEmployeesTrained,

                // theo CLASS
                TotalClassesOpened = totalClassesOpened,
                TotalStudentsInClasses = totalStudentsInClasses,

                // pass / l?p
                TotalClassesHaveStats = totalClassesHaveStats,
                TotalPassedStudents = totalPassedStudents,
                AverageClassPassRate = avgClassPassRate
            };
        }
    }
}
