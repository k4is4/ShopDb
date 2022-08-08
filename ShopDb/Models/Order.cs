using System;
using System.Collections.Generic;

namespace ShopDb.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderRows = new HashSet<OrderRow>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderRow> OrderRows { get; set; }
    }
}
