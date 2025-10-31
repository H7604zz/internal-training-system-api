using Amazon.Runtime.Internal.Util;
using DocumentFormat.OpenXml.Office2010.Excel;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
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

		public async Task<bool> CreateDepartmentAsync(CreateDepartmentDto department)
		{
			if (department == null)
			{
				return false;
			}
			var departmentEntity = new Department
			{
				Name = department.Name,
				Description = department.Description
			};
			await _context.Departments.AddAsync(departmentEntity);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteDepartmentAsync(int departmentId)
		{
			var department = await _context.Departments.FindAsync(departmentId);
			if (department == null)
			{
				return false;
			}
			_context.Departments.Remove(department);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<PagedResult<DepartmenDetailsDto>> GetAllDepartmentsAsync(DepartmentInputDto input)
		{
			if (input.Page <= 0) input.Page = 1;
			if (input.PageSize <= 0) input.PageSize = 10;
			var query = _context.Departments.AsQueryable();
			var totalItems = await query.CountAsync();
			var departmentDto = await query
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.Select(x => new DepartmenDetailsDto
				{
					Id = x.Id,
					Name = x.Name,
					Description = x.Description
				})
				.ToListAsync();

			return new PagedResult<DepartmenDetailsDto>
			{
				Items = departmentDto,
				TotalCount = totalItems,
				Page = input.Page,
				PageSize = input.PageSize
			};
		}

		public async Task<DepartmenDetailsDto> GetDepartmentByIdAsync(int id)
		{
			var department = await _context.Departments.FindAsync(id);
			if (department == null)
			{
				throw new KeyNotFoundException("Department is null");
			}
			var departmentDto = new DepartmenDetailsDto
			{
				Id = department.Id,
				Name = department.Name,
				Description = department.Description
			};
			return departmentDto;
		}

		public async Task<DepartmenCourseAndEmployeeDto> GetDepartmentCourseAndEmployeeAsync(DepartmentCourseAndEmployeeInput input)
		{
			var department = await _context.Departments
			 .FirstOrDefaultAsync(d => d.Id == input.Id);

			if (department == null)
				throw new KeyNotFoundException("Department not found");

			var courses = await _context.Courses
					.Where(c => c.Departments.Any(d => d.Id == input.Id) &&
											(string.IsNullOrEmpty(input.Search) || c.CourseName.Contains(input.Search)))
					.OrderBy(c => c.CourseId)
					.Skip((input.Page - 1) * input.PageSize)
					.Take(input.PageSize)
					.Select(c => new CourseDetailDto
					{
						Code = c.Code,
						CourseName = c.CourseName
					})
					.ToListAsync();

			var users = await _context.Users
					.Where(u => u.DepartmentId == input.Id &&
											(string.IsNullOrEmpty(input.Search) || u.FullName.Contains(input.Search)))
					.OrderBy(u => u.Id)
					.Skip((input.Page - 1) * input.PageSize)
					.Take(input.PageSize)
					.Select(u => new UserProfileDto
					{
						EmployeeId = u.EmployeeId,
						FullName = u.FullName
					})
					.ToListAsync();
			return new DepartmenCourseAndEmployeeDto
			{
				Id = department.Id,
				Name = department.Name,
				CourseDetail = courses,
				userDetail = users,
				TotalCourses = await _context.Courses.CountAsync(c => c.Departments.Any(d => d.Id == input.Id)),
				TotalUsers = await _context.Users.CountAsync(u => u.DepartmentId == input.Id)
			};
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

		public async Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto department)
		{
			var existingDepartment = await _context.Departments.FindAsync(id);
			if (existingDepartment == null)
			{
				return false;
			}
			existingDepartment.Name = department.Name;
			existingDepartment.Description = department.Description;
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
