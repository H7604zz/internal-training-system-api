using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IClassRepository
    {
        Task<PagedResult<ClassDto>> GetClassesAsync(GetAllClassesRequest request);
        Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto);
    }
}
