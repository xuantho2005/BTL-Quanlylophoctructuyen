using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlineClassManagement.Controllers 
{
    // === Yêu cầu phân quyền: Chỉ những ai có Role = "Teacher" mới được vào ===
    [Authorize(Roles = "Teacher")] 
    public class TeacherBaseController : Controller
    {
        // Thuộc tính (Property) này giúp lấy nhanh ID của giáo viên đang đăng nhập
        protected int CurrentTeacherId
        {
            get
            {
                // Lấy UserId (đã lưu dưới dạng NameIdentifier) từ Claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int teacherId))
                {
                    return teacherId;
                }
                
                throw new InvalidOperationException("Không thể xác định ID giáo viên."); 
            }
        }
    }
}