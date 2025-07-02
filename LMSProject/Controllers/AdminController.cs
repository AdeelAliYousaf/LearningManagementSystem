using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProject.Models;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Dashboard() => View("Index");


    [HttpGet]
    public IActionResult EnrollUser() => View();


    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> Enroll(string Role, string FullName, string Email, string Password, string? RollNumber, string? RegistrationNumber, string? Class, string? Specialization)
    {
        Console.WriteLine("ENROLL METHOD CALLED");

        var user = new ApplicationUser
        {
            UserName = Email,
            Email = Email,
            FullName = FullName,
            Role = Role,
            EmailConfirmed = true,
            Specialization = Role == "Teacher" ? Specialization : null
        };

        var result = await _userManager.CreateAsync(user, Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Role);

            if (Role == "Student")
            {
                var student = new Student
                {
                    UserId = user.Id,
                    RollNumber = RollNumber,
                    RegistrationNumber = RegistrationNumber,
                    Class = Class
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"{Role} enrolled successfully!";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction("EnrollUser");
    }


    [HttpGet]
    public async Task<IActionResult> RegisterCourse()
    {
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
        ViewBag.Teachers = teachers;
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> RegisterCourse(Course model)
    {
        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                }
            }

            TempData["Error"] = "There was an issue creating the course. Please fill all required fields.";
            ViewBag.Teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            return View(model);
        }

        _context.Courses.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Course registered successfully!";
        return RedirectToAction("RegisterCourse");
    }

    [HttpGet]
    public async Task<IActionResult> EnrollClassToCourse()
    {
        var courses = await _context.Courses.ToListAsync();
        var classList = await _context.Students
            .Select(s => s.Class)
            .Distinct()
            .ToListAsync();

        ViewBag.Courses = courses;
        ViewBag.Classes = classList;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EnrollClassToCourse(string className, int courseId)
    {
        var students = await _context.Students
            .Where(s => s.Class == className)
            .ToListAsync();

        foreach (var student in students)
        {
            bool alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == student.UserId);

            if (!alreadyEnrolled)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    UserId = student.UserId,
                    CourseId = courseId,
                    EnrolledOn = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"All students in class '{className}' enrolled to course.";
        return RedirectToAction("EnrollClassToCourse");
    }


    [HttpGet]
    public async Task<IActionResult> ClassEnrollments()
    {
        var classGroups = await _context.Students
            .Include(s => s.User)
            .Include(s => s.User.Enrollments)
                .ThenInclude(e => e.Course)
            .ToListAsync();

        var grouped = classGroups
            .GroupBy(s => s.Class)
            .Select(g => new EnrollmentOverviewViewModel
            {
                ClassName = g.Key,
                Students = g.Select(s => new StudentEnrollmentInfo
                {
                    FullName = s.User.FullName,
                    RollNumber = s.RollNumber,
                    Courses = s.User.Enrollments.Select(e => e.Course.Title).ToList()
                }).ToList()
            }).ToList();

        return View(grouped);
    }


    [HttpGet]
    public async Task<IActionResult> TeacherCourseAssignments()
    {
        var teacherCourses = await _context.Courses
            .Include(c => c.Teacher)
            .ToListAsync();

        return View(teacherCourses);
    }

    [HttpGet]
    public async Task<IActionResult> UnrollClassFromCourse()
    {
        var classList = await _context.Students
            .Select(s => s.Class)
            .Distinct()
            .ToListAsync();

        var courses = await _context.Courses.ToListAsync();

        ViewBag.Classes = classList;
        ViewBag.Courses = courses;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UnrollClassFromCourse(string className, int courseId)
    {
        var enrollments = await _context.Enrollments
            .Where(e => e.CourseId == courseId && _context.Students
                .Where(s => s.Class == className)
                .Select(s => s.UserId)
                .Contains(e.UserId))
            .ToListAsync();

        _context.Enrollments.RemoveRange(enrollments);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Class '{className}' successfully unrolled from course.";
        return RedirectToAction("UnrollClassFromCourse");
    }


    [HttpGet]
    public async Task<IActionResult> UnassignTeacher()
    {
        var coursesWithTeachers = await _context.Courses
            .Include(c => c.Teacher)
            .Where(c => c.TeacherId != null)
            .ToListAsync();

        return View(coursesWithTeachers);
    }

    [HttpPost]
    public async Task<IActionResult> UnassignTeacher(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course != null)
        {
            course.TeacherId = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Teacher unassigned from course successfully.";
        }
        else
        {
            TempData["Error"] = "Course not found.";
        }

        return RedirectToAction("UnassignTeacher");
    }

    [HttpGet]
    public async Task<IActionResult> AssignTeacher()
    {
        var unassignedCourses = await _context.Courses
            .Where(c => c.TeacherId == null)
            .ToListAsync();

        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");

        ViewBag.Teachers = teachers;
        return View(unassignedCourses);
    }

    [HttpPost]
    public async Task<IActionResult> AssignTeacher(int courseId, string teacherId)
    {
        var course = await _context.Courses.FindAsync(courseId);

        if (course != null && !string.IsNullOrEmpty(teacherId))
        {
            course.TeacherId = teacherId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Teacher assigned successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to assign teacher.";
        }

        return RedirectToAction("AssignTeacher");
    }

    [HttpGet]
    public async Task<IActionResult> GetTeachersBySpecialization(string specialization)
    {
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
        var filtered = teachers
            .Where(t => t.Specialization != null && t.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase))
            .Select(t => new { t.Id, t.FullName, t.Email })
            .ToList();

        return Json(filtered);
    }


}

