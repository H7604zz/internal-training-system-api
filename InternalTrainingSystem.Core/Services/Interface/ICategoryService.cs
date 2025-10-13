using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICategoryService
    {
        List<CourseCategory> GetCategories();
     }
}
