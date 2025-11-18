namespace BackEnd.Models.RequestModels
{
    public class RequestDeleteConstraintsModel
    {
        public bool CanDelete { get; set; }
        public string? Message { get; set; }
        public int EventsCount { get; set; }
        public int RequestNotesCount { get; set; }
    }
}


