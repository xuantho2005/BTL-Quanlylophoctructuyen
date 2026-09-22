namespace OnlineClassManagement.Models.Enums
{
    /// <summary>
    /// Enum định nghĩa trạng thái đăng ký lớp học
    /// </summary>
    public enum EnrollmentStatus
    {
        /// <summary>
        /// Chờ phê duyệt
        /// </summary>
        Pending = 1,
        
        /// <summary>
        /// Đã được phê duyệt
        /// </summary>
        Approved = 2,
        
        /// <summary>
        /// Bị từ chối
        /// </summary>
        Rejected = 3
    }
}
