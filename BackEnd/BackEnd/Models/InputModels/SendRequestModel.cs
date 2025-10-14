namespace BackEnd.Models.InputModels
{
    public class SendRequestModel
    {
        public string RequestType { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string NumberRooms { get; set; } = string.Empty;
        public string NumberBedRooms { get; set; } = string.Empty;
        public string NumberServices { get; set; } = string.Empty;
        public string MQ { get; set; }
        public bool Garden { get; set; }
        public bool Terrace { get; set; }
        public bool Lift { get; set; }
        public bool Furnished { get; set; }
        public string Heating { get; set; } = string.Empty;
        public string Box { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Information { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
    }
}
