using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineClassManagement.Models.Enums;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho bài tập trong lớp học
    /// </summary>
    [Table("Assignments")]
    public class Assignment
    {
        /// <summary>
        /// Khóa chính của bài tập
        /// </summary>
        [Key]
        public int AssignmentId { get; set; }

        /// <summary>
        /// ID của lớp học
        /// </summary>
        [Required]
        [ForeignKey(nameof(Class))]
        public int ClassId { get; set; }

        /// <summary>
        /// Tiêu đề bài tập
        /// </summary>
        [Required(ErrorMessage = "Tiêu đề bài tập là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả bài tập
        /// </summary>
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// <summary>
        /// Hướng dẫn làm bài
        /// </summary>
        [StringLength(2000, ErrorMessage = "Hướng dẫn không được vượt quá 2000 ký tự")]
        public string? Instructions { get; set; }

        /// <summary>
        /// Ngày hết hạn nộp bài
        /// </summary>
        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        [Display(Name = "Hạn nộp")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Điểm tối đa
        /// </summary>
        [Range(0, 1000, ErrorMessage = "Điểm tối đa phải từ 0 đến 1000")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxScore { get; set; } = 100;

        /// <summary>
        /// Loại bài tập
        /// </summary>
        [Required]
        public AssignmentType AssignmentType { get; set; }

        /// <summary>
        /// Trạng thái xuất bản bài tập
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// Cho phép nộp bài muộn
        /// </summary>
        public bool AllowLateSubmission { get; set; } = false;

        /// <summary>
        /// Thời gian tạo bài tập
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian cập nhật cuối cùng
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        /// <summary>
        /// Lớp học chứa bài tập
        /// </summary>
        public virtual Class Class { get; set; } = null!;

        /// <summary>
        /// Danh sách bài nộp của học sinh
        /// </summary>
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
