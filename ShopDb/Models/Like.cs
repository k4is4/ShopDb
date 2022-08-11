using System;
using System.Collections.Generic;

namespace ShopDb.Models
{
    public partial class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}
