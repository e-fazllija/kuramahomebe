namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello leggero per i dati della mappa
    /// Contiene solo i dati essenziali per Widget13
    /// </summary>
    public class MapDataModel
    {
        // Totali KPI (solo agenzie e agenti - sempre della cerchia completa, solo filtro anno)
        public int TotalAgencies { get; set; }
        public int TotalAgents { get; set; }

        // Dati per la mappa (modelli leggeri)
        public List<MapAgencyModel> Agencies { get; set; } = new List<MapAgencyModel>();
        public List<MapAgentModel> Agents { get; set; } = new List<MapAgentModel>();
    }
}

