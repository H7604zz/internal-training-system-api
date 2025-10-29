using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;
        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetDepartments()
        {
            return await _context.Departments
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name,
                })
                .ToListAsync();
        }
    }
}
