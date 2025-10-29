using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
	public interface IDepartmentService
	{
		Task<List<DepartmentDto>> GetDepartments();
		Task<PagedResult<DepartmenDetailsDto>> GetAllDepartmentsAsync(DepartmentInputDto input);
		Task<DepartmenDetailsDto?> GetDepartmentByIdAsync(int departmentId);
		Task<bool> UpdateDepartmentAsync(UpdateDepartmentDto input);
		Task<int> CreateDepartmentAsync(CreateDepartmentDto input);
		Task<bool> DeleteDepartmentAsync(int departmentId);
	}
}
