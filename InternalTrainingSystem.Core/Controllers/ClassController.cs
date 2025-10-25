using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Temporarily commented for testing
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassController> _logger;

        public ClassController(IClassService classService, ILogger<ClassController> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ClassDto>>> GetClasses([FromQuery] GetAllClassesRequest request)
        {
            try
            {
                var result = await _classService.GetClassesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving classes");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<ClassDto>>> CreateClasses(CreateClassesDto createClassesDto)
        {
            try
            {
                var createdClasses = await _classService.CreateClassesAsync(createClassesDto);
                return Ok(createdClasses);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error creating classes");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error creating classes");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating classes");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
