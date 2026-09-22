using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho tài liệu khóa học
    /// </summary>
    [Table("CourseMaterials")]
    public class CourseMaterial
    {
        /// <summary>
        /// Khóa chính của tài liệu
        /// </summary>
        [Key]
        public int MaterialId { get; set; }

        /// <summary>
        /// ID của lớp học
        /// </summary>
        [Required]
        [ForeignKey(nameof(Class))]
        public int ClassId { get; set; }

        /// <summary>
        /// Tiêu đề tài liệu
        /// </summary>
        [Required(ErrorMessage = "Tiêu đề tài liệu là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả tài liệu
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// URL file tài liệu
        /// </summary>
        [Required(ErrorMessage = "File tài liệu là bắt buộc")]
        [StringLength(500, ErrorMessage = "URL file không được vượt quá 500 ký tự")]
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Tên file gốc
        /// </summary>
        [Required(ErrorMessage = "Tên file là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên file không được vượt quá 255 ký tự")]
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// Loại file (PDF, DOC, PPT, etc.)
        /// </summary>
        [Required(ErrorMessage = "Loại file là bắt buộc")]
        [StringLength(20, ErrorMessage = "Loại file không được vượt quá 20 ký tự")]
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Kích thước file phải lớn hơn 0")]
        public long FileSize { get; set; }

        /// <summary>
        /// ID người upload
        /// </summary>
        [Required]
        [ForeignKey(nameof(UploadedBy))]
        public int UploadedBy { get; set; }

        /// <summary>
        /// Thời gian upload
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Có phải tài liệu công khai không
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Số lần download
        /// </summary>
        public int DownloadCount { get; set; } = 0;

        // Navigation Properties

        /// <summary>
        /// Lớp học chứa tài liệu
        /// </summary>
        public virtual Class Class { get; set; } = null!;

        /// <summary>
        /// Người upload tài liệu
        /// </summary>
        public virtual User UploadedByUser { get; set; } = null!;
    }
}
