using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDb.Models;

namespace ShopDb.Controllers
{
    public class UserController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IConfiguration _conf;

        public UserController(ShopDbContext context, IConfiguration conf)
        {
            _conf = conf;
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            optionsBuilder.UseSqlServer(_conf["ConnectionStrings:ShopDb"]);
            _context = new ShopDbContext(optionsBuilder.Options);
        }

        [HttpGet]
        public IActionResult NewUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewUser(string firstName, string lastName, string phone, string email, string address, string postalCode, string city)
        {
            User user = new User()
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Email = email
            };
            _context.Users.Add(user);

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetString("userName", user.FirstName + " " + user.LastName);

            Address address1 = new Address()
            {
                UserId = user.Id,
                Address1 = address,
                PostalCode = postalCode,
                City = city
            };
            _context.Addresses.Add(address1);
            user.Addresses.Add(address1);

            if (!Models.Utils.UserValidator(user, HttpContext))
            {
                return View();
            }
            
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            if (UserExists(user.Id))
            {
                {
                    var obj = _context.Users.Where(a => a.Id.Equals(user.Id)).FirstOrDefault();

                    if (obj.Id == int.Parse(_conf["AdminId"]))
                    { return Redirect("/Admin/Product"); }

                    if (obj != null)
                    {
                        HttpContext.Session.SetInt32("userId", obj.Id);
                        HttpContext.Session.SetString("userName", obj.FirstName + " " + obj.LastName);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ViewBag.LoginFailedMessage = "Käyttäjä ID:tä ei löydy. Ole hyvä ja yritä uudelleen tai luo uusi käyttäjätili";
            return View(user);
        }

        public IActionResult ProfilePage()
        {
            if (HttpContext.Session.GetInt32("userId") != null)
            {
                var user = _context.Users.Include(a => a.Addresses).Where(i => i.Id == HttpContext.Session.GetInt32("userId")).FirstOrDefault();
                return View(user);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public IActionResult UpdateProfile(string firstName, string lastName, string phone, string email, string address, string postalCode, string city)
        {
            User user = _context.Users.Include(a => a.Addresses).Where(i => i.Id.Equals(HttpContext.Session.GetInt32("userId"))).FirstOrDefault();
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Phone = phone;
            user.Email = email;
            user.Addresses.FirstOrDefault().Address1 = address;
            user.Addresses.FirstOrDefault().PostalCode = postalCode;
            user.Addresses.FirstOrDefault().City = city;

            if (!Models.Utils.UserValidator(user, HttpContext))
            {
                return View("ProfilePage", user);
            }
            _context.Update(user);
            _context.SaveChanges();

            return View("ProfilePage", user);
        }

        public IActionResult OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            var orders = _context.Orders.Where(o => o.UserId == userId).ToList();

            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            var orderRows = _context.OrderRows.Include(o => o.Order).Include(p => p.Product).Where(r => r.OrderId == id).ToList();

            return View(orderRows);
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
