namespace BackEnd.Models.LocationModels
{
    public class LocationSelectModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
    }
} 