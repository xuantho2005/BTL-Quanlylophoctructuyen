using Microsoft.AspNetCore.Mvc;
using OnlineClassManagement.Models;
using OnlineClassManagement.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace OnlineClassManagement.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            var classes = from c in _context.Classes
                         select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(s => s.ClassName.Contains(searchString) || s.ClassCode.Contains(searchString));
            }

            var result = await classes.ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, classes = result.Select(c => new { 
                    classId = c.ClassId, 
                    className = c.ClassName, 
                    classCode = c.ClassCode 
                }) });
            }

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Join(int classId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không có quyền truy cập." });
                }
                return Unauthorized();
            }

            var classToJoin = await _context.Classes.FindAsync(classId);

            if (classToJoin == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy lớp học." });
                }
                return NotFound();
            }

            // Kiểm tra đã tham gia chưa
            var existingEnrollment = await _context.ClassEnrollments
                .FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == userId);
            
            if (existingEnrollment != null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Bạn đã tham gia lớp học này rồi." });
                }
                return RedirectToAction("Index", "Home");
            }

            var enrollment = new ClassEnrollment
            {
                ClassId = classId,
                StudentId = userId,
                EnrollmentDate = System.DateTime.Now,
                Status = OnlineClassManagement.Models.Enums.EnrollmentStatus.Approved
            };

            _context.ClassEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Tham gia lớp học thành công!" });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> JoinedClasses()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var joinedClasses = await _context.ClassEnrollments
                .Where(ce => ce.StudentId == userId)
                .Include(ce => ce.Class)
                .Select(ce => ce.Class)
                .ToListAsync();

            return View(joinedClasses);
        }

        [HttpGet]
        public async Task<IActionResult> CourseMaterials(int classId)
        {
            var materials = await _context.CourseMaterials
                .Where(cm => cm.ClassId == classId)
                .ToListAsync();

            ViewData["ClassId"] = classId;

            return View(materials);
        }

        [HttpGet]
        public async Task<IActionResult> Assignments(int classId)
        {
            var assignments = await _context.Assignments
                .Where(a => a.ClassId == classId)
                .ToListAsync();

            ViewData["ClassId"] = classId;

            return View(assignments);
        }
    }
}
