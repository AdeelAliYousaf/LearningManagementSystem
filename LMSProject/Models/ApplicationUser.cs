using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using LMSProject.Models;
public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }
    public required string Role { get; set; } // "Student", "Teacher", "Admin"

    public string? Specialization { get; set; }


    public Student? Student { get; set; }


    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
