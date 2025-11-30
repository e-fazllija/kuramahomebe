namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyPublicListItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Typology { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double Price { get; set; }
        public int CommercialSurfaceate { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public bool Highlighted { get; set; }
        public bool Auction { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? MainPhotoUrl { get; set; }
    }
}

