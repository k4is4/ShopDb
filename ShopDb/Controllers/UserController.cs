using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return _context.Users != null ?
                        View(await _context.Users.ToListAsync()) :
                        Problem("Entity set 'ShopDbContext.Users'  is null.");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
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
            _context.SaveChanges();
            HttpContext.Session.SetInt32("userId", user.Id);

            Address address1 = new Address()
            {
                UserId = user.Id,
                Address1 = address,
                PostalCode = postalCode,
                City = city
            };
            _context.Addresses.Add(address1);
            _context.SaveChanges();

            return View();
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Phone,Email")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ShopDbContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

                    if (obj.Id == 2)
                    { return Redirect("/Admin/Product"); }

                    if (obj != null)
                    {
                        HttpContext.Session.SetInt32("userId", obj.Id);
                        HttpContext.Session.SetString("userName", obj.FirstName + " " + obj.LastName);
                        return RedirectToAction("ProfilePage");
                    }
                }
            }
            ViewBag.LoginFailedMessage = "Invalid user Id, please rety or create a new user";
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
    }
}
