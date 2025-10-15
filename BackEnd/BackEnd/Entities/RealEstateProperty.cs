using System.ComponentModel.DataAnnotations;

namespace BackEnd.Entities
{
    public class RealEstateProperty : EntityBase
    {
        [Required(ErrorMessage = "Il titolo è obbligatorio")]
        [MaxLength(200, ErrorMessage = "Il titolo non può superare i 200 caratteri")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "La categoria è obbligatoria")]
        [MaxLength(50, ErrorMessage = "La categoria non può superare i 50 caratteri")]
        public string Category { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "La tipologia non può superare i 100 caratteri")]
        public string? Typology { get; set; }
        public bool InHome { get; set; }
        public bool Highlighted { get; set; }
        public bool Auction { get; set; }
        public bool Sold { get; set; }
        public bool Archived { get; set; }
        public bool Negotiation { get; set; }
        [Required(ErrorMessage = "Lo stato è obbligatorio")]
        [MaxLength(50, ErrorMessage = "Lo stato non può superare i 50 caratteri")]
        public string Status { get; set; } = string.Empty;
        [Required(ErrorMessage = "L'indirizzo è obbligatorio")]
        [MaxLength(300, ErrorMessage = "L'indirizzo non può superare i 300 caratteri")]
        public string AddressLine { get; set; } = string.Empty;
        [Required(ErrorMessage = "La città è obbligatoria")]
        [MaxLength(100, ErrorMessage = "La città non può superare i 100 caratteri")]
        public string City { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "La località non può superare i 100 caratteri")]
        public string? Location { get; set; }
        [Required(ErrorMessage = "Lo stato/provincia è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Lo stato non può superare i 100 caratteri")]
        public string State { get; set; } = string.Empty;
        [Required(ErrorMessage = "Il CAP è obbligatorio")]
        [MaxLength(20, ErrorMessage = "Il CAP non può superare i 20 caratteri")]
        public string PostCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "La superficie commerciale è obbligatoria")]
        public int CommercialSurfaceate { get; set; }
        [MaxLength(20, ErrorMessage = "Il piano non può superare i 20 caratteri")]
        public string? Floor { get; set; }
        [Required(ErrorMessage = "Il numero totale di piani è obbligatorio")]
        public int TotalBuildingfloors { get; set; }
        public int Elevators { get; set; }
        [MaxLength(2000, ErrorMessage = "I dettagli aggiuntivi non possono superare i 2000 caratteri")]
        public string? MoreDetails { get; set; }
        public int Bedrooms { get; set; }
        public int WarehouseRooms { get; set; }
        public int Kitchens { get; set; }
        public int Bathrooms { get; set; }
        [MaxLength(100, ErrorMessage = "L'arredamento non può superare i 100 caratteri")]
        public string? Furniture { get; set; }
        [MaxLength(1000, ErrorMessage = "Le altre caratteristiche non possono superare i 1000 caratteri")]
        public string? OtherFeatures { get; set; }
        public int ParkingSpaces { get; set; }
        [MaxLength(100, ErrorMessage = "Il riscaldamento non può superare i 100 caratteri")]
        public string? Heating { get; set; }
        [MaxLength(50, ErrorMessage = "L'esposizione non può superare i 50 caratteri")]
        public string? Exposure { get; set; }
        [MaxLength(10, ErrorMessage = "La classe energetica non può superare i 10 caratteri")]
        public string? EnergyClass { get; set; }
        [MaxLength(100, ErrorMessage = "Il tipo di proprietà non può superare i 100 caratteri")]
        public string? TypeOfProperty { get; set; }
        [MaxLength(100, ErrorMessage = "Lo stato della proprietà non può superare i 100 caratteri")]
        public string? StateOfTheProperty { get; set; }
        public int YearOfConstruction { get; set; }
        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        public double Price { get; set; }
        public double PriceReduced { get; set; }
        public int MQGarden { get; set; }
        public double CondominiumExpenses { get; set; }
        [MaxLength(100, ErrorMessage = "La disponibilità non può superare i 100 caratteri")]
        public string? Availability { get; set; }
        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [MaxLength(5000, ErrorMessage = "La descrizione non può superare i 5000 caratteri")]
        public string Description { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "L'URL del video non può superare i 500 caratteri")]
        public string? VideoUrl { get; set; }
        public int AgreedCommission { get; set; }
        public int FlatRateCommission { get; set; }
        public int CommissionReversal { get; set; }
        [MaxLength(100, ErrorMessage = "Il tipo di incarico non può superare i 100 caratteri")]
        public string? TypeOfAssignment { get; set; }
        public DateTime AssignmentEnd { get; set; }
        public ICollection<RealEstatePropertyPhoto> Photos { get; set; } = new List<RealEstatePropertyPhoto>();
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        [MaxLength(450, ErrorMessage = "L'ID agente non può superare i 450 caratteri")]
        public string AgentId { get; set; } = string.Empty;
        public ApplicationUser Agent { get; set; }
        public ICollection<RealEstatePropertyNotes>? RealEstatePropertyNotes { get; set; }
    }
}
