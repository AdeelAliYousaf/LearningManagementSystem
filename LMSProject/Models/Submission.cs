namespace LMSProject.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }

        public required string StudentId { get; set; }
        public ApplicationUser? Student { get; set; }

        public required string FilePath { get; set; }

        public string? Grade { get; set; }

        public string? Feedback { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
