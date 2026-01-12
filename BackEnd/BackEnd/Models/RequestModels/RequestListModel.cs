using System.ComponentModel.DataAnnotations;
using BackEnd.Models;

namespace BackEnd.Models.RequestModels
{
    public class RequestListModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Contract { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public string City { get; set; } = string.Empty;
        public double PriceTo { get; set; }
        public double PriceFrom { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public bool Archived { get; set; }
        public bool Closed { get; set; }
        public string? UserId { get; set; }
        
        // Livello di accesso (1=completo, 2=solo lettura, 3=limitato)
        public int AccessLevel { get; set; } = 1;
        
        // Informazioni del proprietario (popolato solo se AccessLevel == 3)
        public OwnerInfoModel? OwnerInfo { get; set; }
    }
} 