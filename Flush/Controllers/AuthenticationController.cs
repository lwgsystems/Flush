using System.Threading.Tasks;
using Flush.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Flush.Controllers
{
    /// <summary>
    /// Provides an API facade to the Authentication Provider.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        /// <summary>
        /// The underlying authentication provider.
        /// </summary>
        private readonly AuthenticationProvider _provider;

        /// <summary>
        /// A logger for use by this instance.
        /// </summary>
        private readonly ILogger<AuthenticationController> _logger;

        /// <summary>
        /// Create a new instance of the AuthenticationController
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="provider">An instance of the AuthenticationProvider</param>
        public AuthenticationController(ILogger<AuthenticationController> logger, AuthenticationProvider provider)
        {
            _provider = provider;
            _logger = logger;
        }

        /// <summary>
        /// Register the user.
        /// </summary>
        /// <param name="userDetails">The user details.</param>
        /// <returns>OK on successful registration, else BadRequest.</returns>
        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDetails userDetails)
        {
            if (!ModelState.IsValid || userDetails == null)
            {
                return new BadRequestObjectResult(new { Message = "User Registration Failed" });
            }

            var result = await _provider.Register(userDetails);
            if (!result.Succeeded)
            {
                var dictionary = new ModelStateDictionary();
                foreach (IdentityError error in result.Errors)
                {
                    dictionary.AddModelError(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "User Registration Failed", Errors = dictionary });
            }

            return Ok(new { Message = "User Reigstration Successful" });
        }

        /// <summary>
        /// Log in the user.
        /// </summary>
        /// <param name="credentials">The user credentials.</param>
        /// <returns>OK on successful log in, else BadRequest</returns>
        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCredentials credentials)
        {
            object token;
            if (!ModelState.IsValid
                || credentials == null
                || (token = await _provider.Login(credentials)) != null)
            {
                return new BadRequestObjectResult(new { Message = "Login failed" });
            }

            return Ok(new { Token = token, Message = "Success" });
        }

        /// <summary>
        /// Log out the user.
        /// </summary>
        /// <param name="credentials">The user credentials.</param>
        /// <returns>OK on succesfully logout, else BadRequest.</returns>
        [HttpPost]
        [Route("Logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(LoginCredentials credentials)
        {
            _logger.LogCritical("Logout has been called.");
            var result = await _provider.Logout(credentials);
            if (result)
            {
                return Ok(new
                {
                    Token = "",
                    Messaged = "Logged Out"
                });
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
