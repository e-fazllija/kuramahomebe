using System.ComponentModel.DataAnnotations;
using BackEnd.Models;

namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyListModel
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime AssignmentEnd { get; set; }
        public DateTime UpdateDate { get; set; }
        public int CommercialSurfaceate { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Typology { get; set; }
        public string? StateOfTheProperty { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool Auction { get; set; }
        public bool Sold { get; set; }
        public string? FirstPhotoUrl { get; set; }
        public string? AgencyId { get; set; }
        public string? AgentId { get; set; }
        public double EffectiveCommission { get; set; }
        
        // Livello di accesso (1=completo, 2=solo lettura, 3=limitato)
        public int AccessLevel { get; set; } = 1;
        
        // Informazioni del proprietario (popolato solo se AccessLevel == 3)
        public OwnerInfoModel? OwnerInfo { get; set; }
    }
} 