using BookShoppingCartProject.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
namespace BookShoppingCartProject.Repository
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public UserOrderRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpcontextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpcontextAccessor = httpcontextAccessor;
        }

        

        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();
            if(string.IsNullOrEmpty(userId))
            {
                throw new Exception("User is not logged in");
                
            }
            var orders = await _db.Orders
                            .Include(x=> x.OrderStatus)
                            .Include(x=>x.OrderDetail)
                            .ThenInclude(x=>x.Book)
                            .ThenInclude(x=>x.Genre)
                            .Where(o => o.UserId == userId)
                            .ToListAsync();
            return orders;    
        }
        public string GetUserId()
        {
            var principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }


    }
}
