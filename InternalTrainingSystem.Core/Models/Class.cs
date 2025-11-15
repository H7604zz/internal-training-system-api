using InternalTrainingSystem.Core.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(200)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        public string? MentorId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; } = 30; //số lượng tối đa một lớp có thể chứa

        [StringLength(20)]
        public string Status { get; set; } = ClassConstants.Status.Created; // Created, Completed, Cancelled, Scheduled

        public bool IsActive { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public string? CreatedById { get; set; }

        // Navigation Properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("MentorId")]
        public virtual ApplicationUser Mentor { get; set; } = null!;

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        // Many-to-Many relationship with ApplicationUser (Employees)
        public virtual ICollection<ApplicationUser> Employees { get; set; } = new List<ApplicationUser>();
        
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
