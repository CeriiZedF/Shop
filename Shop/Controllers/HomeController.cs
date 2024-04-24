using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using Shop.Services;
using Shop.Filters;

using Shop.Models;
using Shop.Models.ViewModels;
using Shop.DAL.Repository.IRepository;


namespace Shop.Controllers
{
    [CountRequests]
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly BasketProductSessionService _basketProductSessionService;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, BasketProductSessionService basketProductSessionService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _basketProductSessionService = basketProductSessionService;
        }


        // вывод товаров
        public async Task<IActionResult> Index()
        {
            // по умолчанию показываем все товары
            return await ShowViewProducts(await _productRepository.GetAll(
                includeProperties: "Category,ProductUsage"
            ));
        }

        [HttpGet]
        public async Task<IActionResult> ChoiceCategory(int? id)
        {
            if (id is not null)  // если выбрана категория
            {
                return await ShowViewProducts(await _productRepository.GetAll(
                    p => p.CategoryId == id,
                    includeProperties: "Category,ProductUsage"
                ));
            }
            return RedirectToAction("Index");  // показываем все товары
        }

        // общий метод для отправки view для показа продуктов
        private async Task<IActionResult> ShowViewProducts(IEnumerable<Product> products)
        {
            HomeVM homeVM = new()
            {
                Catergories = await _categoryRepository.GetAll(),
                Products = products
            };
            return View("Index", homeVM);
        }

        // детальная информация о товаре
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) { return NotFound(); }

            Product? p = await _productRepository.FirstOrDefault(
                p => p.Id == id,
                includeProperties: "Category,ProductUsage"
            );
            if (p is null) { return NotFound(); }

            // проверяем есть ли товар в корзине
            DetailsVM detailsVM = new() { Product = p };
            ShoppingCartVM? shoppingCartVM = _basketProductSessionService.GetShoppingCart();
            if (shoppingCartVM is not null)
            {
                detailsVM.ExistsInCart = shoppingCartVM.ProductsId.Contains((int)id);
            }

            return View(detailsVM);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}