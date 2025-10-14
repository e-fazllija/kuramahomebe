namespace BackEnd.Models.RealEstatePropertyModels
{
    public class UploadFilesModel
    {
        public List<IFormFile> Files { get; set; }
        public int PropertyId { get; set; }
    }
}
