using System.ComponentModel.DataAnnotations;

namespace Shop.Models
{
    public class ProductUsage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите название")]
        public string Name { get; set; } = null!;
    }
}
