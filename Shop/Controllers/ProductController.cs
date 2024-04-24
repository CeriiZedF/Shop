using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shop.Filters;
using Shop.ViewModels;
using Shop.Helpers;

using Shop.Models;
using Shop.DAL.Repository.IRepository;

namespace Shop.Controllers
{
    [CountRequests]
    [Authorize(Policy = WC.AdminManagerPolicy)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }


        // READ
        public async Task<IActionResult> Index(int numPage)
        {
            if (numPage <= 0) { numPage = 0; }
            const int pageSize = 2;

            var products = await _productRepository.GetAll(includeProperties: "Category,ProductUsage");
            ProductVM producVM = new()
            {
                Products = products,
                Pager = new Pager(products.Count(), numPage, pageSize)
            };

            return View(producVM);
        }


        // CREATE
        public IActionResult Create()
        {
            ViewData["CategoryItems"] = _productRepository.GetAllDropDownList("Category");
            ViewData["ProductUsageItems"] = _productRepository.GetAllDropDownList("ProductUsage");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (product.Category is not null && !ModelState.IsValid) {
                return RedirectToAction(nameof(Create));
            }

            // отправлена ли картинка
            var files = HttpContext.Request.Form.Files;
            if (files.Count != 0)
            {
                string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);
                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);
                }
                product.Image = fileName + extension;
            }

            await _productRepository.Add(product);
            await _productRepository.Save();
            return RedirectToAction(nameof(Index));
        }


        // UPDATE
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) { return NotFound(); }

            var product = await _productRepository.FirstOrDefault(p => p.Id == id);
            if (product is null) { return NotFound(); }

            ViewData["CategoryItems"] = _productRepository.GetAllDropDownList("Category");
            ViewData["ProductUsageItems"] = _productRepository.GetAllDropDownList("ProductUsage");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product? product)
        {
            if (product is null) { return NotFound(); }
            try
            {
                // находим продукт
                Product? p = await _productRepository.FirstOrDefault(p => p.Id == product.Id);
                if (p is null) { return NotFound(); }

                var files = HttpContext.Request.Form.Files;  // отправленные файлы
                product.Image = p.Image;  // картинка которая была до обновления
                if (files.Count > 0)
                {
                    string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
                    // если картинка была, то удаляем
                    if (p.Image is not null)
                    {
                        string oldFile = Path.Combine(upload, p.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }
                    // загружаем новую
                    string filename = Guid.NewGuid().ToString();
                    string ext = Path.GetExtension(files[0].FileName);
                    using (FileStream fs = new(Path.Combine(upload, filename + ext), FileMode.Create))
                    {
                        await files[0].CopyToAsync(fs);
                    }
                    product.Image = filename + ext;
                }

                _productRepository.Update(product);

                await _productRepository.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction(nameof(Edit));
            }
        }


        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) { return NotFound(); }

            var product = await _productRepository.FirstOrDefault(
                m => m.Id == id,
                includeProperties: "Category"
            );
            if (product is null) { return NotFound(); }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(Product? product)
        {
            if (product is null) { return NotFound(); }
            _productRepository.Remove(product);
            await _productRepository.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
