using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineClassManagement.Models.Enums;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho bài nộp của học sinh
    /// </summary>
    [Table("Submissions")]
    public class Submission
    {
        /// <summary>
        /// Khóa chính của bài nộp
        /// </summary>
        [Key]
        public int SubmissionId { get; set; }

        /// <summary>
        /// ID của bài tập
        /// </summary>
        [Required]
        [ForeignKey(nameof(Assignment))]
        public int AssignmentId { get; set; }

        /// <summary>
        /// ID của học sinh nộp bài
        /// </summary>
        [Required]
        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        /// <summary>
        /// Nội dung bài nộp (text)
        /// </summary>
        [StringLength(5000, ErrorMessage = "Nội dung không được vượt quá 5000 ký tự")]
        public string? Content { get; set; }

        /// <summary>
        /// URL file đính kèm
        /// </summary>
        [StringLength(500, ErrorMessage = "URL file không được vượt quá 500 ký tự")]
        public string? FileUrl { get; set; }

        /// <summary>
        /// Tên file gốc
        /// </summary>
        [StringLength(255, ErrorMessage = "Tên file không được vượt quá 255 ký tự")]
        public string? OriginalFileName { get; set; }

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Thời gian nộp bài
        /// </summary>
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Điểm số (tùy chọn)
        /// </summary>
        [Range(0, 1000, ErrorMessage = "Điểm số phải từ 0 đến 1000")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        /// <summary>
        /// Nhận xét của giáo viên
        /// </summary>
        [StringLength(1000, ErrorMessage = "Nhận xét không được vượt quá 1000 ký tự")]
        public string? Feedback { get; set; }

        /// <summary>
        /// Trạng thái bài nộp
        /// </summary>
        [Required]
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

        /// <summary>
        /// Có phải nộp muộn không
        /// </summary>
        public bool LateSubmission { get; set; } = false;

        /// <summary>
        /// Thời gian chấm điểm
        /// </summary>
        public DateTime? GradedAt { get; set; }

        // Navigation Properties

        /// <summary>
        /// Bài tập được nộp
        /// </summary>
        public virtual Assignment Assignment { get; set; } = null!;

        /// <summary>
        /// Học sinh nộp bài
        /// </summary>
        public virtual User Student { get; set; } = null!;
    }
}
