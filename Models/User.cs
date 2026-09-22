using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho người dùng trong hệ thống
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Khóa chính của người dùng
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Email đăng nhập (duy nhất)
        /// </summary>
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đã được hash
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên đầy đủ
        /// </summary>
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò của người dùng trong hệ thống
        /// </summary>
        [Required]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại (tùy chọn)
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Ngày sinh
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Quê quán
        /// </summary>
        [StringLength(200, ErrorMessage = "Quê quán không được vượt quá 200 ký tự")]
        public string? Hometown { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của tài khoản
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thời gian tạo tài khoản
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian cập nhật cuối cùng
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        /// <summary>
        /// Danh sách các lớp học mà người dùng là giáo viên
        /// </summary>
        public virtual ICollection<Class> TaughtClasses { get; set; } = new List<Class>();

        /// <summary>
        /// Danh sách các lớp học mà học sinh đã đăng ký
        /// </summary>
        public virtual ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();

        /// <summary>
        /// Danh sách các bài tập mà học sinh đã nộp
        /// </summary>
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Danh sách các tài liệu mà người dùng đã upload
        /// </summary>
        public virtual ICollection<CourseMaterial> UploadedMaterials { get; set; } = new List<CourseMaterial>();

        /// <summary>
        /// Danh sách các thông báo mà người dùng đã tạo
        /// </summary>
        public virtual ICollection<Announcement> CreatedAnnouncements { get; set; } = new List<Announcement>();
    }
}
