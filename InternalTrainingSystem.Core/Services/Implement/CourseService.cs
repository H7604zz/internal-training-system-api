using ClosedXML.Excel;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;

        public CourseService(ICourseRepository courseRepo)
        {
            _courseRepo = courseRepo;
        }

        public async Task<Course> GetCourseByCourseCodeAsync(string courseCode)
        {
            return await _courseRepo.GetCourseByCourseCodeAsync(courseCode);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            return await _courseRepo.DeleteCourseAsync(id);
        }

        public async Task<Course> UpdateCourseAsync(int courseId, UpdateCourseMetadataDto meta, IList<IFormFile> lessonFiles, string updatedByUserId, CancellationToken ct = default)
        {
           return await _courseRepo.UpdateCourseAsync( courseId,  meta,  lessonFiles,  updatedByUserId,  ct = default);
        }

        public bool ToggleStatus(int id, string status)
        {
            return _courseRepo.ToggleStatus(id, status);
        }

        public async Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default)
        {
            return await _courseRepo.SearchAsync(req, ct);
        }

        public Course? GetCourseByCourseID(int? couseId)
        {
            return _courseRepo.GetCourseByCourseID(couseId);
        }

        public async Task<PagedResult<CourseListItemDto>> GetAllCoursesPagedAsync(GetAllCoursesRequest request)
        {
            return await _courseRepo.GetAllCoursesPagedAsync(request);
        }

        public async Task<CourseDetailDto?> GetCourseDetailAsync(int courseId)
        {
            return await _courseRepo.GetCourseDetailAsync(courseId);
        }

        // Duyệt khóa học - ban giám đốc
        public async Task<bool> UpdatePendingCourseStatusAsync(int courseId, string newStatus)
        {
            return await _courseRepo.UpdatePendingCourseStatusAsync(courseId, newStatus);
        }

        // Ban giám đốc xóa khóa học đã duyệt
        public async Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason)
        {
            return await _courseRepo.DeleteActiveCourseAsync(courseId, rejectReason);
        }

        public async Task<Course> CreateCourseAsync(CreateCourseMetadataDto meta,
                                IList<IFormFile> lessonFiles, string createdByUserId, CancellationToken ct = default)
        {
            return await _courseRepo.CreateCourseAsync(meta, lessonFiles, createdByUserId, ct);
        }
    }
}
