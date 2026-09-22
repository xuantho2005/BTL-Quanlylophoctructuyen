using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;
using OnlineClassManagement.Models.ViewModels;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnlineClassManagement.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class ClassesController : TeacherBaseController
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Classes
        public async Task<IActionResult> Index()
        {
            // Lấy TempData (nếu có) từ các action Create, Edit, Delete
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            int currentTeacherId = CurrentTeacherId; 
            var classes = await _context.Classes
                .Where(c => c.TeacherId == currentTeacherId)
                .ToListAsync();
            return View(classes);
        }

        // GET: /Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Nhận thông báo từ Action Edit
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (id == null) return NotFound();
            var @class = await _context.Classes
                .FirstOrDefaultAsync(m => m.ClassId == id);
            if (@class == null) return NotFound();
            if (@class.TeacherId != CurrentTeacherId) return Forbid();
            return View(@class);
        }

        // GET: /Classes/Create
        public IActionResult Create()
        {
            var viewModel = new CreateClassWithSchedulesViewModel
            {
                Schedules = new List<ScheduleInputModel>
                {
                    new ScheduleInputModel
                    {
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddMonths(4)
                    }
                }
            };
            return View(viewModel);
        }

        // POST: /Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClassWithSchedulesViewModel viewModel)
        {
            // Validate class code uniqueness
            if (await _context.Classes.AnyAsync(c => c.ClassCode == viewModel.ClassCode))
            {
                ModelState.AddModelError("ClassCode", "Mã lớp này đã tồn tại. Vui lòng chọn mã khác.");
            }

            // Parse schedules trực tiếp từ form để tránh binding issues
            var validSchedules = new List<ScheduleInputModel>();
            
            // Tìm tất cả các index của schedules trong form
            var scheduleIndices = new HashSet<int>();
            foreach (var key in Request.Form.Keys)
            {
                var match = System.Text.RegularExpressions.Regex.Match(key, @"Schedules\[(\d+)\]");
                if (match.Success)
                {
                    scheduleIndices.Add(int.Parse(match.Groups[1].Value));
                }
            }
            
            // Parse từng schedule từ form
            foreach (var index in scheduleIndices.OrderBy(i => i))
            {
                var dayOfWeekStr = Request.Form[$"Schedules[{index}].DayOfWeek"].ToString();
                var startTimeStr = Request.Form[$"Schedules[{index}].StartTime"].ToString();
                var endTimeStr = Request.Form[$"Schedules[{index}].EndTime"].ToString();
                var startDateStr = Request.Form[$"Schedules[{index}].StartDate"].ToString();
                var endDateStr = Request.Form[$"Schedules[{index}].EndDate"].ToString();
                var locationStr = Request.Form[$"Schedules[{index}].Location"].ToString();
                
                // Kiểm tra xem có ít nhất một trường được điền (không phải empty)
                bool hasAnyData = !string.IsNullOrWhiteSpace(dayOfWeekStr) || 
                                 !string.IsNullOrWhiteSpace(startTimeStr) || 
                                 !string.IsNullOrWhiteSpace(endTimeStr) ||
                                 !string.IsNullOrWhiteSpace(startDateStr);
                
                // Nếu có dữ liệu, kiểm tra đầy đủ
                if (hasAnyData)
                {
                    // Kiểm tra đầy đủ thông tin bắt buộc
                    if (!string.IsNullOrWhiteSpace(dayOfWeekStr) && 
                        !string.IsNullOrWhiteSpace(startTimeStr) && 
                        !string.IsNullOrWhiteSpace(endTimeStr) &&
                        !string.IsNullOrWhiteSpace(startDateStr))
                    {
                        var schedule = new ScheduleInputModel();
                        
                        // Parse DayOfWeek
                        if (int.TryParse(dayOfWeekStr, out int dayInt) && Enum.IsDefined(typeof(DayOfWeek), dayInt))
                        {
                            schedule.DayOfWeek = (DayOfWeek)dayInt;
                        }
                        else
                        {
                            continue; // Skip nếu không parse được DayOfWeek
                        }
                        
                        // Parse StartTime
                        if (TimeSpan.TryParse(startTimeStr, out TimeSpan st))
                        {
                            schedule.StartTime = st;
                        }
                        else
                        {
                            continue; // Skip nếu không parse được StartTime
                        }
                        
                        // Parse EndTime
                        if (TimeSpan.TryParse(endTimeStr, out TimeSpan et))
                        {
                            schedule.EndTime = et;
                        }
                        else
                        {
                            continue; // Skip nếu không parse được EndTime
                        }
                        
                        // Parse StartDate
                        if (DateTime.TryParse(startDateStr, out DateTime sd))
                        {
                            schedule.StartDate = sd;
                        }
                        else
                        {
                            continue; // Skip nếu không parse được StartDate
                        }
                        
                        // Parse EndDate
                        if (!string.IsNullOrWhiteSpace(endDateStr) && DateTime.TryParse(endDateStr, out DateTime ed))
                        {
                            schedule.EndDate = ed;
                        }
                        else
                        {
                            schedule.EndDate = schedule.StartDate.AddMonths(4); // Default 4 tháng
                        }
                        
                        schedule.Location = locationStr;
                        
                        validSchedules.Add(schedule);
                    }
                }
            }
            
            viewModel.Schedules = validSchedules;

            // Validate at least one schedule
            if (viewModel.Schedules == null || viewModel.Schedules.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một lịch học cho lớp học. Lịch học phải có đầy đủ: Thứ trong tuần, Giờ bắt đầu, Giờ kết thúc, Ngày bắt đầu và Ngày kết thúc.");
            }

            // Validate schedules
            if (viewModel.Schedules != null)
            {
                for (int i = 0; i < viewModel.Schedules.Count; i++)
                {
                    var schedule = viewModel.Schedules[i];
                    if (schedule.StartTime >= schedule.EndTime)
                    {
                        ModelState.AddModelError($"Schedules[{i}].EndTime", "Giờ kết thúc phải sau giờ bắt đầu.");
                    }
                    if (schedule.StartDate > schedule.EndDate)
                    {
                        ModelState.AddModelError($"Schedules[{i}].EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Create class
                    var @class = new Class
                    {
                        ClassName = viewModel.ClassName,
                        ClassCode = viewModel.ClassCode,
                        Description = viewModel.Description,
                        AcademicYear = viewModel.AcademicYear,
                        Semester = viewModel.Semester,
                        MaxStudents = viewModel.MaxStudents,
                        TeacherId = CurrentTeacherId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsActive = true
                    };

                    _context.Add(@class);
                    await _context.SaveChangesAsync();

                    // Create schedules
                    if (viewModel.Schedules != null && viewModel.Schedules.Count > 0)
                    {
                        foreach (var scheduleInput in viewModel.Schedules)
                        {
                            var schedule = new Schedule
                            {
                                ClassId = @class.ClassId,
                                DayOfWeek = scheduleInput.DayOfWeek,
                                StartTime = scheduleInput.StartTime,
                                EndTime = scheduleInput.EndTime,
                                StartDate = scheduleInput.StartDate,
                                EndDate = scheduleInput.EndDate,
                                Location = scheduleInput.Location
                            };
                            _context.Add(schedule);
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Tạo lớp học mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE KEY constraint"))
                    {
                        ModelState.AddModelError("ClassCode", "Mã lớp này đã tồn tại. Vui lòng chọn mã khác.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu vào CSDL. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            // Ensure at least one schedule exists for display
            if (viewModel.Schedules == null || viewModel.Schedules.Count == 0)
            {
                viewModel.Schedules = new List<ScheduleInputModel>
                {
                    new ScheduleInputModel
                    {
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddMonths(4)
                    }
                };
            }

            return View(viewModel);
        }

        // GET: /Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();
            if (@class.TeacherId != CurrentTeacherId) return Forbid();
            return View(@class);
        }

        // POST: /Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, 
            [Bind("ClassId,ClassName,ClassCode,Description,AcademicYear,Semester,MaxStudents,IsActive")] Class @classFromForm)
        {
            if (id != @classFromForm.ClassId) return NotFound();

            var @classInDb = await _context.Classes.FindAsync(id);
            if (@classInDb == null) return NotFound();
            if (@classInDb.TeacherId != CurrentTeacherId) return Forbid();

            ModelState.Remove(nameof(Class.TeacherId));
            ModelState.Remove(nameof(Class.Teacher));

            if (await _context.Classes.AnyAsync(c => c.ClassCode == @classFromForm.ClassCode && c.ClassId != id))
            {
                ModelState.AddModelError("ClassCode", "Mã lớp này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    @classInDb.ClassName = @classFromForm.ClassName;
                    @classInDb.ClassCode = @classFromForm.ClassCode;
                    @classInDb.Description = @classFromForm.Description;
                    @classInDb.AcademicYear = @classFromForm.AcademicYear;
                    @classInDb.Semester = @classFromForm.Semester;
                    @classInDb.MaxStudents = @classFromForm.MaxStudents;
                    @classInDb.IsActive = @classFromForm.IsActive;
                    @classInDb.UpdatedAt = DateTime.Now; 

                    _context.Update(@classInDb);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
                    return RedirectToAction(nameof(Details), new { id = @classInDb.ClassId });
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE KEY constraint"))
                    {
                        ModelState.AddModelError("ClassCode", "Mã lớp này đã tồn tại.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật CSDL.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }
            return View(@classFromForm);
        }

        // GET: /Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var @class = await _context.Classes
                .FirstOrDefaultAsync(m => m.ClassId == id);
            if (@class == null) return NotFound();
            if (@class.TeacherId != CurrentTeacherId) return Forbid();
            return View(@class);
        }

        // POST: /Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class == null) return NotFound();
            if (@class.TeacherId != CurrentTeacherId) return Forbid();

            try
            {
                _context.Classes.Remove(@class);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa lớp học thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa lớp này. Lớp học có thể đang chứa dữ liệu (bài tập, sinh viên...).";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Classes/StudentList/5
        public async Task<IActionResult> StudentList(int id)
        {
            int classId = id;
            
            // 1. Kiểm tra quyền sở hữu
            if (!await CheckClassOwnershipAsync(classId))
            {
                return Forbid(); // Lỗi 403
            }

            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null) return NotFound();

            // 2. Lấy danh sách học viên
            var students = await _context.ClassEnrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student) // Tải thông tin User (Student)
                .Select(e => e.Student)
                .Where(s => s.Role == Models.Enums.UserRole.Student.ToString()) // Đảm bảo là Student
                .ToListAsync();

            // 3. Gửi thông tin sang View
            ViewBag.ClassId = classId;
            ViewBag.ClassName = @class.ClassName;

            return View(students);
        }

        // === HÀM HELPER KIỂM TRA QUYỀN SỞ HỮU ===
        private async Task<bool> CheckClassOwnershipAsync(int classId)
        {
            var @class = await _context.Classes
                .AsNoTracking() // Không cần theo dõi entity
                .FirstOrDefaultAsync(c => c.ClassId == classId);
                
            if (@class == null || @class.TeacherId != CurrentTeacherId)
            {
                return false;
            }
            return true;
        }
    }
}