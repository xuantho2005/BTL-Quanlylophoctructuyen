using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;

namespace OnlineClassManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            var totalUsers = await _context.Users.CountAsync();
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin");
            var totalTeachers = await _context.Users.CountAsync(u => u.Role == "Teacher");
            var totalStudents = await _context.Users.CountAsync(u => u.Role == "Student");
            
            var activeClasses = await _context.Classes.CountAsync(c => c.IsActive);
            var totalClasses = await _context.Classes.CountAsync();
            
            var totalAssignments = await _context.Assignments.CountAsync();
            var totalSubmissions = await _context.Submissions.CountAsync();
            
            // Thống kê đăng ký mới theo thời gian (30 ngày gần nhất)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var newUsersByDay = await _context.Users
                .Where(u => u.CreatedAt >= thirtyDaysAgo)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Students = g.Count(u => u.Role == "Student"),
                    Teachers = g.Count(u => u.Role == "Teacher"),
                    Admins = g.Count(u => u.Role == "Admin")
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalAdmins = totalAdmins;
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.ActiveClasses = activeClasses;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalAssignments = totalAssignments;
            ViewBag.TotalSubmissions = totalSubmissions;
            ViewBag.NewUsersByDay = newUsersByDay.Select(x => new 
            { 
                date = x.Date.ToString("yyyy-MM-dd"), 
                count = x.Count, 
                students = x.Students, 
                teachers = x.Teachers, 
                admins = x.Admins 
            }).ToList();

            return View();
        }
    }
}
