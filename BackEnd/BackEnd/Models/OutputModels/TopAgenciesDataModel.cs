namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello per i dati delle Top Agenzie
    /// Contiene solo i dati essenziali per Widget7 - Top Agenzie
    /// </summary>
    public class TopAgenciesDataModel
    {
        public List<TopAgencyItemModel> Agencies { get; set; } = new List<TopAgencyItemModel>();
    }

    /// <summary>
    /// Modello per un singolo elemento della top agenzie
    /// </summary>
    public class TopAgencyItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Properties { get; set; }
        public int Customers { get; set; }
        public int Requests { get; set; }
        public int SoldProperties { get; set; }
        public int Appointments { get; set; }
        public decimal Commissions { get; set; }
    }
}

