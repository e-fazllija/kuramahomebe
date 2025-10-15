using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.CityModels
{
    public class CityUpdateModel
    {
        [Required(ErrorMessage = "L'ID città è obbligatorio")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Il nome della città è obbligatorio")]
        [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'ID provincia è obbligatorio")]
        public int ProvinceId { get; set; }
    }
}