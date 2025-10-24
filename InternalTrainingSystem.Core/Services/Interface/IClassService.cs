using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Configuration;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IClassService
    {
        Task<PagedResult<ClassDto>> GetClassesAsync(GetAllClassesRequest request);
        Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto, string? currentUserId);
    }
}
