namespace LMSProject.Models
{
    public class AssignmentStudent
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public required Assignment Assignment { get; set; }

        public required string StudentId { get; set; }

        public required ApplicationUser Student { get; set; }
    }

}
