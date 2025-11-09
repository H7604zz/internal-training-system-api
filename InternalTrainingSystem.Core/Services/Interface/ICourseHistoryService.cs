using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using System.Collections.Generic;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseHistoryService
    {
        Task<List<CourseHistoryDto>> GetCourseHistoriesByIdAsync(int Id);
    }
}

