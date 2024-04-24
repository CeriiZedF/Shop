namespace Shop.Models.ViewModels
{
    public class DetailsVM
    {
        public DetailsVM()
        {
            Product = new();
            ExistsInCart = false;
        }

        public Product Product { get; set; }
        public bool ExistsInCart { get; set; }
    }
}
