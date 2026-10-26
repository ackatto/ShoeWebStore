using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeWebStore.Data;
using ShoeWebStore.Models;

namespace ShoeWebStore.Pages
{
    public class OrdersModel : PageModel
    {
        public List<Order> Orders { get; set; } = new();
        public bool IsAdmin => HttpContext.Session.GetString("Role") == "Администратор";
        public bool IsManager => IsAdmin || HttpContext.Session.GetString("Role") == "Менеджер";

        public IActionResult OnGet()
        {
            // Заказы видят только менеджер и админ
            if (!IsManager)
                return RedirectToPage("/Products");

            Orders = DatabaseHelper.GetOrders();
            return Page();
        }
    }
}