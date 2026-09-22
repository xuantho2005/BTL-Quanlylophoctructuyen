using Microsoft.AspNetCore.Mvc;
using OnlineClassManagement.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Globalization;
using OnlineClassManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace OnlineClassManagement.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

                        public async Task<IActionResult> Index(DateTime? startDate)

                        {

                            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))

                            {

                                return Unauthorized();

                            }

                

                            var startOfWeek = startDate ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

                

                            var schedules = await _context.Schedules

                                .Include(s => s.Class)

                                .Where(s => s.Class.Enrollments.Any(e => e.StudentId == userId && e.Status == OnlineClassManagement.Models.Enums.EnrollmentStatus.Approved) 
                                    && s.StartDate <= startOfWeek.AddDays(6) 
                                    && s.EndDate >= startOfWeek)

                                .ToListAsync();

                

                            var viewModel = new OnlineClassManagement.Models.ViewModels.WeeklyScheduleViewModel

                            {

                                StartOfWeek = startOfWeek

                            };

                

                            for (int i = 0; i < 7; i++)

                            {

                                var date = startOfWeek.AddDays(i);

                                var dayOfWeek = date.DayOfWeek;

                

                                viewModel.MorningSchedules[dayOfWeek] = schedules

                                    .Where(s => s.DayOfWeek == dayOfWeek && s.StartTime < TimeSpan.FromHours(12))

                                    .ToList();

                                viewModel.AfternoonSchedules[dayOfWeek] = schedules

                                    .Where(s => s.DayOfWeek == dayOfWeek && s.StartTime >= TimeSpan.FromHours(12))

                                    .ToList();

                            }

                

                            var weeks = new List<SelectListItem>();

                            var year = DateTime.Today.Year;

                            var firstDayOfYear = new DateTime(year, 1, 1);

                            var firstMonday = firstDayOfYear.AddDays((int)DayOfWeek.Monday - (int)firstDayOfYear.DayOfWeek);

                            if (firstMonday > firstDayOfYear) { firstMonday = firstMonday.AddDays(-7); }

                

                            for (int i = 0; i < 53; i++)

                            {

                                var weekStart = firstMonday.AddDays(i * 7);

                                if (weekStart.Year > year) break;

                                var weekEnd = weekStart.AddDays(6);

                                weeks.Add(new SelectListItem

                                {

                                    Text = $"Tuần {i + 1} ({weekStart:dd/MM} - {weekEnd:dd/MM})",

                                    Value = weekStart.ToString("yyyy-MM-dd")

                                });

                            }

                                        ViewBag.WeeksList = weeks;

                                        ViewBag.SelectedDate = startOfWeek.ToString("yyyy-MM-dd");

                

                            return View(viewModel);

                        }    }
}