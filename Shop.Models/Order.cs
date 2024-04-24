using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int ProductId { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("UserId")]
        public virtual ShopUser ShopUser { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
