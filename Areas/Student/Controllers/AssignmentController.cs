using Microsoft.AspNetCore.Mvc;
using OnlineClassManagement.Models;
using OnlineClassManagement.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using OnlineClassManagement.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace OnlineClassManagement.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);

            if (assignment == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == userId);

            ViewBag.Submission = submission;

            return View(assignment);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int assignmentId, IFormFile file)
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

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy bài tập." });
                }
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Vui lòng chọn file để nộp." });
                }
                return RedirectToAction("Details", new { id = assignmentId });
            }

            try
            {
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/submissions", uniqueFileName);
                
                // Đảm bảo thư mục tồn tại
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Kiểm tra xem đã nộp chưa, nếu có thì cập nhật
                var existingSubmission = await _context.Submissions
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

                if (existingSubmission != null)
                {
                    // Xóa file cũ nếu có
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + existingSubmission.FileUrl.Replace('/', '\\'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Cập nhật submission
                    existingSubmission.FileUrl = "/submissions/" + uniqueFileName;
                    existingSubmission.OriginalFileName = file.FileName;
                    existingSubmission.FileSize = file.Length;
                    existingSubmission.SubmittedAt = DateTime.Now;
                    existingSubmission.Status = SubmissionStatus.Submitted;
                    _context.Update(existingSubmission);
                }
                else
                {
                    var submission = new Submission
                    {
                        AssignmentId = assignmentId,
                        StudentId = userId,
                        FileUrl = "/submissions/" + uniqueFileName,
                        OriginalFileName = file.FileName,
                        FileSize = file.Length,
                        SubmittedAt = DateTime.Now,
                        Status = SubmissionStatus.Submitted
                    };
                    _context.Submissions.Add(submission);
                }

                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = "Nộp bài thành công!",
                        submission = new {
                            fileName = file.FileName,
                            fileSize = file.Length,
                            submittedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                        }
                    });
                }

                return RedirectToAction("Details", new { id = assignmentId });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Lỗi khi nộp bài: " + ex.Message });
                }
                return RedirectToAction("Details", new { id = assignmentId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewSubmissionFile(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);

            if (submission == null || string.IsNullOrEmpty(submission.FileUrl))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + submission.FileUrl.Replace('/', '\\'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), submission.OriginalFileName);
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},  
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
    }
}
