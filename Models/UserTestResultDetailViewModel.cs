namespace Web_TracNghiem_HTSV.Models
{
    public class UserTestResultDetailViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string TestId { get; set; }
        public int TotalScore { get; set; }
        public TimeSpan? TimeTaken { get; set; }
        public DateTime? LatestSubmittedAt { get; set; }
    }
}
