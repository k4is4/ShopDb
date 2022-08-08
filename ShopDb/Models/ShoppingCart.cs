using System;
using System.Collections.Generic;

namespace ShopDb.Models
{
    public partial class ShoppingCart
    {
        public ShoppingCart()
        {
            ShoppingCartRows = new HashSet<ShoppingCartRow>();
        }

        public int CartId { get; set; }
        public int? UserId { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<ShoppingCartRow> ShoppingCartRows { get; set; }
    }
}
