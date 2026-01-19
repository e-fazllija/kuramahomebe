using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyCreateModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tipologia obbligatoria")]
        public string Typology { get; set; } = string.Empty;
        public bool InHome { get; set; }
        public bool Highlighted { get; set; }
        public bool Auction { get; set; }
        public bool Sold { get; set; }
        public bool Negotiation { get; set; }
        public bool Archived { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public string AddressLine { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        public string? Location { get; set; }
        [Required]
        public string State { get; set; } = string.Empty;
        [Required]
        public string PostCode { get; set; } = string.Empty;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Superficie obbligatoria")]
        public int CommercialSurfaceate { get; set; }
        public string? Floor { get; set; }
        [Required]
        public int TotalBuildingfloors { get; set; }
        public int Elevators { get; set; }
        public string? MoreDetails { get; set; }
        public string? MoreFeatures { get; set; }
        public int Bedrooms { get; set; }
        public int WarehouseRooms { get; set; }
        public int Kitchens { get; set; }
        public int Bathrooms { get; set; }
        public string? Furniture { get; set; }
        public string? OtherFeatures { get; set; }
        public int ParkingSpaces { get; set; }
        public string? Heating { get; set; }
        public string? Exposure { get; set; }
        [Required(ErrorMessage = "Classe energetica obbligatoria")]
        public string EnergyClass { get; set; } = string.Empty;
        public string? TypeOfProperty { get; set; }
        [Required(ErrorMessage = "Stato dell'immobile obbligatorio")]
        public string StateOfTheProperty { get; set; } = string.Empty;
        [Range(1000, 3000, ErrorMessage = "Anno di costruzione obbligatorio")]
        public int YearOfConstruction { get; set; }
        [Required]
        public double Price { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Il prezzo ribassato non può essere negativo")]
        [CustomValidation(typeof(RealEstatePropertyCreateModel), "ValidatePriceReduced")]
        public double PriceReduced { get; set; }
        public int MQGarden { get; set; }
        public double CondominiumExpenses { get; set; }
        public string? Availability { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public DateTime AssignmentEnd { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [Range(1, int.MaxValue, ErrorMessage = "Cliente obbligatorio")]
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Agente obbligatorio")]
        public string AgentId { get; set; } = string.Empty;
        public string? TypeOfAssignment { get; set; }
        public int AgreedCommission { get; set; }
        public int FlatRateCommission { get; set; }
        public int CommissionReversal { get; set; }
        public double EffectiveCommission { get; set; }

        /// <summary>
        /// Valida che PriceReduced non sia maggiore di Price
        /// </summary>
        public static ValidationResult? ValidatePriceReduced(double priceReduced, ValidationContext context)
        {
            var model = (RealEstatePropertyCreateModel)context.ObjectInstance;
            if (priceReduced > 0 && priceReduced > model.Price)
            {
                return new ValidationResult("Il prezzo ribassato non può essere maggiore del prezzo originale");
            }
            return ValidationResult.Success;
        }
    }
}
