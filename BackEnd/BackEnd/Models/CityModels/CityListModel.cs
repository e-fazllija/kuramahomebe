namespace BackEnd.Models.CityModels
{
    public class CityListModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public int LocationsCount { get; set; }
    }
}
