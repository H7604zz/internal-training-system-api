using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ICategoryRepository
    {
        List<CourseCategory> GetCategories();
    }
}
