using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeWebStore.Data;

namespace ShoeWebStore.Pages
{
    public class DeleteOrderModel : PageModel
    {
        public int OrderId { get; set; }

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");

            OrderId = id;
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");

            DatabaseHelper.DeleteOrder(id);
            return RedirectToPage("/Orders");
        }
    }
}