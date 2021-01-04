using System;
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
    }
}
