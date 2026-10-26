using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeWebStore.Pages
{
    public class GuestModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.SetString("FullName", "Гость");
            HttpContext.Session.SetString("Role", "Гость");
            return RedirectToPage("/Products");
        }
    }
}
