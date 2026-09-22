using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineClassManagement.Models.ViewModels
{
    public class CreateClassWithSchedulesViewModel
    {
        // Class properties
        [Required(ErrorMessage = "Tên lớp học là bắt buộc")]
        [Display(Name = "Tên lớp học")]
        public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã lớp là bắt buộc")]
        [Display(Name = "Mã lớp")]
        public string ClassCode { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        [Display(Name = "Năm học")]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "Số sinh viên tối đa")]
        public int? MaxStudents { get; set; }

        // Schedules
        public List<ScheduleInputModel> Schedules { get; set; } = new List<ScheduleInputModel>();
    }

    public class ScheduleInputModel
    {
        [Required(ErrorMessage = "Thứ trong tuần là bắt buộc")]
        [Display(Name = "Thứ trong tuần")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(4);

        [Display(Name = "Địa điểm")]
        [StringLength(255)]
        public string? Location { get; set; }
    }
}

