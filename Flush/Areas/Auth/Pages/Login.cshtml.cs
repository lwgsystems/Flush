using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Flush.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Flush.Areas.Auth.Pages
{
    /// <summary>
    /// A model for the login page, supporting registration, token procurement
    /// and redirection.
    /// </summary>
    public class LoginModel : PageModel
    {
        [Required]
        [Display(Name = "Room Name")]
        [BindProperty]
        public string InputRoom { get; set; }

        [Required]
        [Display(Name = "What's Your Name?")]
        [BindProperty]
        public string InputName { get; set; }

        /// <summary>
        /// An error message returned during post, if applicable.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// A logger for use by this model.
        /// </summary>
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Construct an instance of the LoginModel.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IActionResult> OnGetAsync(string r, string n)
        {
            ViewData["Title"] = "Authorisation";

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            /*
             * We can populate these even if r/n are null. They'll just appear
             * empty to the user.
             */
            InputRoom = r;
            InputName = n;

            await Task.CompletedTask;
            return Page();
        }

        /// <summary>
        /// Peforms guest user registration and generates a JWT.
        /// </summary>
        /// <returns>A JSON response containing a JWT.</returns>
        public async Task<IActionResult> OnPostAcquireTokenAsync()
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("The user attempted to login");

                var serviceProvider = HttpContext.RequestServices;
                var authenticationProvider = (AuthenticationProvider)
                    serviceProvider.GetService(typeof(AuthenticationProvider));

                // TODO: This is fragile. Make stronger in Elderberry.
                var userNameEscaped = InputName.Replace(" ", "+");
                var inputRoomLowercased = InputRoom.ToLowerInvariant();
                var email = $"{InputName.ToLowerInvariant().Replace(" ", "")}@{inputRoomLowercased}";

                var result = await authenticationProvider.Register(new UserDetails()
                {
                    Username = userNameEscaped,
                    Email = email,
                });
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "A user by that name already exists in this room.");
                    return Page();
                }

                var token = await authenticationProvider.Login(new LoginCredentials()
                {
                    Email = email,
                });
                if (token is null)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred when trying to join the room.");
                    return Page();
                }

                // Redirect back, with the token.
                ViewData["Token"] = token;
                return Page();
            }

            // If we got this far, something failed, redisplay form
            _logger.LogError("The model state was not valid.");
            return Page();
        }

        /// <summary>
        /// Redirects to the game.
        /// </summary>
        /// <returns>A redirection to the game.</returns>
        public async Task<IActionResult> OnPostAuthorisedRedirectAsync()
        {
            await Task.CompletedTask;
            return RedirectToPage("/Standard", new { area = "Play" });
        }
    }
}
