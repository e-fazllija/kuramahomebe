namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello per i dati dei Top Agenti
    /// Contiene solo i dati essenziali per Widget7 - Top Agenti
    /// </summary>
    public class TopAgentsDataModel
    {
        public List<TopAgentItemModel> Agents { get; set; } = new List<TopAgentItemModel>();
    }

    /// <summary>
    /// Modello per un singolo elemento della top agenti
    /// </summary>
    public class TopAgentItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int SoldProperties { get; set; }
        public int LoadedProperties { get; set; }
        public int Requests { get; set; }
        public int Appointments { get; set; }
        public decimal Commissions { get; set; }
    }
}



