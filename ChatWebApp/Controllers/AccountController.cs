using ChatWebApp.Data;
using ChatWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Login(string nickname, string password)
        {
            var user = _context.User.FirstOrDefault(u => u.Nickname == nickname && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("Nickname", user.Nickname);

                return RedirectToAction("Index", "Home"); 
            }

            ViewBag.Error = "Invalid credentials. Please try again.";
            return View();
        }

        // GET
        public IActionResult Register()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                newUser.CreatedAt = DateTime.Now;
                _context.User.Add(newUser);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(newUser);
        }

        // Logout User
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
