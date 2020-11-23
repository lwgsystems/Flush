using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Flush.Data.Identity.Model;
using Flush.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Flush.Providers
{
    /// <summary>
    /// Models a user login request.
    /// </summary>
    public class LoginCredentials
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password => "P@ssw0rd";
    }

    /// <summary>
    /// Models a user registration request.
    /// </summary>
    public class UserDetails : LoginCredentials
    {
        [Required]
        public string Username { get; set; }
    }

    /// <summary>
    /// A provider for ASP.NET Core Identity backed authentication services.
    /// </summary>
    public class AuthenticationProvider
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthenticationProvider> _logger;

        /// <summary>
        /// Create an instance of the AuthenticationProvider.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="jwtOptions">The JWT generation options.</param>
        /// <param name="userManager">The user manager.</param>
        public AuthenticationProvider(ILogger<AuthenticationProvider> logger,
            IOptions<JwtOptions> jwtOptions,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }

        #region Public API

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="userDetails">The request.</param>
        /// <returns>A result indicating the success status.</returns>
        public async Task<IdentityResult> Register(UserDetails userDetails)
        {
            _logger.LogDebug($"Registering {userDetails}");
            var identityUser = new ApplicationUser() { UserName = userDetails.Username, Email = userDetails.Email };
            return await _userManager.CreateAsync(identityUser, userDetails.Password);
        }

        /// <summary>
        /// Log-in a user.
        /// </summary>
        /// <param name="credentials">The request.</param>
        /// <returns>A JWT Bearer token if successful, else null.</returns>
        public async Task<object> Login(LoginCredentials credentials)
        {
            IdentityUser identityUser;

            if ((identityUser = await ValidateUser(credentials)) == null)
            {
                return null;
            }

            var token = GenerateToken(identityUser);
            return token;
        }

        /// <summary>
        /// Log out a user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>True if successful, else false.</returns>
        public async Task<bool> Logout(string user)
        {
            _logger.LogDebug("user is logging out of the application");

            var findResult = await _userManager.FindByEmailAsync(user.Replace('_', '@'));
            if (findResult is null)
            {
                _logger.LogDebug("The user doesn't exist");
                return false;
            }

            var deleteResult = await _userManager.DeleteAsync(findResult);
            if (!deleteResult.Succeeded)
            {
                _logger.LogDebug("we were unable to remove the user when they logged out.");
                return false;
            }

            return true;
        }

        #endregion

        #region Token generation and validation

        /// <summary>
        /// Validate the given log in credentials.
        /// </summary>
        /// <param name="credentials">The request.</param>
        /// <returns>A validated user object on success, else null.</returns>
        private async Task<IdentityUser> ValidateUser(LoginCredentials credentials)
        {
            var identityUser = await _userManager.FindByEmailAsync(credentials.Email);
            if (identityUser != null)
            {
                var result = _userManager.PasswordHasher.VerifyHashedPassword(identityUser, identityUser.PasswordHash, credentials.Password);
                return result == PasswordVerificationResult.Failed ? null : identityUser;
            }

            return null;
        }

        /// <summary>
        /// Generate a JWT Bearer token.
        /// </summary>
        /// <param name="identityUser">
        /// The ASP.NET Core Identity user to generate a token for.
        /// </param>
        /// <returns>The JWT Bearer token.</returns>
        private object GenerateToken(IdentityUser identityUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var skey = Helpers.DeriveSecretKeyFromCertificate(_jwtOptions.HashAlgorithm,
                _jwtOptions.Thumbprint);
            var key = Encoding.Default.GetBytes(skey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, identityUser.UserName),
                    new Claim(ClaimTypes.Email, identityUser.Email),
                    new Claim(ClaimTypes.NameIdentifier, identityUser.Email.Replace('@','_'))
                }),

                Expires = DateTime.UtcNow.AddSeconds(3600),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Audience = "Flush",
                Issuer = "Flush"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
}
