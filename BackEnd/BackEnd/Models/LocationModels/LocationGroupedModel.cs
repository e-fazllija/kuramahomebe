namespace BackEnd.Models.LocationModels
{
    public class LocationGroupedModel
    {
        public string City { get; set; } = string.Empty;
        public List<LocationItemModel> Locations { get; set; } = new List<LocationItemModel>();
    }

    public class LocationItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
} 