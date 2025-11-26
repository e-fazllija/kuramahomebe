using System;

namespace BackEnd.Models.CustomerModels
{
    public class CustomerExportModel
    {
        public string Format { get; set; } = "excel";
        public string? Filter { get; set; }
        public char? FromName { get; set; }
        public char? ToName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Type { get; set; }
        public bool? GoldCustomer { get; set; }
        public string? OwnerId { get; set; }
    }
}
