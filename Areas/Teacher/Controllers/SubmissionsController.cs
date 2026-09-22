using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;
// Thêm namespace cho Enums
using OnlineClassManagement.Models.Enums;

namespace OnlineClassManagement.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    // Kế thừa TeacherBaseController để bảo vệ
    public class SubmissionsController : TeacherBaseController
    {
        private readonly ApplicationDbContext _context;

        public SubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === Hàm helper để kiểm tra quyền sở hữu Assignment ===
        private async Task<(bool, Assignment?)> CheckAssignmentOwnershipAsync(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null || assignment.Class.TeacherId != CurrentTeacherId)
            {
                return (false, null);
            }
            return (true, assignment);
        }

        // GET: /Submissions?assignmentId=5
        public async Task<IActionResult> Index(int assignmentId)
        {
            var (isOwner, assignment) = await CheckAssignmentOwnershipAsync(assignmentId);
            if (!isOwner)
            {
                return Forbid(); // Lỗi 403
            }
            if (assignment == null)
            {
                return NotFound();
            }

            // 1. Lấy tất cả sinh viên đã đăng ký lớp học này
            var enrolledStudents = await _context.ClassEnrollments
                .Where(e => e.ClassId == assignment.ClassId)
                .Include(e => e.Student)
                .Select(e => e.Student)
                .ToListAsync();

            // 2. Lấy tất cả bài nộp cho bài tập này
            var submissions = await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

            // 3. Kết hợp 2 danh sách lại
            var viewModel = new SubmissionGradingViewModel
            {
                AssignmentDetails = assignment,
                StudentSubmissions = new List<StudentSubmissionInfo>()
            };

            foreach (var student in enrolledStudents)
            {
                viewModel.StudentSubmissions.Add(new StudentSubmissionInfo
                {
                    StudentId = student.UserId,
                    StudentName = student.FullName,
                    StudentEmail = student.Email,
                    // Tìm bài nộp tương ứng của sinh viên này
                    Submission = submissions.FirstOrDefault(s => s.StudentId == student.UserId)
                });
            }

            return View(viewModel);
        }

        // POST: /Submissions/Grade (ĐÃ NÂNG CẤP)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int submissionId, decimal score, string feedback)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment.Class)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null) return NotFound();
            if (submission.Assignment.Class.TeacherId != CurrentTeacherId) return Forbid();
            
            var maxScore = submission.Assignment.MaxScore;
            if (score < 0 || score > maxScore)
            {
                return Json(new { success = false, message = $"Điểm phải nằm trong khoảng từ 0 đến {maxScore}." });
            }

            try
            {
                // === CẬP NHẬT CÁC TRƯỜNG MỚI ===
                submission.Score = score;
                submission.Feedback = feedback;
                submission.Status = SubmissionStatus.Graded; // Chuyển trạng thái
                submission.GradedAt = DateTime.Now; // Ghi lại ngày chấm
                // === KẾT THÚC CẬP NHẬT ===

                _context.Update(submission);
                await _context.SaveChangesAsync();

                // Trả về JSON thành công (gửi kèm dữ liệu mới)
                return Json(new { 
                    success = true, 
                    message = "Đã lưu!",
                    gradedAt = submission.GradedAt?.ToString("dd/MM/yyyy HH:mm"),
                    status = submission.Status.ToString() // Gửi về "Graded"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi máy chủ: {ex.Message}" });
            }
        }
    }
}