using Microsoft.AspNetCore.Identity;

namespace OnlineShopApp.Models
{
    public class Users : IdentityUser
    {
        public ICollection<Cart> Carts { get; set; }
    }
}
