using System;

namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyExportModel
    {
        public string Format { get; set; } = "excel";
        public string? Filter { get; set; }
        public char? FromName { get; set; }
        public char? ToName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Contract { get; set; }
        public int? PriceFrom { get; set; }
        public int? PriceTo { get; set; }
        public string? Category { get; set; }
        public string? Typologie { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public bool? Sold { get; set; }
        public bool? Auction { get; set; }
        public string? Status { get; set; }
        public string? AgencyId { get; set; }
        public string? AgentId { get; set; }
    }
}
