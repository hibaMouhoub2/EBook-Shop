using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartProject.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpcontextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpcontextAccessor = httpcontextAccessor;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if(userId == null) { throw new Exception("Invalid userid"); }
            var shoppingCart = await _db.ShoppingCarts
                                    .Include(x => x.CartDetails)
                                    .ThenInclude(x=>x.Book)
                                    .ThenInclude(x=>x.Genre)
                                    .Where(x=>x.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            
            try
            {
                
                if (string.IsNullOrEmpty(userId)) { throw new Exception("user is not logged in"); }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                //Add cart details
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
                
            }catch (Exception ex)
            {
                
            }

            var cartItemCount =await  GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<int> Removetem(int bookId)
        {
            string userId = GetUserId();
            //using var transaction = _db.Database.BeginTransaction();
            try
            {
                
                if (string.IsNullOrEmpty(userId)) { throw new Exception("user is not logged in"); }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new Exception("Invalid cart");
                }
               
                
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null) { throw new Exception("No items in cart"); }
                else if(cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem); 
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1; 
                }
                _db.SaveChanges();
                //transaction.Commit();
               
            }
            catch (Exception ex)
            {
                
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
        public async Task<int> GetCartItemCount(string userId="")
        {
            if(!string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              select new { cartDetail.Id }
                              ).ToListAsync();
            return data.Count;
        }

        public string GetUserId()
        {
            var principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();    
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged in");
                var cart= await GetCart(userId);
                if (cart is null)
                    throw new Exception("invalid cart");
                var cartDetail = _db.CartDetails
                                    .Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new Exception("Cart is empty");
                var order = new Order
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now,
                    OrderStatusId = 1 //pending by default
                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach(var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                    };
                    _db.OrdersDetails.Add(orderDetail);

                }
                _db.SaveChanges();

                //remove cart details
                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch(Exception)
            {
                return false;
            }


        }

        Task<int> ICartRepository.GetCartItemCount(string userId)
        {
            throw new NotImplementedException();
        }

        Task<ShoppingCart> ICartRepository.GetCart(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
