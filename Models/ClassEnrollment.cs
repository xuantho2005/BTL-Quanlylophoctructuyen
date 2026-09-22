using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineClassManagement.Models.Enums;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho việc đăng ký lớp học của học sinh
    /// </summary>
    [Table("ClassEnrollments")]
    public class ClassEnrollment
    {
        /// <summary>
        /// Khóa chính của đăng ký
        /// </summary>
        [Key]
        public int EnrollmentId { get; set; }

        /// <summary>
        /// ID của lớp học
        /// </summary>
        [Required]
        [ForeignKey(nameof(Class))]
        public int ClassId { get; set; }

        /// <summary>
        /// ID của học sinh
        /// </summary>
        [Required]
        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        /// <summary>
        /// Ngày đăng ký
        /// </summary>
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Trạng thái đăng ký
        /// </summary>
        [Required]
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        /// <summary>
        /// Điểm số của học sinh trong lớp (tùy chọn)
        /// </summary>
        [Range(0, 10, ErrorMessage = "Điểm số phải từ 0 đến 10")]
        [Column(TypeName = "decimal(3,2)")]
        public decimal? Grade { get; set; }

        /// <summary>
        /// Ghi chú về đăng ký
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        // Navigation Properties

        /// <summary>
        /// Lớp học được đăng ký
        /// </summary>
        public virtual Class Class { get; set; } = null!;

        /// <summary>
        /// Học sinh đăng ký
        /// </summary>
        public virtual User Student { get; set; } = null!;
    }
}
