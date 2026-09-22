using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;

namespace OnlineClassManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassesManagementController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ClassesManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/ClassesManagement
        public async Task<IActionResult> Index(string? searchTerm, bool? isActiveFilter)
        {
            var query = _context.Classes
                .Include(c => c.Teacher)
                .AsQueryable();

            // Lọc theo trạng thái
            if (isActiveFilter.HasValue)
            {
                query = query.Where(c => c.IsActive == isActiveFilter.Value);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => 
                    c.ClassName.Contains(searchTerm) || 
                    c.ClassCode.Contains(searchTerm) ||
                    c.Teacher.FullName.Contains(searchTerm));
            }

            var classes = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.IsActiveFilter = isActiveFilter;

            // Nếu là AJAX request, trả về partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ClassesTable", classes);
            }

            return View(classes);
        }

        // GET: /Admin/ClassesManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Assignments)
                .Include(c => c.Materials)
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: /Admin/ClassesManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: /Admin/ClassesManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                // Không xóa thật, chỉ vô hiệu hóa để đảm bảo tính nhất quán dữ liệu
                @class.IsActive = false;
                @class.UpdatedAt = DateTime.UtcNow;
                _context.Update(@class);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã vô hiệu hóa lớp học thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/ClassesManagement/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                @class.IsActive = false;
                @class.UpdatedAt = DateTime.UtcNow;
                _context.Update(@class);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã vô hiệu hóa lớp học thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/ClassesManagement/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                @class.IsActive = true;
                @class.UpdatedAt = DateTime.UtcNow;
                _context.Update(@class);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã kích hoạt lớp học thành công!", isActive = true });
                }
                
                TempData["SuccessMessage"] = "Đã kích hoạt lớp học thành công!";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Không tìm thấy lớp học." });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

