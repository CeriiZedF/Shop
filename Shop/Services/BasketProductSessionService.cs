using System.Text.Json;
using Shop.Models;
using Shop.Models.ViewModels;

namespace Shop.Services
{
    // Работа с сессиями для корзины товаров (общая сумма и коллекция id продуктов)
    public class BasketProductSessionService
    {
        private static string ShoppingCartKey = "ShopCart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BasketProductSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTotalSum()
        {
            ShoppingCartVM? shopCart = GetShoppingCart();
            return shopCart is null ? 0 : shopCart.TotalSum;
        }

        public void Clear()
        {
            // очищаем корзину
            _httpContextAccessor?.HttpContext?.Session.Set(
                ShoppingCartKey,
                JsonSerializer.SerializeToUtf8Bytes(new ShoppingCartVM())
            );
        }

        public ShoppingCartVM? GetShoppingCart()
        {
            string? json = _httpContextAccessor?.HttpContext?.Session.GetString(ShoppingCartKey);
            if (json is null) { return null; }
            return JsonSerializer.Deserialize<ShoppingCartVM>(json);
        }

        public void SetShoppingCart(Product product, bool isDelete = false)
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext is null) { return; }

            if (!httpContext.Session.TryGetValue(ShoppingCartKey, out byte[]? cartData))
            {
                cartData = JsonSerializer.SerializeToUtf8Bytes(new ShoppingCartVM());
            }

            ShoppingCartVM shoppingCart = JsonSerializer.Deserialize<ShoppingCartVM>(cartData) ?? new();
            if (isDelete)
            {
                shoppingCart.ProductsId.RemoveAll(pId => pId == product.Id);
                shoppingCart.TotalSum -= (int)product.Price;
            }
            else
            {
                shoppingCart.ProductsId.Add(product.Id);
                shoppingCart.TotalSum += (int)product.Price;
            }
            httpContext.Session.Set(ShoppingCartKey, JsonSerializer.SerializeToUtf8Bytes(shoppingCart));
        }
    }
}
