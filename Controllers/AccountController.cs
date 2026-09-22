using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OnlineClassManagement.Models;
using OnlineClassManagement.Models.ViewModels;
using OnlineClassManagement.Data;
using System.Linq;
using BCrypt.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;

namespace OnlineClassManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                bool passwordMatches = false;
                if (user != null)
                {
                    // Nếu mật khẩu trong DB đã là hash BCrypt ($2...), verify bình thường
                    if (!string.IsNullOrEmpty(user.Password) && user.Password.StartsWith("$2"))
                    {
                        try
                        {
                            passwordMatches = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                        }
                        catch
                        {
                            // Hash không hợp lệ (salt sai format). Fallback: nếu DB đang lưu sai định dạng nhưng là plaintext
                            if (user.Password == model.Password)
                            {
                                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                                _context.Users.Update(user);
                                await _context.SaveChangesAsync();
                                passwordMatches = true;
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp dữ liệu cũ lưu plaintext (như trong script .sql seed)
                        if (user.Password == model.Password)
                        {
                            // Nâng cấp: hash lại và lưu để lần sau dùng BCrypt chuẩn
                            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                            _context.Users.Update(user);
                            await _context.SaveChangesAsync();
                            passwordMatches = true;
                        }
                    }
                }
                if (user != null && passwordMatches)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim(ClaimTypes.GivenName, user.FullName),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Điều hướng theo vai trò sau khi đăng nhập
                    if (!string.IsNullOrEmpty(user.Role) && user.Role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else if (!string.IsNullOrEmpty(user.Role) && user.Role.Equals("Teacher", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Classes", new { area = "Teacher" });
                    }
                    else if (!string.IsNullOrEmpty(user.Role) && user.Role.Equals("Student", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("JoinedClasses", "Class", new { area = "Student" });
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Student" // Default role
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
