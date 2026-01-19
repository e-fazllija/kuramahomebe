namespace BackEnd.Models.OutputModels
{
    public class Widget3DataModel
    {
        // Mesi dell'anno (formato: "gen 25", "feb 25", ecc.)
        public List<string> Months { get; set; } = new List<string>();

        // Dati immobili inseriti per mese e categoria
        public PropertiesDataModel PropertiesData { get; set; } = new PropertiesDataModel();

        // Dati immobili venduti per mese e categoria
        public PropertiesDataModel SoldPropertiesData { get; set; } = new PropertiesDataModel();

        // Provvigioni incassate per mese (chiave = mese, valore = importo)
        public Dictionary<string, decimal> CommissionsMonthlyData { get; set; } = new Dictionary<string, decimal>();

        // Totali provvigioni
        public decimal TotalCommissionsPortfolio { get; set; }
        public decimal TotalCommissionsEarned { get; set; }

        // Totali valori immobili
        public decimal TotalPortfolioValue { get; set; }
        public decimal TotalSoldValue { get; set; }
    }

    public class PropertiesDataModel
    {
        // Immobili in vendita per mese
        public Dictionary<string, int> Sale { get; set; } = new Dictionary<string, int>();

        // Immobili in affitto per mese
        public Dictionary<string, int> Rent { get; set; } = new Dictionary<string, int>();

        // Immobili in asta per mese
        public Dictionary<string, int> Auction { get; set; } = new Dictionary<string, int>();
    }
}

