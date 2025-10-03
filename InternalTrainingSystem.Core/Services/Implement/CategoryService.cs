using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<CourseCategory> GetCategories()
        {
            return _context.CourseCategories.ToList();
        }
    }
}
