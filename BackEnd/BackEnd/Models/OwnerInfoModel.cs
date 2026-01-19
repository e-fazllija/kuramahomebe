namespace BackEnd.Models
{
    /// <summary>
    /// Modello per le informazioni del proprietario di un'entità (usato per livello 3)
    /// </summary>
    public class OwnerInfoModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin", "Agency", "Agent"
        public string? AgencyName { get; set; } // Nome dell'Agency se il proprietario è un Agent
    }
}
