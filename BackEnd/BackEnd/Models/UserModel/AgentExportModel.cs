using System;

namespace BackEnd.Models.UserModel
{
    public class AgentExportModel
    {
        public string Format { get; set; } = "excel";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? AgencyId { get; set; }
        public bool? OnlyActive { get; set; }
        public string? Search { get; set; }
    }
}

