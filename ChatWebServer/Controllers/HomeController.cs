using ChatWebServer.DBContext;
using ChatWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

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
            var hashedPassword = PasswordHasher.HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user == null)
            {
                return View("AuthenticateUser");
            }
            else if (user.Role == "ADMIN")
            {
                return RedirectToAction("AdminPage", new { username = user.Username });
            }
            else
            {
                return RedirectToAction("UserPage", new { username = user.Username });
            }
        }

        public IActionResult AdminPage(string username)
        {
            var userList = _context.Users.ToList();
            return View(userList);
        }

        public IActionResult UserPage(string username)
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveMessage(string message, int userID)
        {
            if (string.IsNullOrEmpty(message)) return BadRequest("Message cannot be empty.");

            var newMessage = new Message { Value = message, Timestamp = DateTimeOffset.Now, FK_userID = userID };

            _context.Messages.Add(newMessage);
            _context.SaveChanges();

            return Ok("Message saved successfully.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
