using LMSProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Enrollment
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course Course { get; set; } // removed `required`

    public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
}
