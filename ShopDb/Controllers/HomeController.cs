using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDb.Models;
using System.Diagnostics;

namespace ShopDb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _conf;
        private readonly ShopDbContext _shopDb;

        public HomeController(ILogger<HomeController> logger, IConfiguration conf)
        {
            _logger = logger;
            _conf = conf;

            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            optionsBuilder.UseSqlServer(_conf["ConnectionStrings:ShopDb"]);
            _shopDb = new ShopDbContext(optionsBuilder.Options);
        }

        public IActionResult Index()
        {
            var topProducts = GetTopProducts(4);

            return View(topProducts);
        }

        public IActionResult ShoppingCart()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            var cartId = _shopDb.ShoppingCarts.Where(c => c.UserId == userId).Select(c => c.Id).FirstOrDefault();
            var cartRows = _shopDb.ShoppingCartRows.Include(c => c.Product).Where(x => x.ShoppingCartId == cartId);
            return View(cartRows.ToList());
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            var cartId = _shopDb.ShoppingCarts.Where(c => c.UserId == userId).Select(c => c.Id).FirstOrDefault();

            // Check if user already has an active ShoppingCart, else create new ShoppingCart
            if (cartId == 0)
            {
                var cart = new ShoppingCart() { UserId = userId };
                _shopDb.ShoppingCarts.Add(cart);
                _shopDb.SaveChanges();
                cartId = cart.Id;
            }

            // Check if ShoppingCart already includes the product, else add new ShoppingCartRow for the product
            var cartRow = _shopDb.ShoppingCartRows.Where(r => r.ShoppingCartId == cartId && r.ProductId == id).FirstOrDefault();

            if (cartRow is null)
            {
                cartRow = new ShoppingCartRow() { ProductId = id, ShoppingCartId = cartId, Quantity = quantity };
                _shopDb.ShoppingCartRows.Add(cartRow);
            }
            else
            {
                cartRow.Quantity += quantity;
            }
            _shopDb.SaveChanges();

            return RedirectToAction("ShoppingCart");
        }

        public IActionResult DeleteFromCart(int id, int quantity = 1)
        {
            var cartRow = _shopDb.ShoppingCartRows.Where(r => r.Id == id).FirstOrDefault();

            if (cartRow.Quantity > quantity)
            {
                cartRow.Quantity -= quantity;
            }
            else
            {
                _shopDb.ShoppingCartRows.Remove(cartRow);
            }
            _shopDb.SaveChanges();

            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ConfirmOrder(int id, decimal total)
        {
            var cart = _shopDb.ShoppingCarts.Where(c => c.Id == id).FirstOrDefault();

            var order = new Order()
            {
                UserId = (int)HttpContext.Session.GetInt32("userId"),
                Date = DateTime.Today,
                TotalPrice = total
            };

            _shopDb.Orders.Add(order);
            _shopDb.SaveChanges();

            var cartRows = _shopDb.ShoppingCartRows.Include(c => c.Product).Where(x => x.ShoppingCartId == id).ToList();

            // Create new OrderRow for each ShoppingCartRow and delete the whole cart after
            foreach (var item in cartRows)
            {
                item.Product.Quantity -= item.Quantity;

                var orderRow = new OrderRow()
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };
                _shopDb.OrderRows.Add(orderRow);
                _shopDb.ShoppingCartRows.Remove(item);
            }

            _shopDb.SaveChanges();
            _shopDb.ShoppingCarts.Remove(cart);
            _shopDb.SaveChanges();

            return View(cartRows);
        }
        public List<Product> GetTopProducts(int quantity)
        {
            var topProductsDict = new Dictionary<int, int>();
            var topProductsList = new List<Product>();

            // Count how many items of each product sold
            foreach (var item in _shopDb.OrderRows)
            {
                if (!topProductsDict.ContainsKey(item.ProductId))
                {
                    topProductsDict.Add(item.ProductId, item.Quantity);
                }
                else
                {
                    topProductsDict[item.ProductId] += item.Quantity;
                }
            }

            foreach (var item in topProductsDict.OrderByDescending(p => p.Value))
            {
                var product = _shopDb.Product.Where(p => p.Id == item.Key).First();
                topProductsList.Add(product);
            }

            return topProductsList.Take(quantity).ToList();
        }
    }
}