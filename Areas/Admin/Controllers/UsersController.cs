using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;
using OnlineClassManagement.Models.Enums;
using BCrypt.Net;

namespace OnlineClassManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string? roleFilter, string? searchTerm)
        {
            var query = _context.Users.AsQueryable();

            // Lọc theo role
            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => 
                    u.FullName.Contains(searchTerm) || 
                    u.Email.Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            ViewBag.RoleFilter = roleFilter ?? "All";
            ViewBag.SearchTerm = searchTerm;

            // Nếu là AJAX request, trả về partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UsersTable", users);
            }

            return View(users);
        }

        // GET: /Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.TaughtClasses)
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Class)
                .Include(u => u.Submissions)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Password,FullName,Role,PhoneNumber,DateOfBirth,Hometown")] User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                    return View(user);
                }

                // Hash password
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsActive = true;

                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: /Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Không cho phép edit password ở đây, sẽ có action riêng
            return View(user);
        }

        // POST: /Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Email,FullName,PhoneNumber,DateOfBirth,Hometown,IsActive")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra email trùng (trừ chính nó)
                    if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                        return View(user);
                    }

                    // Cập nhật thông tin (không cập nhật password và role - role không thể thay đổi để đảm bảo tính nhất quán dữ liệu)
                    existingUser.Email = user.Email;
                    existingUser.FullName = user.FullName;
                    // existingUser.Role = user.Role; // Bỏ tính năng sửa role để đảm bảo tính nhất quán dữ liệu
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.DateOfBirth = user.DateOfBirth;
                    existingUser.Hometown = user.Hometown;
                    existingUser.IsActive = user.IsActive;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: /Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Không xóa thật, chỉ vô hiệu hóa
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Update(user);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã vô hiệu hóa tài khoản thành công!", isActive = false });
                }
                
                TempData["SuccessMessage"] = "Đã vô hiệu hóa tài khoản thành công!";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserId = id;
            ViewBag.UserName = user.FullName;
            return View();
        }

        // POST: /Admin/Users/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                var user = await _context.Users.FindAsync(id);
                ViewBag.UserId = id;
                ViewBag.UserName = user?.FullName;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Xác nhận mật khẩu không khớp.");
                var user = await _context.Users.FindAsync(id);
                ViewBag.UserId = id;
                ViewBag.UserName = user?.FullName;
                return View();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            existingUser.UpdatedAt = DateTime.UtcNow;
            _context.Update(existingUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Update(user);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã kích hoạt tài khoản thành công!", isActive = true });
                }
                
                TempData["SuccessMessage"] = "Đã kích hoạt tài khoản thành công!";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}

