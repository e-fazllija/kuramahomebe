namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello per i dati delle Top Tipologie
    /// </summary>
    public class TopTypologiesDataModel
    {
        public List<TopTypologyItemModel> CategoriesPortfolio { get; set; } = new List<TopTypologyItemModel>();
        public List<TopTypologyItemModel> CategoriesRequests { get; set; } = new List<TopTypologyItemModel>();
    }

    /// <summary>
    /// Modello per un singolo elemento della top tipologie
    /// </summary>
    public class TopTypologyItemModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Percentage { get; set; }
    }
}

