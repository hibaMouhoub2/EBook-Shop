using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartProject.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrerRepo;

        public UserOrderController(IUserOrderRepository userOrerRepo)
        {
            _userOrerRepo = userOrerRepo;
        }
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrerRepo.UserOrders();
            return View(orders);
        }
    }
}
