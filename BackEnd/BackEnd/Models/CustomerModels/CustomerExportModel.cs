namespace BackEnd.Models.CustomerModels
{
    public class CustomerExportModel
    {
        public string? Filter { get; set; }
        public char? FromName { get; set; }
        public char? ToName { get; set; }
    }
}
