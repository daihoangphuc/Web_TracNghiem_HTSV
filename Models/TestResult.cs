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

        
        [ForeignKey("Test")]
        public string TestId { get; set; } // ID of the user who took the test
        public Test? Test { get; set; }

        public string? QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; } // Is the selected answer correct?
        public DateTime StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; } // Submission time

        public int TotalScore { get; set; } // Total score achieved


    }
}
