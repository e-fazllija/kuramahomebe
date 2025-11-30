using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models.RealEstatePropertyModels
{
    public class RealEstatePropertyPublicDetailModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Typology { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string State { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
        public int CommercialSurfaceate { get; set; }
        public string? Floor { get; set; }
        public int TotalBuildingfloors { get; set; }
        public int Elevators { get; set; }
        public string? MoreDetails { get; set; }
        public string? MoreFeatures { get; set; }
        public int Bedrooms { get; set; }
        public int WarehouseRooms { get; set; }
        public int Kitchens { get; set; }
        public int Bathrooms { get; set; }
        public string? Furniture { get; set; }
        public string? OtherFeatures { get; set; }
        public int ParkingSpaces { get; set; }
        public string? Heating { get; set; }
        public string? Exposure { get; set; }
        public string? EnergyClass { get; set; }
        public string? TypeOfProperty { get; set; }
        public string? StateOfTheProperty { get; set; }
        public int YearOfConstruction { get; set; }
        public double Price { get; set; }
        public double PriceReduced { get; set; }
        public int MQGarden { get; set; }
        public double CondominiumExpenses { get; set; }
        public string? Availability { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public bool Highlighted { get; set; }
        public bool Auction { get; set; }
        public DateTime CreationDate { get; set; }
        public List<PropertyPhotoModel> Photos { get; set; } = new List<PropertyPhotoModel>();
        
        // Agenzia
        public AgencyContactModel? Agency { get; set; }
        
        // Agente (se presente)
        public AgentContactModel? Agent { get; set; }
    }

    public class PropertyPhotoModel
    {
        public string Url { get; set; } = string.Empty;
        public int Position { get; set; }
    }

    public class AgencyContactModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MobilePhone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }
    }

    public class AgentContactModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MobilePhone { get; set; }
    }
}

