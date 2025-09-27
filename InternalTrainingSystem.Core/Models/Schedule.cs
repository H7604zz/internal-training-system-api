using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models 
{
    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled

        public int MaxParticipants { get; set; } = 0; // 0 means unlimited

        public bool IsOnline { get; set; } = false;

        [StringLength(500)]
        public string? OnlineLink { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int CourseId { get; set; }

        [Required]
        public string InstructorId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("InstructorId")]
        public virtual ApplicationUser Instructor { get; set; } = null!;

        public virtual ICollection<ScheduleParticipant> ScheduleParticipants { get; set; } = new List<ScheduleParticipant>();
        public virtual ICollection<CourseHistory> CourseHistories { get; set; } = new List<CourseHistory>();
    }
}