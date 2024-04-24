using Shop.Helpers;
using Shop.Models;

namespace Shop.ViewModels
{
    public class ProductVM
    {
        public IEnumerable<Product> Products { get; set; } = null!;
        public Pager Pager { get; set; } = null!;
    }
}
