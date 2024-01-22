namespace OnlineShopApp.Models
{
    public class Cart
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
