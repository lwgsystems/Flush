using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

namespace Flush.Pages
{
    /// <summary>
    /// The index page model.
    /// </summary>
    /// <remarks>
    /// The index page is a simple redirect.
    /// </remarks>
    public class IndexModel : PageModel
    {
        /// <summary>
        /// HTTP GET handler.
        /// </summary>
        /// <returns>A Redirect to the login page.</returns>
        public async Task<IActionResult> OnGet()
        {
            string room = null;
            if (HttpContext.Request.Query.TryGetValue("r", out StringValues valueR))
                room = valueR;
            else if (HttpContext.Request.Query.TryGetValue("room", out StringValues valueRoom))
                room = valueRoom;

            // Redirect to /Auth/Login, normalising the room parameter
            // (if given) in the process.
            await Task.CompletedTask;
            return RedirectToPage("/login", new
            {
                // we can redirect to a page in an area, but we have to specify
                // the area as a route value, rather than as part of the route.
                area = "auth",

                // normalised room name
                r = room
            });
        }
    }
}
