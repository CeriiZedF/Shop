using Microsoft.AspNetCore.Mvc.Rendering;

namespace Shop.Models.ViewModels
{
    public class UserVM
    {
        public ShopUser ShopUser { get; set; } = null!;

        public string? Password { get; set; }

        public string Role { get; set; } = null!;

        // для выбора роли в выпадающем списке в UI
        public SelectList? Roles { get; set; } = null!;
    }
}
