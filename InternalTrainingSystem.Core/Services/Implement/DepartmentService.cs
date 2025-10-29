using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepo;
        public DepartmentService(IDepartmentRepository departmentRepo) {
            _departmentRepo = departmentRepo;
        }

        public async Task<List<DepartmentDto>> GetDepartments()
        {
            return await _departmentRepo.GetDepartments();
        }
    }
}
