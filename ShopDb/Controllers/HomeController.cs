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
            HttpContext.Session.SetString("key", "value");
            return View();
        }

        public IActionResult ShoppingCart()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            //var userId = 2;
            var cartId = _shopDb.ShoppingCarts.Where(c => c.UserId == userId).Select(c => c.Id).FirstOrDefault();
            var cartRows = _shopDb.ShoppingCartRows.Include(c => c.Product).Where(x => x.ShoppingCartId == cartId);
            return View(cartRows.ToList());
        }


        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            var cartId = _shopDb.ShoppingCarts.Where(c => c.UserId == userId).Select(c => c.Id).FirstOrDefault();

            if (cartId == 0)
            {
                var cart = new ShoppingCart() { UserId = userId };
                _shopDb.ShoppingCarts.Add(cart);
                _shopDb.SaveChanges();
                cartId = cart.Id;
            }

            var cartRow = _shopDb.ShoppingCartRows.Where(r => r.ShoppingCartId == cartId && r.ProductId == productId).FirstOrDefault();

            if (cartRow is null)
            {
                cartRow = new ShoppingCartRow() { ProductId = productId, ShoppingCartId = cartId, Quantity = quantity };
                _shopDb.ShoppingCartRows.Add(cartRow);
            }
            else
            {
                cartRow.Quantity += quantity;
            }
            _shopDb.SaveChanges();

            return Content($"Lisätty tuote tiedoilla: userId {userId}, productId {productId}, cartId {cartId}, cartRow {cartRow}");
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

            return Content($"Poistettu tuote ostoskorista");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}