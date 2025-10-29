using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<CourseCategory> GetCategories()
        {
            return _context.CourseCategories.ToList();
        }
    }
}
