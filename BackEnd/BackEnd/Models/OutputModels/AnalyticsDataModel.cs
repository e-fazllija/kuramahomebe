namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello per i dati Analytics Dashboard
    /// Contiene dati mensili per richieste, immobili, clienti e appuntamenti
    /// </summary>
    public class AnalyticsDataModel
    {
        public RequestsAnalyticsData Requests { get; set; } = new();
        public PropertiesAnalyticsData Properties { get; set; } = new();
        public CustomersAnalyticsData Customers { get; set; } = new();
        public AppointmentsAnalyticsData Appointments { get; set; } = new();
    }

    /// <summary>
    /// Dati analytics per le richieste
    /// </summary>
    public class RequestsAnalyticsData
    {
        public int Total { get; set; }
        public Dictionary<string, int> MonthlyData { get; set; } = new(); // Richieste inserite
        public Dictionary<string, int> ClosedData { get; set; } = new(); // Richieste evase
    }

    /// <summary>
    /// Dati analytics per gli immobili
    /// </summary>
    public class PropertiesAnalyticsData
    {
        public int Total { get; set; }
        public Dictionary<string, int> MonthlyData { get; set; } = new(); // Immobili inseriti
        public Dictionary<string, int> SoldData { get; set; } = new(); // Immobili venduti
    }

    /// <summary>
    /// Dati analytics per i clienti
    /// </summary>
    public class CustomersAnalyticsData
    {
        public int Total { get; set; }
        public Dictionary<string, int> MonthlyData { get; set; } = new(); // Clienti venditori
        public Dictionary<string, int> BuyersData { get; set; } = new(); // Clienti acquirenti
    }

    /// <summary>
    /// Dati analytics per gli appuntamenti
    /// </summary>
    public class AppointmentsAnalyticsData
    {
        public int Total { get; set; }
        public Dictionary<string, int> MonthlyData { get; set; } = new(); // Appuntamenti fissati
        public Dictionary<string, int> ConfirmedData { get; set; } = new(); // Appuntamenti confermati
    }
}

