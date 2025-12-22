namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Dati per le richieste matchate ordinate per percentuale match
    /// </summary>
    public class MatchedRequestsDataModel
    {
        public List<MatchedRequestItemModel> MatchedRequests { get; set; } = new();
        public int Total { get; set; }
    }

    public class MatchedRequestItemModel
    {
        public int RequestId { get; set; }
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public int MatchPercentage { get; set; }
    }
}
