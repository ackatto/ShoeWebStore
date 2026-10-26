using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeWebStore.Data;
namespace ShoeWebStore.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty] public string Login { get; set; }
        [BindProperty] public string Password { get; set; }

        public void OnGet()
        {
            

        }
        public IActionResult OnPost()
        {
            if(string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("","Введите логин и пароль.");
                return Page();
            }

            var user = DatabaseHelper.GetUser(Login, Password);

            if (user==null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль.");
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToPage("/Products");
        }
    }
}

