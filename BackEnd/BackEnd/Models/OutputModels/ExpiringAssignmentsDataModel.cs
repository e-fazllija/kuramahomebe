namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Dati per immobili con incarico in scadenza (meno di 30 giorni) e scaduti
    /// </summary>
    public class ExpiringAssignmentsDataModel
    {
        public List<ExpiringAssignmentItemModel> Properties { get; set; } = new();
        public int Total { get; set; }
        public List<ExpiringAssignmentItemModel> ExpiredProperties { get; set; } = new();
        public int TotalExpired { get; set; }
    }

    public class ExpiringAssignmentItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime AssignmentEnd { get; set; }
        public int DaysUntilExpiry { get; set; }
    }
}

