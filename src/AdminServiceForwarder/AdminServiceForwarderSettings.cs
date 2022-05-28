namespace AdminServiceForwarder
{
    /// <summary>
    /// Settings class for DI to pass appsettings info
    /// </summary>
    public class AdminServiceForwarderSettings
    {
        /// <summary>
        /// All origins that are allowed for CORS requests
        /// </summary>
        public string[] AllowedCORSOrigins { get; set; } = default!;
        /// <summary>
        /// Should WindowsAuth be enabled? If disabled, will have to use AzureAD auth
        /// </summary>
        public bool EnableWindowsAuth { get; set; } = true;
        /// <summary>
        /// Should we disable SSL checks? If AdminServiceServer name is different than the cert assigned, this should be true
        /// </summary>
        public bool DisableSSLCertificateValidation { get; set; } = false;
        /// <summary>
        /// What is the AdminService server? Localhost is the default
        /// </summary>
        public string AdminServiceServer { get; set; } = "localhost";
    }
}
