namespace Shop.Areas.API
{
    public class JWTSettings
    {
        public string SecretKey { get; set; } = String.Empty;
        public string Issure { get; set; } = String.Empty;
        public string Audience { get; set; } = String.Empty;
    }
}
