using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Web_TracNghiem_HTSV.Models;

namespace Web_TracNghiem_HTSV.Models
{
    public class Question
    {
        [Key]
        public string? QuestionId { get; set; }
        public string? QuestionContent { get; set; }
        public string? CorrectAnswer { get; set; } // Đáp án đúng
        // Danh sách các đáp án cho câu hỏi
        public ICollection<Answer> Answers { get; set; }

    }
}
