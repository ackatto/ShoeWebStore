using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeWebStore.Models;
using ShoeWebStore.Data;

namespace ShoeWebStore.Pages
{
    public class ProductsModel : PageModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();

        public string Search { get; set; } = "";
        public int? SupplierId { get; set; }
        public string Sort { get; set; } = "ASC";
        public int CurrentPage{ get; set; }
        public bool IsAdmin => HttpContext.Session.GetString("Role") == "Администратор";
        public bool IsManager =>IsAdmin || HttpContext.Session.GetString("Role") == "Менеджер";
        public IActionResult OnGet(string search, int? supplierId, string sort, int pageNumber)
        {
            if (HttpContext.Session.GetString("Role") == null)
                return RedirectToPage("/Login");

            if (IsManager)
            {
                Search = search ?? "";
                SupplierId = supplierId;
                Sort = sort ?? "ASC";
            }

            // Защита от ввода отрицательных страниц
            CurrentPage = pageNumber < 1 ? 1 : pageNumber;

            // Размер страницы (по ТЗ 5 товаров)
            const int pageSize = 5;

            // Правильная математика: (страница - 1) * размер_страницы
            int offset = (CurrentPage - 1) * pageSize;

            // Для гостей и клиентов принудительно очищаем фильтры
            if (!IsManager)
            {
                Products = DatabaseHelper.GetProducts("", null, "ASC", offset);
            }
            else
            {
                Products = DatabaseHelper.GetProducts(Search, SupplierId, Sort, offset);
            }

            Suppliers = DatabaseHelper.GetSuppliers();
            return Page();
        }
    }
}
