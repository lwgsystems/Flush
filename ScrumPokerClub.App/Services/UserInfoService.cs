using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

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

        /// <inheritdoc/>
        public string Email { get; init; }

        /// <summary>
        /// Initialises an instance of <see cref="UserInfoService"/>.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP Context Accessor.</param>
        public UserInfoService(ILogger<UserInfoService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            var claims = httpContextAccessor.HttpContext.User?.Claims;
            if (claims is null)
            {
                var exception = new SpcSecurityException("User is not authourised.");
                logger.LogError(exception, "An error occurred during authorisation.");
                throw exception;
            }

            var name = claims.FirstOrDefault(c => c.Type == "name")?.Value;
            if (name is null)
            {
                var exception = new SpcIdentityException("Microsoft Identity Platform did not provide a NameIdentifier claim.");
                logger.LogError(exception, "An error occurred during authorisation.");
                return;
            }
            Name = name;

            var identifier = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (identifier is null)
            {
                var exception = new SpcIdentityException("Microsoft Identity Platform did not provide an ObjectIdentifier (OID) claim.");
                logger.LogError(exception, "An error occurred during authorisation.");
                return;
            }
            Identifier = identifier;

            var email = claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            if (email is null)
            {
                var exception = new SpcIdentityException("Microsoft Identity Platform did not provide an EmailAddress claim.");
                logger.LogError(exception, "An error occurred during authorisation.");
                return;
            }
            Email = email;
        }
    }
}
