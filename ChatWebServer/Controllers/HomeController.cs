using ChatWebServer.DBContext;
using ChatWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography;

namespace ChatWebServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("AuthenticateUser");
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            if (!UserIsAuthenticated(new User { Password = PasswordHasher.HashPassword(password), Username = username })) return View("AuthenticateUser");
            return View("Index", username);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool UserIsAuthenticated(User userToAuthenticate)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == userToAuthenticate.Username && PasswordHasher.VerifyPassword(u.Password, userToAuthenticate.Password));

            return user != null;
        }
    }
}
