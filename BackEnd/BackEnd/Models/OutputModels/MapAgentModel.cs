namespace BackEnd.Models.OutputModels
{
    /// <summary>
    /// Modello leggero per gli agenti nel filtro
    /// Contiene solo i campi necessari per il dropdown
    /// </summary>
    public class MapAgentModel
    {
        public string Id { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}









