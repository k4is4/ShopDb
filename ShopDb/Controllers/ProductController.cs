using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDb.Models;

namespace ShopDb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IConfiguration _conf;
        private readonly ShopDbContext _shopDb;

        public ProductController(ShopDbContext context, IConfiguration conf)
        {
            _context = context;
            _conf = conf;

            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            optionsBuilder.UseSqlServer(_conf["ConnectionStrings:ShopDb"]);
            _shopDb = new ShopDbContext(optionsBuilder.Options);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Header = "Kaikki tuotteet";
            var allProducts = _context.Product.Include(p => p.Category);
            return View(await allProducts.ToListAsync());
        }

        public async Task<IActionResult> GetProductsByCategory(int id)
        {
            var products = _context.Product.Include(p => p.Category).Where(i => i.Category.Id == id);
            ViewBag.Header = products.First().Category.Name;
            return View("Index", await products.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> GetProductsByName(string name)
        {

            if (string.IsNullOrEmpty(name))
            {
                return RedirectToAction("Index", "Home");
            }
            var search = name.ToLower().Trim();
            var shopDbContext = _context.Product.Where(i => i.Name.ToLower().Contains(search)).Include(p => p.Category);
            if (shopDbContext.ToList().Count() == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Header = $"Haun \"{name}\" tulokset, {shopDbContext.ToList().Count().ToString()} kpl";
            return View("Index", await shopDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("userId");
            var like = _context.Likes
                .Where(a => a.UserId.Equals(userId) && a.ProductId.Equals(id))
                .FirstOrDefault();

            product.Like = like;
            return View(product);
        }

        private bool ProductExists(int id)
        {
          return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
