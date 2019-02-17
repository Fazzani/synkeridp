namespace SynkerIdpAdminUI.STS.Identity.Configuration
{
    public class StsAuthentificationConfiguration
    {
        public GoogleOptions Google { get; set; }
    }
    public class GoogleOptions
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
    }
}
