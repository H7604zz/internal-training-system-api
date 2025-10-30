using DocumentFormat.OpenXml.Wordprocessing;
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

		public async Task<DepartmenCourseAndEmployeeDto?> GetDepartmentCourseAndEmployeeAsync(DepartmentCourseAndEmployeeInput input)
		{
			var department = await _departmentRepo.GetDepartmentCourseAndEmployeeAsync(input.Id);
			if (department == null)
				throw new KeyNotFoundException("Department not found");

			var pagedCourses = department.Courses?
					.Skip((input.Page - 1) * input.PageSize)
					.Take(input.PageSize)
					.Select(e => new CourseDetailDto
					{
						CourseId = e.CourseId,
						CourseName = e.CourseName
					})
					.ToList() ?? new List<CourseDetailDto>();

			var pagedUsers = department.Users?
					.Skip((input.Page - 1) * input.PageSize)
					.Take(input.PageSize)
					.Select(u => new UserProfileDto
					{
						Id = u.Id,
						FullName = u.FullName
					})
					.ToList() ?? new List<UserProfileDto>();

			var departmentCourseAndEmployeeDto = new DepartmenCourseAndEmployeeDto
			{
				Id = department.Id,
				Name = department.Name,
				CourseDetail = pagedCourses,
				userDetail = pagedUsers,
				TotalCourses = department.Courses?.Count ?? 0, 
				TotalUsers = department.Users?.Count ?? 0      
			};

			return departmentCourseAndEmployeeDto;
		}

		public async Task<List<DepartmentDto>> GetDepartments()
		{
			return await _departmentRepo.GetDepartments();
		}

		public async Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto input)
		{
			var department = await _departmentRepo.GetDepartmentByIdAsync(id);
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
