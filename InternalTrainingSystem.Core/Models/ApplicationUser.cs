using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? EmployeeId { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        public DateTime? HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        // Navigation properties
        public Department? Department { get; set; }
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public virtual ICollection<CourseHistory> CourseHistories { get; set; } = new List<CourseHistory>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual ICollection<Class> MentoredClasses { get; set; } = new List<Class>();
        public virtual ICollection<Class> CreatedClasses { get; set; } = new List<Class>();
        
        // Many-to-Many relationship with Class (as Employee)
        public virtual ICollection<Class> EnrolledClasses { get; set; } = new List<Class>();
        
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    }
}