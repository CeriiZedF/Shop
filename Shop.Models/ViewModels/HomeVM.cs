namespace Shop.Models.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Category> Catergories { get; set; } = null!;
        public IEnumerable<Product> Products { get; set; } = null!;
    }
}
