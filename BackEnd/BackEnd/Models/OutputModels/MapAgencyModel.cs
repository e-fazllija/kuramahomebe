namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello leggero per le agenzie nella mappa
    /// Contiene solo i campi necessari per geocoding e visualizzazione
    /// </summary>
    public class MapAgencyModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AdminId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}








