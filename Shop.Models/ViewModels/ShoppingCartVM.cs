namespace Shop.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public List<int> ProductsId { get; set; } = null!;
        public int TotalSum { get; set; }

        public ShoppingCartVM()
        {
            ProductsId = new();
            TotalSum = 0;
        }
    }
}
