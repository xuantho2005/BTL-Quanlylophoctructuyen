using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlineClassManagement.Controllers 
{
    /// <summary>
    /// Base Controller cho Admin - Yêu cầu phân quyền Admin
    /// </summary>
    [Authorize(Roles = "Admin")] 
    public class AdminBaseController : Controller
    {
        /// <summary>
        /// Thuộc tính giúp lấy nhanh ID của Admin đang đăng nhập
        /// </summary>
        protected int CurrentAdminId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int adminId))
                {
                    return adminId;
                }
                
                throw new InvalidOperationException("Không thể xác định ID Admin."); 
            }
        }
    }
}

