using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface IDepartmentRepository
	{
		Task<List<DepartmentDto>> GetDepartmentsAsync();
		Task<Models.Department> AddDepartmentAsync(Models.Department department);
		Task UpdateDepartmentAsync(Models.Department department);
		Task DeleteDepartmentAsync(int departmentId);
		Task<Models.Department> GetDepartmentByIdAsync(int id);
		Task<Department> GetDepartmentCourseAndEmployeeAsync(int departmentId);
	}
}
