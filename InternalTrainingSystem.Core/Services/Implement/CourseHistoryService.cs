using InternalTrainingSystem.Core.Models;
﻿using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseHistoryService : ICourseHistoryService
    {
        private readonly ICourseHistoryRepository _courseHistoryRepository;

        public CourseHistoryService(ICourseHistoryRepository courseHistoryRepository)
        {
            _courseHistoryRepository = courseHistoryRepository;
        }
        public async Task<List<CourseHistoryDto>> GetCourseHistoriesByIdAsync(int Id)
        {
            return await _courseHistoryRepository.GetCourseHistoriesByIdAsync(Id);
        }
    }
}

