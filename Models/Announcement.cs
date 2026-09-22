using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho thông báo trong lớp học
    /// </summary>
    [Table("Announcements")]
    public class Announcement
    {
        /// <summary>
        /// Khóa chính của thông báo
        /// </summary>
        [Key]
        public int AnnouncementId { get; set; }

        /// <summary>
        /// ID của lớp học
        /// </summary>
        [Required]
        [ForeignKey(nameof(Class))]
        public int ClassId { get; set; }

        /// <summary>
        /// Tiêu đề thông báo
        /// </summary>
        [Required(ErrorMessage = "Tiêu đề thông báo là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung thông báo
        /// </summary>
        [Required(ErrorMessage = "Nội dung thông báo là bắt buộc")]
        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// ID người tạo thông báo
        /// </summary>
        [Required]
        [ForeignKey(nameof(CreatedBy))]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Thời gian tạo thông báo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Có phải thông báo quan trọng không
        /// </summary>
        public bool IsImportant { get; set; } = false;

        /// <summary>
        /// Ngày hết hạn thông báo (tùy chọn)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Số lần xem thông báo
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Trạng thái hiển thị thông báo
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties

        /// <summary>
        /// Lớp học chứa thông báo
        /// </summary>
        public virtual Class Class { get; set; } = null!;

        /// <summary>
        /// Người tạo thông báo
        /// </summary>
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
