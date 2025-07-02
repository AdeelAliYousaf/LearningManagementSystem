using LMSProject.Models;
using LMSProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; // ✅ Required for IWebHostEnvironment
using System.IO;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env; 

    public StudentController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IWebHostEnvironment env) 
    {
        _userManager = userManager;
        _context = context;
        _env = env; 
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public async Task<IActionResult> PersonalDetails()
    {
        var userId = _userManager.GetUserId(User);
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return NotFound("Student details not found.");

        return View(student);
    }

    public async Task<IActionResult> StudentCourses()
    {
        var userId = _userManager.GetUserId(User);
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        var courses = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
            .Select(e => e.Course)
            .ToListAsync();

        ViewBag.ClassName = student?.Class ?? "Your Class";
        return View(courses);
    }

    public async Task<IActionResult> StudentAssignments()
    {
        var userId = _userManager.GetUserId(User);

        var assignments = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .SelectMany(e => e.Course.Assignments)
            .Include(a => a.Course)
            .ToListAsync();

        var submissions = await _context.Submissions
            .Where(s => s.StudentId == userId)
            .ToListAsync();

        ViewBag.Submissions = submissions;

        return View(assignments);
    }


    public async Task<IActionResult> StudentAnnouncements()
    {
        var userId = _userManager.GetUserId(User);

        var announcements = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .SelectMany(e => _context.Announcements
                .Where(a => a.CourseId == e.CourseId &&
                            (a.StudentId == null || a.StudentId == userId))) 
            .ToListAsync();

        return View(announcements);
    }


    public async Task<IActionResult> StudentSubmissions()
    {
        var userId = _userManager.GetUserId(User);

        var submissions = await _context.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == userId)
            .Include(s => s.Assignment.Course)
            .ToListAsync();

        return View(submissions);
    }

    [HttpGet]
    public async Task<IActionResult> SubmitAssignment(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null || assignment.DueDate < DateTime.Now)
        {
            TempData["Error"] = "Assignment not found or due date has passed.";
            return RedirectToAction("StudentAssignments");
        }

        var model = new SubmitAssignmentViewModel { AssignmentId = id };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAssignment(SubmitAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please upload a valid file.";
            return RedirectToAction(nameof(StudentAssignments));
        }

        var userId = _userManager.GetUserId(User);
        var assignment = await _context.Assignments.FindAsync(model.AssignmentId);

        if (assignment == null || assignment.DueDate < DateTime.Now)
        {
            TempData["Error"] = "Invalid assignment or deadline has passed.";
            return RedirectToAction(nameof(StudentAssignments));
        }

        var uploads = Path.Combine(_env.WebRootPath, "submissions");
        Directory.CreateDirectory(uploads);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
        var filePath = Path.Combine(uploads, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        var existingSubmission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == model.AssignmentId && s.StudentId == userId);

        if (existingSubmission == null)
        {
            _context.Submissions.Add(new Submission
            {
                AssignmentId = model.AssignmentId,
                StudentId = userId,
                SubmittedAt = DateTime.Now,
                FilePath = "/submissions/" + fileName,
                Feedback = "Submitted successfully!"
            });
        }
        else
        {
            existingSubmission.FilePath = "/submissions/" + fileName;
            existingSubmission.SubmittedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Assignment submitted successfully!";
        return RedirectToAction(nameof(StudentAssignments));
    }
}
