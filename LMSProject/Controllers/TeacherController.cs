using LMSProject.Models;
using LMSProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public TeacherController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public async Task<IActionResult> MyCourses()
    {
        var userId = _userManager.GetUserId(User);
        var courses = await _context.Courses
            .Where(c => c.TeacherId == userId)
            .ToListAsync();
        return View(courses);
    }

    public async Task<IActionResult> MyAssignments()
    {
        var userId = _userManager.GetUserId(User);
        var assignments = await _context.Assignments
            .Include(a => a.Course)
            .Where(a => a.Course.TeacherId == userId)
            .ToListAsync();
        return View(assignments);
    }

    public async Task<IActionResult> MyAnnouncements()
    {
        var userId = _userManager.GetUserId(User);
        var announcements = await _context.Announcements
            .Include(a => a.Course)
            .Where(a => a.Course.TeacherId == userId)
            .ToListAsync();
        return View(announcements);
    }

    public async Task<IActionResult> Submissions()
    {
        var userId = _userManager.GetUserId(User);
        var submissions = await _context.Submissions
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Course)
            .Include(s => s.Student)
                .ThenInclude(appUser => appUser.Student) // ApplicationUser → Student
            .Where(s => s.Assignment.Course.TeacherId == userId)
            .ToListAsync();

        return View(submissions);
    }


    // GET: Upload Assignment Form
    [HttpGet]
    public async Task<IActionResult> UploadAssignment()
    {
        var teacherId = _userManager.GetUserId(User);

        // Fetch teacher's courses
        var courses = await _context.Courses
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync();

        // Fetch classes enrolled in the teacher's courses
        var classes = await _context.Students
            .Where(s => s.User.Enrollments.Any(e => courses.Select(c => c.Id).Contains(e.CourseId)))
            .Select(s => s.Class)
            .Distinct()
            .ToListAsync();

        ViewBag.Courses = courses;
        ViewBag.Classes = classes;

        return View();
    }

    // POST: Handle assignment upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAssignment(UploadAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid submission.";
            return RedirectToAction(nameof(UploadAssignment));
        }

        string filePath = string.Empty;

        // Handle file upload
        if (model.File != null && model.File.Length > 0)
        {
            string uploads = Path.Combine(_env.WebRootPath, "assignments");
            Directory.CreateDirectory(uploads);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
            filePath = Path.Combine("assignments", fileName);

            using (var stream = new FileStream(Path.Combine(_env.WebRootPath, filePath), FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }
        }

        // Create Assignment object
        var assignment = new Assignment
        {
            Title = model.Title,
            Description = model.Description,
            DueDate = model.DueDate,
            CourseId = model.CourseId,
            FilePath = "/" + filePath,
            Course = await _context.Courses.FindAsync(model.CourseId),
            Submissions = new List<Submission>()
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        // Assign assignment to students based on class selection
        var studentIds = await _context.Students
            .Where(s => model.SelectedClasses.Contains(s.Class))
            .Select(s => s.UserId)
            .ToListAsync();

        foreach (var studentId in studentIds)
        {
            _context.AssignmentStudents.Add(new AssignmentStudent
            {
                AssignmentId = assignment.Id,
                StudentId = studentId,
                Assignment = assignment,
                Student = await _userManager.FindByIdAsync(studentId)
            });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Assignment uploaded and assigned successfully!";
        return RedirectToAction("MyAssignments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GradeSubmission(int submissionId, string grade)
    {
        var userId = _userManager.GetUserId(User);

        var submission = await _context.Submissions
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Course)
            .Include(s => s.Student)
                .ThenInclude(u => u.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.Assignment.Course.TeacherId == userId);

        if (submission == null) return NotFound();

        submission.Grade = grade;

        // Create a personalized announcement
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == submission.StudentId);

        if (student != null)
        {
            _context.Announcements.Add(new Announcement
            {
                Title = $"Assignment Graded: {submission.Assignment.Title}",
                Message = $"Hi {student.User.FullName},\n\n" +
                          $"Your submission for **{submission.Assignment.Title}** in course **{submission.Assignment.Course.Title}** " +
                          $"has been graded. Your grade is: **{grade}**.\n\n" +
                          $"Keep up the good work!",
                CourseId = submission.Assignment.CourseId,
                CreatedAt = DateTime.Now,
                PostedById = userId // ✅ Make sure this column exists in Announcement
            });
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Grade submitted and announced.";
        return RedirectToAction("Submissions");
    }


}
