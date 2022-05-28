namespace MEMConsole.Client.Models
{
    public static class MEMAuthenticationState
    {
        public static bool UsingAuthentication { get; set; } = false;
        public static string AuthorizedUrl { get; set; } = "";
        public static string Scopes { get; set; } = "";
    }
}
