using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;

namespace OnlineClassManagement.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    // Kế thừa TeacherBaseController để bảo vệ
    public class CourseMaterialsController : TeacherBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Tiêm IWebHostEnvironment để xử lý file upload
        public CourseMaterialsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // === Hàm helper để kiểm tra quyền sở hữu Class ===
        private async Task<bool> CheckClassOwnershipAsync(int classId)
        {
            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null || @class.TeacherId != CurrentTeacherId)
            {
                return false;
            }
            return true;
        }

        // GET: /CourseMaterials?classId=5
        // Hiển thị danh sách tài liệu cho 1 lớp
        public async Task<IActionResult> Index(int classId)
        {
            if (!await CheckClassOwnershipAsync(classId))
            {
                return Forbid(); 
            }

            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null) return NotFound();

            var materials = await _context.CourseMaterials
                .Where(m => m.ClassId == classId)
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

            return View(materials);
        }

        // GET: /CourseMaterials/Create?classId=5
        // Hiển thị form upload
        public async Task<IActionResult> Create(int classId)
        {
            if (!await CheckClassOwnershipAsync(classId))
            {
                return Forbid();
            }

            var @class = await _context.Classes.FindAsync(classId);
            if (@class == null) return NotFound();

            var material = new CourseMaterial
            {
                ClassId = classId
            };

            ViewBag.ClassName = @class.ClassName;
            return View(material);
        }

        // POST: /CourseMaterials/Create (ĐÃ SỬA LỖI LOGIC)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ClassId,Title,Description")] CourseMaterial material, IFormFile file)
        {
            // Kiểm tra quyền sở hữu trước
            if (!await CheckClassOwnershipAsync(material.ClassId))
            {
                return Forbid();
            }
            
            // === PHẦN SỬA LỖI QUAN TRỌNG ===
            
            // 1. Gán các giá trị do server quản lý
            material.UploadedBy = CurrentTeacherId;

            // 2. Xóa TẤT CẢ các trường do server gán khỏi ModelState
            //    Chúng không bao giờ được gửi từ form, nên phải xóa
            ModelState.Remove(nameof(material.Class));
            ModelState.Remove(nameof(material.UploadedByUser));
            ModelState.Remove(nameof(material.UploadedAt));
            ModelState.Remove(nameof(material.IsPublic));
            ModelState.Remove(nameof(material.DisplayOrder));
            ModelState.Remove(nameof(material.DownloadCount));
            ModelState.Remove(nameof(material.FileUrl));
            ModelState.Remove(nameof(material.OriginalFileName));
            ModelState.Remove(nameof(material.FileSize));
            ModelState.Remove(nameof(material.FileType));

            // 3. Kiểm tra file thủ công
            if (file == null || file.Length == 0)
            {
                // Thêm lỗi duy nhất này nếu không có file
                ModelState.AddModelError("FileUrl", "Vui lòng chọn một tệp để tải lên.");
            }
            // === KẾT THÚC PHẦN SỬA ===

            if (ModelState.IsValid) // Bây giờ chỉ kiểm tra Title, Description, và lỗi file thủ công
            {
                try
                {
                    // === Xử lý Upload File ===
                    var uploadFile = file!; // đảm bảo không null trong nhánh hợp lệ
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadFile.FileName;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "materials");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }
                    // === Kết thúc xử lý Upload File ===

                    // Cập nhật model với đường dẫn FileUrl CHÍNH XÁC
                    material.FileUrl = "/materials/" + uniqueFileName;
                    
                    // Gán các giá trị từ file
                    material.UploadedAt = DateTime.Now;
                    material.OriginalFileName = uploadFile.FileName;
                    material.FileSize = uploadFile.Length;
                    material.FileType = uploadFile.ContentType;
                    material.IsPublic = true;
                    material.DisplayOrder = 0;
                    material.DownloadCount = 0;
                    // UploadedBy đã gán ở trên

                    _context.Add(material);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Tải lên tài liệu thành công!";
                    return RedirectToAction(nameof(Index), new { classId = material.ClassId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tải tệp lên: {ex.Message}");
                }
            }

            // Nếu thất bại, quay lại form
            var @class = await _context.Classes.FindAsync(material.ClassId);
            if (@class != null) ViewBag.ClassName = @class.ClassName;
            return View(material);
        }

        // GET: /CourseMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.CourseMaterials
                .Include(m => m.Class) 
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            if (material.Class.TeacherId != CurrentTeacherId)
            {
                return Forbid();
            }

            return View(material);
        }

        // POST: /CourseMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null) return NotFound();

            if (!await CheckClassOwnershipAsync(material.ClassId))
            {
                return Forbid();
            }

            try
            {
                // Xóa File vật lý trên server
                if (!string.IsNullOrEmpty(material.FileUrl))
                {
                    string relativePath = material.FileUrl.TrimStart('/');
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.CourseMaterials.Remove(material);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa tài liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index), new { classId = material.ClassId });
        }
    }
}