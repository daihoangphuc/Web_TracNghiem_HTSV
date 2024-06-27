namespace Web_TracNghiem_HTSV.Models
{
    public class TestViewModel
    {
        public string TestId { get; set; }
        public string TestName { get; set; }
        public bool IsTestTaken { get; set; }
        public bool IsLocked { get; set; } = true;
    }
}
