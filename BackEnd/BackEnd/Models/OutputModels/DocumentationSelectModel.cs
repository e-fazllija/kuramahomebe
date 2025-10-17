namespace BackEnd.Models.OutputModels
{
    public class DocumentationSelectModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public bool IsFolder { get; set; }
        public bool IsPrivate { get; set; }
        public string? ParentPath { get; set; }
        public string? DisplayName { get; set; }
        public string? AgencyId { get; set; }
        public string? UserId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
