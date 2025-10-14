namespace BackEnd.Models.InputModels
{
    public class SendFileModel
    {
        public IFormFile File {  get; set; }
        public string? FileName { get; set; }
        public string FolderName { get; set; }
    }
}
