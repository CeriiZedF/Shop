using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shop.DAL.Repository.IRepository;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize(Policy = WC.AdminManagerPolicy)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }


        // READ
        public async Task<IActionResult> Index()
        {
            return View(await _categoryRepository.GetAll());
        }


        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid || category is null) { return View(); }
            _categoryRepository.Add(category);
            _categoryRepository.Save();
            return RedirectToAction(nameof(Index));
        }


        // UPDATE
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) { return NotFound(); }
            Category? category = await _categoryRepository.FirstOrDefault(c => c.Id == id);
            if (category is null) { return NotFound(); }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid) { return View(); }
            await _categoryRepository.Update(category);
            await _categoryRepository.Save();
            return RedirectToAction(nameof(Index));
        }


        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) { return NotFound(); }
            Category? category = await _categoryRepository.FirstOrDefault(c => c.Id == id);
            if (category is null) { return NotFound(); }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(Category category)
        {
            if (category is null) { return NotFound(); }
            _categoryRepository.Remove(category);
            await _categoryRepository.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
