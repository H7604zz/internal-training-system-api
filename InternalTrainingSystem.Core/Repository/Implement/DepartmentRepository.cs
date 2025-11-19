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

		public async Task<List<DepartmentListDto>> GetDepartmentsAsync()
		{
			return await _context.Departments
							.Select(d => new DepartmentListDto
							{
								DepartmentId = d.Id,
								DepartmentName = d.Name,
								Description = d.Description,
							})
							.ToListAsync();
		}

		public async Task<bool> CreateDepartmentAsync(DepartmentRequestDto request)
		{
			var name = request.Name?.Trim();
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Tên phòng ban không được để trống.");

			bool exists = await _context.Departments
					.AnyAsync(d => d.Name.ToLower() == name.ToLower());

			if (exists)
				throw new InvalidOperationException("Tên phòng ban đã tồn tại.");

			var department = new Models.Department
			{
				Name = name,
				Description = request.Description
			};

			await _context.Departments.AddAsync(department);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> DeleteDepartmentAsync(int departmentId)
		{
			var department = await _context.Departments.FindAsync(departmentId);
			if (department is null)
				return false;

			_context.Departments.Remove(department);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<DepartmentDetailDto> GetDepartmentDetailAsync(int departmentId)
		{
			var department = await _context.Departments
					.Include(d => d.Courses)
					.Include(d => d.Users)
					.FirstOrDefaultAsync(d => d.Id == departmentId);

			if (department == null)
				throw new KeyNotFoundException("Không tìm thấy phòng ban.");

			var dto = new DepartmentDetailDto
            {
				DepartmentId = department.Id,
				DepartmentName = department.Name,
				Description = department.Description,

				UserDetail = department.Users?.Select(u => new UserProfileDto
				{
					Id = u.Id,
					EmployeeId = u.EmployeeId,
					FullName = u.FullName,
					Email = u.Email!,
					Department = u.Department.Name,
					Position = u.Position,
					IsActive = u.IsActive,
				}).ToList()
			};

			return dto;
		}

		public async Task<bool> UpdateDepartmentAsync(int id, DepartmentRequestDto request)
		{
			var name = request.Name?.Trim();
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Tên phòng ban không được để trống.");

			var existingDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
			if (existingDepartment == null)
				throw new KeyNotFoundException("Không tìm thấy phòng ban.");

			bool nameExists = await _context.Departments
					.AnyAsync(d => d.Id != id && d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

			if (nameExists)
				throw new InvalidOperationException("Tên phòng ban đã tồn tại.");

			existingDepartment.Name = name;
			existingDepartment.Description = request.Description;

			return await _context.SaveChangesAsync() > 0;
		}
	}
}