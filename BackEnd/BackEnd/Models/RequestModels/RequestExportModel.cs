using System;
using System.Collections.Generic;

namespace BackEnd.Models.RequestModels
{
    public class RequestExportModel
    {
        public string Format { get; set; } = "excel";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
        public string? Contract { get; set; }
        public List<string>? PropertyTypes { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
    }
}

