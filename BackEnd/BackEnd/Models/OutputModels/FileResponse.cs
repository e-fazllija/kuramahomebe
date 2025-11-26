namespace BackEnd.Models.OutputModels
{
    public class FileResponse
    {
        public FileResponse(Stream stream, string contentType, string name)
        {
            _stream = stream;
            _contentType = contentType;
            _name = name;
        }
        public Stream _stream { get; set; }
        public string _contentType { get; set; }
        public string _name { get; set; }
    }
}
