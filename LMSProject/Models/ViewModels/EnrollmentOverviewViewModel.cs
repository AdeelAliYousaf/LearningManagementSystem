public class EnrollmentOverviewViewModel
{
    public required string ClassName { get; set; }
    public required List<StudentEnrollmentInfo> Students { get; set; }
}

public class StudentEnrollmentInfo
{
    public required string FullName { get; set; }
    public required string RollNumber { get; set; }
    public required List<string> Courses { get; set; }
}
