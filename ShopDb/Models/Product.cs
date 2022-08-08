using System;
using System.Collections.Generic;

namespace ShopDb.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderRows = new HashSet<OrderRow>();
            ShoppingCartRows = new HashSet<ShoppingCartRow>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? PictureUrl { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<OrderRow> OrderRows { get; set; }
        public virtual ICollection<ShoppingCartRow> ShoppingCartRows { get; set; }
    }
}
