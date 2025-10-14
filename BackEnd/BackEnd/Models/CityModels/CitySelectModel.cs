namespace BackEnd.Models.CityModels
{
    public class CitySelectModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
    }
} 