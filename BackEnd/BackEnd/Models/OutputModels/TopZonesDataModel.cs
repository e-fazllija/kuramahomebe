namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello per i dati delle Top Zone
    /// Contiene solo i dati essenziali per Widget7 - Top Zone
    /// </summary>
    public class TopZonesDataModel
    {
        public List<TopZoneItemModel> PropertiesZones { get; set; } = new List<TopZoneItemModel>();
        public List<TopZoneItemModel> RequestsZones { get; set; } = new List<TopZoneItemModel>();
    }

    /// <summary>
    /// Modello per un singolo elemento della top zone
    /// </summary>
    public class TopZoneItemModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Percentage { get; set; }
    }
}



