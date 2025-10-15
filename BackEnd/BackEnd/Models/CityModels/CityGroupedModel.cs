namespace BackEnd.Models.CityModels
{
    public class CityGroupedModel
    {
        public string Province { get; set; } = string.Empty;
        public List<CityItemModel> Cities { get; set; } = new List<CityItemModel>();
    }

    public class CityItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
