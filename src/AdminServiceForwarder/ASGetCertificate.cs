using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AdminServiceForwarder
{
    public static class ASGetCertificate
    {
        public static X509Certificate2? GetCertificate()
        {
            var cert = GetFromStore(StoreName.My, StoreLocation.LocalMachine);
            if(cert == null)
            {
                return GetFromStore(StoreName.My, StoreLocation.CurrentUser);
            }
            return cert;
        }
        private static X509Certificate2? GetFromStore(StoreName storeName, StoreLocation storeLocation)
        {
            using var certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);
            foreach (var c in certStore.Certificates)
            {
                if (c.NotBefore < DateTime.UtcNow && c.NotAfter > DateTime.UtcNow)
                {
                    foreach (var ext in c.Extensions)
                    {
                        if (ext is X509EnhancedKeyUsageExtension keyUsage)
                        {
                            foreach (var oid in keyUsage.EnhancedKeyUsages)
                            {
                                if (oid.FriendlyName == "Server Authentication")
                                {
                                    return c;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
