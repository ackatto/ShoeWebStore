using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoeWebStore.Data;
using ShoeWebStore.Models;
using System.Drawing;


public class ProductModel : PageModel
{
    private readonly IWebHostEnvironment _env;
    public ProductModel (IWebHostEnvironment env) { _env = env; }

    [BindProperty] public Product Item { get; set; } = new();
    [BindProperty] public IFormFile UploadedImage { get; set; }

    public bool IsEdit { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Manufacturers { get; set; } = new();
    public List<SelectListItem> Suppliers { get; set; } = new();

    public IActionResult OnGet(int? id)
    {
        if (HttpContext.Session.GetString("Role") != "Администратор")
            return RedirectToPage("/Products");

        LoadLists();

        if(id.HasValue)
        {
            IsEdit = true;
            var product = DatabaseHelper.GetProducts();
            Item = product.First(p => p.Id == id.Value);
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if (HttpContext.Session.GetString("Role") != "Администратор")
            return RedirectToPage("/Products");

        if (!ModelState.IsValid)
        {
            LoadLists();
            return Page();
        }

        // 1. Запоминаем путь к СТАРОЙ картинке (если мы редактируем товар)
        string oldImagePath = "";
        if (Item.Id != 0)
        {
            var oldProduct = DatabaseHelper.GetProducts().FirstOrDefault(p => p.Id == Item.Id);
            if (oldProduct != null)
                oldImagePath = oldProduct.ImagePath;
        }

        // 2. Если админ загрузил НОВУЮ картинку
        if (UploadedImage != null && UploadedImage.Length > 0)
        {
            string uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            string fileName = Guid.NewGuid() + Path.GetExtension(UploadedImage.FileName);
            string fullName = Path.Combine(uploads, fileName);

            // Сжимаем картинку до 300x200
            using (var originalImage = Image.FromStream(UploadedImage.OpenReadStream()))
            {
                using (var resizedImage = new Bitmap(originalImage, 300, 200))
                {
                    resizedImage.Save(fullName);
                }
            }

            // Обновляем путь в модели
            Item.ImagePath = "/uploads/" + fileName;

            // 3. УДАЛЯЕМ СТАРУЮ КАРТИНКУ С ДИСКА
            if (!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("picture.png"))
            {
                string oldPath = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
        }

        // 4. Сохраняем в БД
        if (Item.Id == 0)
            DatabaseHelper.InsertProduct(Item);
        else
            DatabaseHelper.UpdateProduct(Item);

        return RedirectToPage("/Products");
    }

    private void LoadLists()
    {
        Categories = DatabaseHelper.GetCategories()
            .Select(c => new SelectListItem(c.Name,c.Id.ToString())).ToList();
        Manufacturers = DatabaseHelper.GetManufacturers()
            .Select(m => new SelectListItem(m.Name, m.Id.ToString())).ToList();
        Suppliers = DatabaseHelper.GetSuppliers()
            .Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
    }
}
