using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Название товара")]
        [Required(ErrorMessage = "Укажите название товара")]
        public string Name { get; set; } = null!;

        [DisplayName("Короткое описание товара")]
        public string? ShortDesc { get; set; }

        [DisplayName("Описание товара")]
        public string? Description { get; set; }

        [DisplayName("Цена товара")]
        [Required(ErrorMessage = "Укажите цену товара")]
        [Range(1, int.MaxValue)]
        public double Price { get; set; }

        [DisplayName("Изображение товара")]
        public string? Image { get; set; }

        [DisplayName("Категория товара")]
        [Required(ErrorMessage = "Выберите категорию")]
        public int CategoryId { get; set; }

        [DisplayName("Использование товара")]
        [Required(ErrorMessage = "Выберите использование")]
        public int ProductUsageId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        //  virtual позволяет использ. ленивую загрузку данных, что означает, что
        //  связанные данные будут загружаться только при необходимости
        // (навигационное свойство)

        [ForeignKey("ProductUsageId")]
        public virtual ProductUsage ProductUsage { get; set; } = null!;
    }
}
