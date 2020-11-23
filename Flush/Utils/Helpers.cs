using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Flush.Data.Game.InMemory;
using Flush.Data.Game.Model;
using Flush.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Flush.Utils
{
    /// <summary>
    /// Helpful methods.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets a boolean value indicating if we are in a development environment.
        /// </summary>
        public static bool DevelopmentMode =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ==
                Environments.Development;

        /// <summary>
        /// Derive a secret key using the certifate specified in
        /// the application settings.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm.</param>
        /// <param name="thumbprint">The certificate thumbprint.</param>
        /// <returns>
        /// A base64 encoded hash of the certificate private key.
        /// </returns>
        /// <remarks>
        /// Thumbprint should point to a certificate located in the
        /// localmachine/my store. The certificate should contain a private key.
        /// The resulting password shall be the hash of the raw private key
        /// bytes, using the hashing algorhtm specified by hashAlgorithm.
        ///
        /// This ensures that:
        /// * The initial password is not a plain-text.
        /// * The private key is never exposed.
        /// * The resulting secret key bytes are a result of SQLCipher's 64k
        ///   HMAC round.
        /// </remarks>
        public static string DeriveSecretKeyFromCertificate(string hashAlgorithm, string thumbprint)
        {
            // In dev mode, just use the thumbprint
            var key = thumbprint;

            if (!DevelopmentMode)
            {
                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var pvk = store.Certificates
                        .Find(X509FindType.FindByThumbprint, thumbprint, false)[0]
                        .PrivateKey
                        .ExportPkcs8PrivateKey();
                    using (var hash = HashAlgorithm.Create(hashAlgorithm))
                    {
                        var pvkHash = hash.ComputeHash(pvk);
                        key = BitConverter.ToString(pvkHash).Replace("-", "");
                    }
                }
            }

            Debug.WriteLineIf(
                DevelopmentMode,
                $"The secret key is \"{key}\"");
            return key;
        }
    }
}
