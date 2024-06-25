using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web_TracNghiem_HTSV.Models;

namespace Web_TracNghiem_HTSV.Models
{
    public class TestResult
    {
        [Key]
        public string TestResultId { get; set; } // Ensure this is of type Guid

        [ForeignKey("User")]
        public string UserId { get; set; } // ID of the user who took the test
        public User? User { get; set; }

        [ForeignKey("Question")]
        public string QuestionId { get; set; } // ID of the user who took the test
        public Question? Question { get; set; }

        public string SelectedAnswer { get; set; }
        public DateTime SubmittedAt { get; set; } // Submission time
        public int TotalScore { get; set; } // Total score achieved
    }
}
