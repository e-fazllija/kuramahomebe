namespace BackEnd.Models
{
    /// <summary>
    /// Modello di risposta per entità con accesso limitato (livello 3)
    /// Contiene solo le informazioni del proprietario
    /// </summary>
    public class LimitedAccessResponseModel
    {
        public int Id { get; set; }
        public int AccessLevel { get; set; } = 3;
        public OwnerInfoModel OwnerInfo { get; set; } = new OwnerInfoModel();
        public string EntityType { get; set; } = string.Empty; // "Property", "Request", "Customer"
    }
}
