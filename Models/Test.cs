using System.ComponentModel.DataAnnotations.Schema;

namespace Web_TracNghiem_HTSV.Models
{
    public class Test
    {
        public string TestId { get; set; }
        public string TestName { get; set; }

        public bool IsLocked { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<TestResult> TestResults { get; set; }

    }
}
