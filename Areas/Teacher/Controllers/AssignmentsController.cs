using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;
using OnlineClassManagement.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineClassManagement.Controllers;

namespace OnlineClassManagement.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class AssignmentsController : TeacherBaseController
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<bool> CheckClassOwnershipAsync(int classId)
        {
            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null || @class.TeacherId != CurrentTeacherId)
            {
                return false;
            }
            return true;
        }

        // GET: /Assignments?classId=5
        public async Task<IActionResult> Index(int classId)
        {
            if (!await CheckClassOwnershipAsync(classId)) return Forbid();
            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null) return NotFound();

            var assignments = await _context.Assignments
                .Where(a => a.ClassId == classId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.ClassId = classId;
            ViewBag.ClassName = @class.ClassName;
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(assignments);
        }
        
        // GET: /Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
                
            if (assignment == null) return NotFound();
            
            // Kiểm tra quyền sở hữu
            if (assignment.Class.TeacherId != CurrentTeacherId) return Forbid();

            // === LẤY THÊM THÔNG TIN TÓM TẮT ===
            // 1. Đếm tổng số sinh viên trong lớp
            int totalStudents = await _context.ClassEnrollments
                .CountAsync(e => e.ClassId == assignment.ClassId);

            // 2. Đếm số bài đã nộp cho bài tập này
            int submittedCount = await _context.Submissions
                .CountAsync(s => s.AssignmentId == assignment.AssignmentId);
            
            ViewBag.TotalStudents = totalStudents;
            ViewBag.SubmittedCount = submittedCount;
            // === KẾT THÚC LẤY THÊM THÔNG TIN ===

            return View(assignment);
        }

        private void PopulateViewBagData()
        {
            // Dùng cho dropdown chọn Loại bài tập
            ViewBag.AssignmentTypes = new SelectList(
                Enum.GetValues(typeof(AssignmentType)), 
                AssignmentType.Homework 
            );
        }

        // GET: /Assignments/Create?classId=5
        public async Task<IActionResult> Create(int classId)
        {
            if (!await CheckClassOwnershipAsync(classId)) return Forbid();
            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null) return NotFound();
            
            var assignment = new Assignment
            {
                ClassId = classId,
                DueDate = DateTime.Now.AddDays(7),
                MaxScore = 10,
                AllowLateSubmission = false,
                IsPublished = true,
                AssignmentType = AssignmentType.Homework 
            };

            ViewBag.ClassName = @class.ClassName;
            PopulateViewBagData(); 
            return View(assignment);
        }

        // POST: /Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ClassId,Title,Description,Instructions,DueDate,MaxScore,AssignmentType,AllowLateSubmission,IsPublished")] Assignment assignment)
        {
            if (!await CheckClassOwnershipAsync(assignment.ClassId)) return Forbid();

            ModelState.Remove(nameof(assignment.Class));

            if (ModelState.IsValid)
            {
                try
                {
                    assignment.CreatedAt = DateTime.Now;
                    assignment.UpdatedAt = DateTime.Now;
                    _context.Add(assignment);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Tạo bài tập mới thành công!";
                    return RedirectToAction(nameof(Index), new { classId = assignment.ClassId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu: {ex.Message}");
                }
            }
            
            var @class = await _context.Classes.FindAsync(assignment.ClassId);
            if (@class != null) ViewBag.ClassName = @class.ClassName;
            PopulateViewBagData(); 
            return View(assignment);
        }

        // GET: /Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null) return NotFound();
            if (assignment.Class.TeacherId != CurrentTeacherId) return Forbid();

            PopulateViewBagData(); 
            return View(assignment);
        }

        // POST: /Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, 
            [Bind("AssignmentId,ClassId,Title,Description,Instructions,DueDate,MaxScore,CreatedAt,AssignmentType,AllowLateSubmission,IsPublished")] Assignment assignmentFromForm)
        {
            if (id != assignmentFromForm.AssignmentId) return NotFound();
            if (!await CheckClassOwnershipAsync(assignmentFromForm.ClassId)) return Forbid();

            ModelState.Remove(nameof(assignmentFromForm.Class));

            if (ModelState.IsValid)
            {
                try
                {
                    var assignmentInDb = await _context.Assignments.FindAsync(id);
                    if (assignmentInDb == null) return NotFound();
                    
                    // Cập nhật các trường
                    assignmentInDb.Title = assignmentFromForm.Title;
                    assignmentInDb.Description = assignmentFromForm.Description;
                    assignmentInDb.Instructions = assignmentFromForm.Instructions;
                    assignmentInDb.DueDate = assignmentFromForm.DueDate;
                    assignmentInDb.MaxScore = assignmentFromForm.MaxScore;
                    assignmentInDb.AssignmentType = assignmentFromForm.AssignmentType;
                    assignmentInDb.AllowLateSubmission = assignmentFromForm.AllowLateSubmission;
                    assignmentInDb.IsPublished = assignmentFromForm.IsPublished;
                    assignmentInDb.UpdatedAt = DateTime.Now;

                    _context.Update(assignmentInDb);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
                    return RedirectToAction(nameof(Index), new { classId = assignmentFromForm.ClassId }); 
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");
                }
            }
            
            PopulateViewBagData(); 
            return View(assignmentFromForm);
        }

        // GET: /Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null) return NotFound();
            if (assignment.Class.TeacherId != CurrentTeacherId) return Forbid();
            return View(assignment);
        }

        // POST: /Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();
            if (!await CheckClassOwnershipAsync(assignment.ClassId)) return Forbid();

            try
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài tập thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa bài tập này. Đã có sinh viên nộp bài.";
            }
            catch (Exception ex)
            {
                 TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index), new { classId = assignment.ClassId });
        }
    }
}