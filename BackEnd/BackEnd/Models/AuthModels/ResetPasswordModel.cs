using DocumentFormat.OpenXml.Spreadsheet;

namespace BackEnd.Models.AuthModels
{
    public class ResetPasswordModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
