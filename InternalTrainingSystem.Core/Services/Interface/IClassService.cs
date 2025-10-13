using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IClassService
    {
        Task<IEnumerable<ClassDto>> GetClassesAsync();
        Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto, string? currentUserId);
    }
}
