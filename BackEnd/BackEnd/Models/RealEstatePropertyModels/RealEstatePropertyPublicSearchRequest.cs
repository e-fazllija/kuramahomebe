namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyPublicSearchRequest
    {
        public string? Keyword { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Category { get; set; }
        public string? Typology { get; set; }
        public string? Status { get; set; }
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}

