using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoeWebStore.Data;
using ShoeWebStore.Models;

namespace ShoeWebStore.Pages
{
    public class OrderModel : PageModel
    {
        [BindProperty] public Order Item { get; set; } = new();
        public bool IsEdit { get; set; }
        public List<SelectListItem> UsersList { get; set; } = new();

        public IActionResult OnGet(int? id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");

            // Загружаем список клиентов из БД
            UsersList = DatabaseHelper.GetUsers()
                .Select(u => new SelectListItem(u.FullName, u.Id.ToString())).ToList();

            if (id.HasValue)
            {
                IsEdit = true;
                Item = DatabaseHelper.GetOrders().FirstOrDefault(o => o.Id == id.Value);
                if (Item == null) return RedirectToPage("/Orders");
            }
            else
            {
                // Если это создание нового заказа, ставим сегодняшнюю дату
                Item.OrderDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");

            if (!ModelState.IsValid)
            {
                // Если ошибка валидации, нужно заново загрузить список клиентов для выпадающего списка
                UsersList = DatabaseHelper.GetUsers()
                    .Select(u => new SelectListItem(u.FullName, u.Id.ToString())).ToList();
                return Page();
            }

            if (Item.Id == 0)
                DatabaseHelper.InsertOrder(Item);
            else
                DatabaseHelper.UpdateOrder(Item);

            return RedirectToPage("/Orders");
        }
    }
}