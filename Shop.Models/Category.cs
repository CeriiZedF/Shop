using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Shop.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Название категории")]
        [Required(ErrorMessage = "Укажите название категории")]
        public string Name { get; set; } = null!;

        [DisplayName("Порядок отображения")]
        [Required(ErrorMessage = "Укажите порядок")]
        [Range(1, int.MaxValue, ErrorMessage = "Значение {0} должно быть между {1} и {2}")]
        public int DisplayOrder { get; set; }
    }
}
