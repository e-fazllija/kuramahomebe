using System;

namespace BackEnd.Models.CalendarModels
{
    public class CalendarExportModel
    {
        public string Format { get; set; } = "excel";
        public string? Filter { get; set; }
        public char? FromName { get; set; }
        public char? ToName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? AgencyId { get; set; }
        public string? AgentId { get; set; }
    }
}
