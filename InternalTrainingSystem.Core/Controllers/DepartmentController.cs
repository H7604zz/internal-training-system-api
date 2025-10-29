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
			return Ok(new { Id = id});
		}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var success = await _departmentService.UpdateDepartmentAsync(dto);
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