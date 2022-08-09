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