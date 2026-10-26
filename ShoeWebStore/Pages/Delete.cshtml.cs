using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeWebStore.Data;
using ShoeWebStore.Models;

namespace ShoeWebStore.Pages
{
    public class DeleteModel : PageModel
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = "";

        public string Error { get; set; }

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Products");

            ProductId = id;
            var product = DatabaseHelper.GetProducts().FirstOrDefault(p=>p.Id==id);
            if(product==null) return RedirectToPage("/Products");
            ProductName = product.Name;

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Products");
            try
            {
                DatabaseHelper.DeleteProduct(id);
                return RedirectToPage("/Products");
            }
            catch(InvalidOperationException ex)
            {
                ProductId = id;
                Error = ex.Message;
                ProductName = DatabaseHelper.GetProducts().FirstOrDefault(p=>p.Id==id)?.Name ??"";
                return Page();
            }

        }
    }
}
