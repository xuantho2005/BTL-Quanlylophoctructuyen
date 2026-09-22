
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineClassManagement.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [ForeignKey("SubmissionId")]
        public virtual Submission? Submission { get; set; }

        [Required]
        public int GraderId { get; set; }

        [ForeignKey("GraderId")]
        public virtual User? Grader { get; set; }

        public decimal Score { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime GradedAt { get; set; }
    }
}
