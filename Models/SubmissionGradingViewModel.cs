namespace OnlineClassManagement.Models
{
    // ViewModel này chứa MỌI THỨ cần thiết cho trang chấm điểm
    public class SubmissionGradingViewModel
    {
        public Assignment AssignmentDetails { get; set; }
        public List<StudentSubmissionInfo> StudentSubmissions { get; set; }

        public SubmissionGradingViewModel()
        {
            AssignmentDetails = new Assignment();
            StudentSubmissions = new List<StudentSubmissionInfo>();
        }
    }

    // ViewModel này đại diện cho 1 sinh viên và bài nộp của họ
    public class StudentSubmissionInfo
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;

        // Thông tin bài nộp (có thể là NULL nếu chưa nộp)
        public Submission? Submission { get; set; } 
    }
}