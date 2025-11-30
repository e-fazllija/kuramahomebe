namespace BackEnd.Interfaces.IBusinessServices
{
    public interface IIdealistaService
    {
        Task<string?> GetAccessTokenAsync(string clientId, string clientSecret);
        Task<int?> CreatePropertyAsync(string accessToken, string feedKey, object propertyData);
        Task<bool> UpdatePropertyAsync(string accessToken, string feedKey, int propertyId, object propertyData);
        Task<bool> DeactivatePropertyAsync(string accessToken, string feedKey, int propertyId);
        Task<bool> UpdatePropertyImagesAsync(string accessToken, string feedKey, int propertyId, List<string> imageUrls, string propertyType = "flat");
        Task<bool> DeletePropertyImagesAsync(string accessToken, string feedKey, int propertyId);
    }
}

