using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseService
    {
        public List<Course> GetCourses();
        public bool UpdateCourses(Course course);
        public bool DeleteCoursesByCourseId(int id);
        public Course? CreateCourses(Course course);
        public bool ToggleStatus(int id, bool isActive);
    }
}
