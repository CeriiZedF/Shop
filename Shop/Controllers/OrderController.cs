using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shop.Services;

using Shop.Models;
using Shop.DAL.Repository.IRepository;

namespace Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private IOrderRepository _orderRepository;
        private CurrentUserProvider _currentUserProvider;

        public OrderController(IOrderRepository orderRepository, CurrentUserProvider currentUserProvider)
        {
            _orderRepository = orderRepository;
            _currentUserProvider = currentUserProvider;
        }


        // возвращается список заказов данного пользователя
        public async Task<IActionResult> Index()
        {
            ShopUser? user = await _currentUserProvider.GetCurrentShopUser();
            if (user is null)
            {
                ViewBag.ErrorMess = "Sorry, try again...";
                return View();
            }

            var orders = await _orderRepository.GetAll(
                o => o.UserId == user.Id,
                includeProperties: "Product,Product.Category"
            );
            return View(orders);
        }

        // весь список заказов (для менеджеров и админов)
        [Authorize(Policy = WC.AdminManagerPolicy)]
        public async Task<IActionResult> GetAllOrders()
        {
            var allOrders = await _orderRepository.GetAll(
                includeProperties: "Product,Product.Category,ShopUser"
            );
            return View(allOrders);
        }


        // удаление одного товара в заказах
        [Authorize(Policy = WC.AdminManagerPolicy)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) { return NotFound(); }

            Order? order = await _orderRepository.FirstOrDefault(o => o.Id == id);
            if (order is null) { return NotFound(); }

            _orderRepository.Remove(order);
            await _orderRepository.Save();

            return RedirectToAction("GetAllOrders");
        }


        // редактирование одного товара в заказах
        [Authorize(Policy = WC.AdminManagerPolicy)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) { return NotFound(); }

            Order? order = await _orderRepository.FirstOrDefault(
                o => o.Id == id,
                includeProperties: "Product,ShopUser"
            );
            if (order is null) { return NotFound(); }

            return View(order);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Edit(Order order)
        {
            return RedirectToAction("GetAllOrders");
        }
    }
}
