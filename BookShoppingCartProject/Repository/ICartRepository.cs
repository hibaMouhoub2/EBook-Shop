using System.Threading.Tasks;

namespace BookShoppingCartProject.Repository
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty);
        Task<int> Removetem(int bookId);
        Task<ShoppingCart> GetUserCart();
        Task<int> GetCartItemCount(string userId = "");
        Task<ShoppingCart> GetCart(string userId);
        Task<bool> DoCheckout();
    }
}