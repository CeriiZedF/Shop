using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

using Shop.Services;

using Shop.Models;
using Shop.Models.ViewModels;
using Shop.DAL.Repository.IRepository;

namespace Shop.Controllers
{
    public class BasketProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly BasketProductSessionService _basketProductSessionService;
        private readonly CurrentUserProvider _currentUserProvider;

        public BasketProductController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            BasketProductSessionService basketProductSessionService,
            CurrentUserProvider currentUserProvider)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _basketProductSessionService = basketProductSessionService;
            _currentUserProvider = currentUserProvider;
        }

        // только для авторизованных
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // получение товаров из сессии
            ShoppingCartVM? shoppingCart = _basketProductSessionService.GetShoppingCart();
            if (shoppingCart is null) { return View(); }

            var products = await _productRepository.GetAll(
                p => shoppingCart.ProductsId.Contains(p.Id),
                includeProperties: "Category,ProductUsage"
            );
            return View("Index", products);
        }

        // получение суммы в корзине
        public int GetTotalSum()
        {
            return _basketProductSessionService.GetTotalSum();
        }

        // добавление товара в корзину
        [HttpPost]
        public IActionResult AddInBasket([FromBody] DetailsVM? detailsVM)
        {
            if (detailsVM is null)
            {
                return Json(new { success = false, message = "Something went wrong, try again later" });
            }

            // обновляем данные о товарах в сессии
            _basketProductSessionService.SetShoppingCart(detailsVM.Product);

            return Json(new { success = true, message = "Successfully added to cart" });
        }

        // удаление товара из корзины
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            // проверка что такой продукт есть
            if (id is null) { return View(); }

            Product? p = await _productRepository.FirstOrDefault(p => p.Id == id);
            if (p is null) { return View(); }

            // обновляем данные о товарах в сессии
            _basketProductSessionService.SetShoppingCart(p, true);  // true - удаляем

            return RedirectToAction("Index");
        }

        // отправка заказа
        [HttpPost]
        public async Task<IActionResult> SendOrder(string productsIdJson)
        {
            var productsId = JsonSerializer.Deserialize<List<int>>(productsIdJson);
            if (productsId is null) { return RedirectToAction("Index"); }

            // получаем пользователя
            ShopUser? user = await _currentUserProvider.GetCurrentShopUser();
            if (user is null) { return RedirectToAction("Index"); }

            DateTime dtNow = DateTime.Now;
            foreach (int id in productsId)
            {
                Order order = new()
                {
                    UserId = user.Id,
                    ProductId = id,
                    CreatedDate = dtNow
                };
                await _orderRepository.Add(order);
            }
            await _orderRepository.Save();
            _basketProductSessionService.Clear();  // очищаем корзину
            return View("ThankSendOrder");  // уведомление об успешной покупке
        }

        public IActionResult ThankSendOrder()
        {
            return View();
        }
    }
}
