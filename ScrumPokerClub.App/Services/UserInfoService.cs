using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ScrumPokerClub.Services
{
    /// <summary>
    /// Provides details of the connected user via a HTTP Context instance.
    /// </summary>
    class UserInfoService : IUserInfoService
    {
        /// <inheritdoc/>
        public string Name { get; init; }

        /// <inheritdoc/>
        public string Identifier { get; init; }

        /// <summary>
        /// Initialises an instance of <see cref="UserInfoService"/>.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP Context Accessor.</param>
        public UserInfoService(IHttpContextAccessor httpContextAccessor)
        {
            var context = httpContextAccessor.HttpContext;
            var claims = context.User?.Claims ?? throw new ArgumentNullException(nameof(context.User));

            Name = claims.FirstOrDefault(c => c.Type == "name")?.Value ??
                throw new SecurityException("Microsoft Identity Platform did not provider a NameIdentifier claim.");

            Identifier = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ??
                throw new SecurityException("Microsoft Identity Platform did not provider an ObjectIdentifier (OID) claim.");
        }
    }
}
