namespace EcommerceApp.Services
{
    public class EmailSettings
    {
        public string ApiKey { get; set; } = "";
        public string From { get; set; } = "";
        public string DisplayName { get; set; } = "Z-Commerce";

        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
    }
}