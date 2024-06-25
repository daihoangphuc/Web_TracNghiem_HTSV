using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_TracNghiem_HTSV.Models
{
    public class Answer
    {
        [Key]
        public  string AnswerId { get; set; }
        public  string AnswerDescription { get; set; }
        // Câu hỏi mà đáp án này thuộc về
        [ForeignKey("QuestionId")]
        public string QuestionId { get; set; }
        public Question? Question { get; set; }

    }
}
