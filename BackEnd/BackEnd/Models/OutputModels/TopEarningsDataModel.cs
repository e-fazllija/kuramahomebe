namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Dati per Top Guadagni (portafoglio e vendite anno)
    /// </summary>
    public class TopEarningsDataModel
    {
        public List<TopEarningItemModel> Portfolio { get; set; } = new();
        public List<TopEarningItemModel> SalesYear { get; set; } = new();
        public decimal TotalPortfolioCommission { get; set; }
        public decimal TotalSalesYearCommission { get; set; }
    }

    public class TopEarningItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string UserFirstName { get; set; } = string.Empty;
        public double Price { get; set; }
        public double EffectiveCommission { get; set; }
    }
}

