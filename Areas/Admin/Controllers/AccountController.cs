using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineClassManagement.Controllers;
using OnlineClassManagement.Data;
using OnlineClassManagement.Models;
using BCrypt.Net;

namespace OnlineClassManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Account/Profile
        public async Task<IActionResult> Profile()
        {
            var adminId = CurrentAdminId;
            var admin = await _context.Users.FindAsync(adminId);
            
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: /Admin/Account/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var adminId = CurrentAdminId;
            var admin = await _context.Users.FindAsync(adminId);
            
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: /Admin/Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([Bind("UserId,Email,FullName,PhoneNumber,DateOfBirth,Hometown")] User user)
        {
            var adminId = CurrentAdminId;
            
            if (adminId != user.UserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAdmin = await _context.Users.FindAsync(adminId);
                    if (existingAdmin == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra email trùng (trừ chính nó)
                    if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != adminId))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                        return View(user);
                    }

                    existingAdmin.Email = user.Email;
                    existingAdmin.FullName = user.FullName;
                    existingAdmin.PhoneNumber = user.PhoneNumber;
                    existingAdmin.DateOfBirth = user.DateOfBirth;
                    existingAdmin.Hometown = user.Hometown;
                    existingAdmin.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingAdmin);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return View(user);
        }

        // GET: /Admin/Account/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Admin/Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var adminId = CurrentAdminId;
            var admin = await _context.Users.FindAsync(adminId);
            
            if (admin == null)
            {
                return NotFound();
            }

            // Kiểm tra mật khẩu hiện tại
            bool passwordMatches = false;
            if (!string.IsNullOrEmpty(admin.Password) && admin.Password.StartsWith("$2"))
            {
                try
                {
                    passwordMatches = BCrypt.Net.BCrypt.Verify(currentPassword, admin.Password);
                }
                catch
                {
                    if (admin.Password == currentPassword)
                    {
                        passwordMatches = true;
                    }
                }
            }
            else
            {
                if (admin.Password == currentPassword)
                {
                    passwordMatches = true;
                }
            }

            if (!passwordMatches)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View();
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải có ít nhất 6 ký tự.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Xác nhận mật khẩu không khớp.");
                return View();
            }

            admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            admin.UpdatedAt = DateTime.UtcNow;
            _context.Update(admin);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Profile));
        }
    }
}

