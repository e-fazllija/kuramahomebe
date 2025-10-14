using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.RealEstatePropertyPhotoModels
{
    public class RealEstatePropertyPhotoCreateModel
    {
        public int RealEstatePropertyPhotoId { get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string Url { get; set; } = string.Empty;
        [Required]
        public int Type { get; set; }
        public bool Highlighted { get; set; }
        [Required]
        public int Position { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;

    }
}
