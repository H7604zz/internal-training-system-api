using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InternalTrainingSystem.Core.Repository.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepo;
       
        public ClassService(IClassRepository classRepository)
        {
            _classRepo = classRepository;
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(GetAllClassesRequest request)
        {
            return await _classRepo.GetClassesAsync(request);
        }

        public async Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto)
        {
            return await _classRepo.CreateClassesAsync(createClassesDto);
        }
    }
}
