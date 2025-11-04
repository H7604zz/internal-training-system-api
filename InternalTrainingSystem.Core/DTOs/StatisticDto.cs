namespace InternalTrainingSystem.Core.DTOs
{
	public class CoursePassRateDto
	{
		public int CourseId { get; set; }
		public string CourseName { get; set; }
		public double PassRate { get; set; } 
		public int TotalParticipants { get; set; }
		public int TotalPassed { get; set; }
	}
	public class StaffRejectStatisticDto
	{
		public string StaffId { get; set; }
		public string StaffName { get; set; }
		public int RejectCount { get; set; }
	}
}
