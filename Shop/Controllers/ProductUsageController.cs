using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shop.Models;
using Shop.DAL.Repository.IRepository;

namespace Shop.Controllers
{
    [Authorize(Policy = WC.AdminManagerPolicy)]
    public class ProductUsageController : Controller
    {
        private readonly IProductUsageRepository _productUsageRepository;

        public ProductUsageController(IProductUsageRepository productUsageRepository)
        {
            _productUsageRepository = productUsageRepository;
        }

        // READ
        public async Task<IActionResult> Index()
        {
            return View(await _productUsageRepository.GetAll());
        }


        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductUsage pu)
        {
            if (!ModelState.IsValid || pu is null) { return View(); }
            await _productUsageRepository.Add(pu);
            await _productUsageRepository.Save();
            return RedirectToAction(nameof(Index));
        }


        // UPDATE
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) { return View(); }
            ProductUsage? pu = await _productUsageRepository.FirstOrDefault(pu => pu.Id == id);
            if (pu is null) { return View(); }
            return View(pu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductUsage pu)
        {
            if (!ModelState.IsValid || pu is null) { return View(); }
            _productUsageRepository.Update(pu);
            await _productUsageRepository.Save();
            return RedirectToAction(nameof(Index));
        }


        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) { return View(); }
            ProductUsage? pu = await _productUsageRepository.FirstOrDefault(pu => pu.Id == id);
            if (pu is null) { return View(); }
            return View(pu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProductUsage pu)
        {
            if (pu is null) { return View(); }
            _productUsageRepository.Remove(pu);
            await _productUsageRepository.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
