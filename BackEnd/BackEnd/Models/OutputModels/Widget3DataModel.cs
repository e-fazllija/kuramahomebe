namespace BackEnd.Models.OutputModels
{
    public class Widget3DataModel
    {
        // Mesi dell'anno (formato: "gen 25", "feb 25", ...)
        public List<string> Months { get; set; } = new List<string>();

        // Dati immobili inseriti per mese e categoria
        public PropertiesDataModel PropertiesData { get; set; } = new PropertiesDataModel();

        // Dati immobili venduti per mese e categoria
        public PropertiesDataModel SoldPropertiesData { get; set; } = new PropertiesDataModel();

        // Provvigioni incassate per mese (chiave: "gen 25", valore: importo)
        public Dictionary<string, decimal> CommissionsMonthlyData { get; set; } = new Dictionary<string, decimal>();

        // Totale provvigioni portafoglio (tutti gli immobili dell'anno)
        public decimal TotalCommissionsPortfolio { get; set; }

        // Totale provvigioni incassate (immobili venduti nell'anno)
        public decimal TotalCommissionsEarned { get; set; }
    }

    public class PropertiesDataModel
    {
        // Vendita per mese (chiave: "gen 25", valore: conteggio)
        public Dictionary<string, int> Sale { get; set; } = new Dictionary<string, int>();

        // Affitto per mese (chiave: "gen 25", valore: conteggio)
        public Dictionary<string, int> Rent { get; set; } = new Dictionary<string, int>();

        // Aste per mese (chiave: "gen 25", valore: conteggio)
        public Dictionary<string, int> Auction { get; set; } = new Dictionary<string, int>();
    }
}
