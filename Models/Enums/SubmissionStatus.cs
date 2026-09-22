namespace OnlineClassManagement.Models.Enums
{
    /// <summary>
    /// Enum định nghĩa trạng thái nộp bài
    /// </summary>
    public enum SubmissionStatus
    {
        /// <summary>
        /// Đã nộp bài
        /// </summary>
        Submitted = 1,
        
        /// <summary>
        /// Đã chấm điểm
        /// </summary>
        Graded = 2,
        
        /// <summary>
        /// Đã trả bài
        /// </summary>
        Returned = 3
    }
}
