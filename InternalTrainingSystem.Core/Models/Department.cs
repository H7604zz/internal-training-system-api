using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        // 1-1 với User (mỗi phòng ban gắn đúng 1 user; ví dụ trưởng phòng)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // n-n với Course
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
