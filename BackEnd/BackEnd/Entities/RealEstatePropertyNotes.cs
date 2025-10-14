namespace BackEnd.Entities
{
    public class RealEstatePropertyNotes: EntityBase
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public int? CalendarId { get; set; }
        public int RealEstatePropertyId { get; set; }
        public RealEstateProperty RealEstateProperty { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
