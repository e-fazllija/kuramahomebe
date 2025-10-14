namespace BackEnd.Entities
{
    public class RequestNotes : EntityBase
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int? CalendarId { get; set; }
        public int RequestId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
