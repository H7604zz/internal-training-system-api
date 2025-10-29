using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface IDepartmentRepository
	{
		Task<List<DepartmentDto>> GetDepartments();
		Task<PagedResult<Models.Department>> GetAllDepartments(int page, int pageSize);
		Task<Models.Department> AddDepartmentAsync(Models.Department department);
		Task UpdateDepartmentAsync(Models.Department department);
		Task DeleteDepartmentAsync(int departmentId);
		Task<Models.Department> GetDepartmentByIdAsync(int id);
	}
}
