using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShoppingCartProject.Models
{
    [Table("Order")]
    public class Order
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Required]
        public int  OrderStatusId{ get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool IsDeleted { get; set; } = false;

        public List<OrderDetail> OrderDetail { get; set; }
    }
}
