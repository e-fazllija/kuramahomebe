using BackEnd.Entities;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.CalendarModels
{
    public class CalendarCreateModel
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        [Required]
        public string NomeEvento { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public int? RealEstatePropertyId { get; set; }
        public int? RequestId { get; set; }
        public string? DescrizioneEvento { get; set; }
        public string? LuogoEvento { get; set; }
        public string? Color { get; set; }
        public DateTime DataInizioEvento { get; set; }
        public DateTime DataFineEvento { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public bool Confirmed { get; set; }
        public bool Cancelled { get; set; }
        public bool Postponed { get; set; }
    }
}
