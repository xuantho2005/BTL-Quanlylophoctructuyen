using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineClassManagement.Models
{
    /// <summary>
    /// Model đại diện cho lớp học
    /// </summary>
    [Table("Classes")]
    public class Class
    {
        /// <summary>
        /// Khóa chính của lớp học
        /// </summary>
        [Key]
        public int ClassId { get; set; }

        /// <summary>
        /// Tên lớp học
        /// </summary>
        [Required(ErrorMessage = "Tên lớp học là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lớp học không được vượt quá 100 ký tự")]
        [Display(Name = "Tên Lớp")]
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// Mã lớp học (duy nhất)
        /// </summary>
        [Required(ErrorMessage = "Mã lớp học là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã lớp học không được vượt quá 20 ký tự")]
        [Display(Name = "Mã Lớp")]
        public string ClassCode { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả lớp học
        /// </summary>
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô Tả")]
        public string? Description { get; set; }

        /// <summary>
        /// ID của giáo viên phụ trách lớp
        /// </summary>
        [Required]
        [ForeignKey(nameof(Teacher))]
        public int TeacherId { get; set; }

        /// <summary>
        /// Học kỳ
        /// </summary>
        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        [StringLength(20, ErrorMessage = "Học kỳ không được vượt quá 20 ký tự")]
        public string Semester { get; set; } = string.Empty;

        /// <summary>
        /// Năm học
        /// </summary>
        [Required(ErrorMessage = "Năm học là bắt buộc")]
        [StringLength(10, ErrorMessage = "Năm học không được vượt quá 10 ký tự")]
        public string AcademicYear { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng học sinh tối đa
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Số lượng học sinh tối đa phải từ 1 đến 1000")]
        public int? MaxStudents { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của lớp học
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thời gian tạo lớp học
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian cập nhật cuối cùng
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        /// <summary>
        /// Giáo viên phụ trách lớp học
        /// </summary>
        public virtual User Teacher { get; set; } = null!;

        /// <summary>
        /// Danh sách học sinh đăng ký lớp học
        /// </summary>
        public virtual ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();

        /// <summary>
        /// Danh sách bài tập của lớp học
        /// </summary>
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        /// <summary>
        /// Danh sách tài liệu khóa học
        /// </summary>
        public virtual ICollection<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();

        /// <summary>
        /// Danh sách thông báo của lớp học
        /// </summary>
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        /// <summary>
        /// Lịch học và lịch thi của lớp học
        /// </summary>
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
