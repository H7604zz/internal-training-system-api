using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseCategory>> GetAll()
        {
            var items = _service.GetCategories();
            return Ok(items);
        }
    }
}
