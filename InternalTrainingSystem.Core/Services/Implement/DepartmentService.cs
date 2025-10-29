using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalTrainingSystem.Core.Services.Implement
{
	public class DepartmentService : IDepartmentService
	{
		private readonly IDepartmentRepository _departmentRepo;
		public DepartmentService(IDepartmentRepository departmentRepo)
		{
			_departmentRepo = departmentRepo;
		}

		public async Task<int> CreateDepartmentAsync(CreateDepartmentDto input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input is null");
			}
			var department = new Models.Department
			{
				Name = input.Name,
				Description = input.Description
			};
			await _departmentRepo.AddDepartmentAsync(department);
			return department.Id;
		}

		public async Task<bool> DeleteDepartmentAsync(int departmentId)
		{
			var department = await _departmentRepo.GetDepartmentByIdAsync(departmentId);
			if (department == null)
			{
				throw new KeyNotFoundException("department not found");
			}
			await _departmentRepo.DeleteDepartmentAsync(departmentId);
			return true;
		}

		public async Task<PagedResult<DepartmenDetailsDto>> GetAllDepartmentsAsync(DepartmentInputDto input)
		{
			var result = await _departmentRepo.GetAllDepartmentsAsync(input.Page, input.PageSize);
			var departmentDtos = result.Items.Select(department => new DepartmenDetailsDto
			{
				Id = department.Id,
				Name = department.Name,
				Description = department.Description
			}).ToList();
			return new PagedResult<DepartmenDetailsDto>
			{
				Items = departmentDtos,
				TotalCount = result.TotalCount,
				Page = result.Page,
				PageSize = result.PageSize
			};
		}

		public async Task<DepartmenDetailsDto?> GetDepartmentByIdAsync(int departmentId)
		{
			var department = await _departmentRepo.GetDepartmentByIdAsync(departmentId);
			if (department == null)
			{
				throw new KeyNotFoundException("department not found");
			}
			var departmentDto = new DepartmenDetailsDto
			{
				Id = department.Id,
				Name =  department.Name,
				Description = department.Description
			};
			return departmentDto;
		}

		public async Task<List<DepartmentDto>> GetDepartments()
		{
			return await _departmentRepo.GetDepartments();
		}

		public async Task<bool> UpdateDepartmentAsync(UpdateDepartmentDto input)
		{
			var department = await _departmentRepo.GetDepartmentByIdAsync(input.Id);
			if (department == null)
			{
				throw new KeyNotFoundException("department not found");
			}
			department.Name = input.Name;
			department.Description = input.Description;
			await _departmentRepo.UpdateDepartmentAsync(department);
			return true;
		}
	}
}
