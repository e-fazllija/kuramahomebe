using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.RealEstatePropertyPhotoModels
{
    public class RealEstatePropertyPhotoSelectModel
    {
        public int Id { get; set; }
        public int RealEstatePropertyPhotoId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Type { get; set; }
        public bool Highlighted { get; set; }
        public int Position { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }

    }
}
