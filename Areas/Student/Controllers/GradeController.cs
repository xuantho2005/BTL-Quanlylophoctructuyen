
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
    public class GradeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> MyGrades()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var submissions = await _context.Submissions
                .Where(s => s.StudentId == userId && s.Score.HasValue)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Class)
                .ToListAsync();

            return View(submissions);
        }
    }
}
