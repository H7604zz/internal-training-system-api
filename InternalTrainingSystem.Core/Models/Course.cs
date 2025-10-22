using InternalTrainingSystem.Core.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string? Code { get; set; }    

        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required] 
        public int CourseCategoryId { get; set; }

        public int Duration { get; set; } // in hours

        [StringLength(20)]
        public string Level { get; set; } = CourseConstants.Levels.Beginner; // Beginner, Intermediate, Advanced

        public string Status { get; set; } = CourseConstants.Status.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsOnline { get; set; } = true;

        public bool IsMandatory { get; set; } = false;

        [StringLength(255)]
        public string? RejectionReason { get; set; }

        public string? ApproveById { get; set; }

        // Foreign Keys
        [Required]
        public string CreatedById { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        [ForeignKey("ApproveById")]
        public virtual ApplicationUser ApproveBy { get; set; } = null!;

        [ForeignKey("CourseCategoryId")]
        public virtual CourseCategory CourseCategory { get; set; } = null!;

        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<CourseHistory> CourseHistories { get; set; } = new List<CourseHistory>();
        public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public virtual ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    }
}