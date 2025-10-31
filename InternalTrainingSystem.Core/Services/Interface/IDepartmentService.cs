using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
	public interface IDepartmentService
	{
		Task<List<DepartmentDto>> GetDepartmentsAsync();
		Task<DepartmentDto?> GetDepartmentByIdAsync(int departmentId);
		Task<DepartmenCourseAndEmployeeDto?> GetDepartmentCourseAndEmployeeAsync(DepartmentCourseAndEmployeeInput input);
		Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto input);
		Task<int> CreateDepartmentAsync(CreateDepartmentDto input);
		Task<bool> DeleteDepartmentAsync(int departmentId);
	}
}
