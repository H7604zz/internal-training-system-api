using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DepartmentController : ControllerBase
	{
		private readonly IDepartmentService _departmentService;

		public DepartmentController(IDepartmentService departmentService)
		{
			_departmentService = departmentService;
		}


		[HttpGet]
		public async Task<IActionResult> GetAllDepartments()
		{
			var departments = await _departmentService.GetDepartments();

			return Ok(departments);
		}
		[HttpGet("details")]
		public async Task<IActionResult> GetDepartmentCourseAndEmployee([FromQuery] DepartmentCourseAndEmployeeInput input)
		{
			var department = await _departmentService.GetDepartmentCourseAndEmployeeAsync(input);
			return Ok(department);
		}
		[HttpGet("get-all")]
		public async Task<IActionResult> GetPaged([FromQuery] DepartmentInputDto input)
		{
			var result = await _departmentService.GetAllDepartmentsAsync(input);
			return Ok(result);
		}
		[HttpPost]
		public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto input)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var id = await _departmentService.CreateDepartmentAsync(input);
			return Ok(new { message = "Xác nhận tạo thành công!" });
		}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var success = await _departmentService.UpdateDepartmentAsync(id, dto);
			if (!success)
				return NotFound();
			return NoContent();
		}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var success = await _departmentService.DeleteDepartmentAsync(id);
			if (!success)
				return NotFound();
			return NoContent();
		}
	}
}