using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Flush.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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
        /// The URL to redirect to on successful log in.
        /// </summary>
        public string ReturnUrl => Url.Content("/game");

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
        /// Attempt to register and log in a new player.
        /// </summary>
        /// <returns>
        /// A redirect action on success, else a page with errors set.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("The user attempted to login");

                var serviceProvider = HttpContext.RequestServices;
                var authenticationProvider = (AuthenticationProvider)
                    serviceProvider.GetService(typeof(AuthenticationProvider));

                var inputRoomLowercased = InputRoom.ToLowerInvariant();
                var useridentity = $"{InputName}@{inputRoomLowercased}";

                var result = await authenticationProvider.Register(new UserDetails()
                {
                    Username = useridentity,
                    Email = useridentity,
                });
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "A user by that name already exists in this room.");
                    return Page();
                }

                var token = await authenticationProvider.Login(new LoginCredentials()
                {
                    Email = useridentity,
                });
                if (token is null)
                {
                    ModelState.AddModelError(string.Empty, "We were unable to join that room.");
                    return Page();
                }

                return RedirectToPage("/Standard", new
                {
                    area = "Play",
                    r = inputRoomLowercased,
                    t = token
                });
            }

            // If we got this far, something failed, redisplay form
            _logger.LogError("The model state was not valid.");
            return Page();
        }
    }
}
