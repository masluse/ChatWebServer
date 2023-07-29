using ChatWebServer.DBContext;
using ChatWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChatWebServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private static User currentUser; 

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
            var loginUser = new User { Password = PasswordHasher.HashPassword(password), Username = username, IsActive = true, UserID = 0 };
            if (!UserIsAuthenticated(loginUser)) return View("AuthenticateUser");
            else return View("Index", username);
        }


        [HttpPost]
        public IActionResult SaveMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return BadRequest("Message cannot be empty.");

            var newMessage = new Message { Value = message, Timestamp = DateTimeOffset.Now, FK_userID = currentUser.UserID };

            _context.Messages.Add(newMessage);
            _context.SaveChanges();

            return Ok("Message saved succesfully.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool UserIsAuthenticated(User userToAuthenticate)
        {
            currentUser = _context.Users.FirstOrDefault(u => u.Username == userToAuthenticate.Username);

            if (currentUser == null) return false;
            else return currentUser.Password == userToAuthenticate.Password;
        }

    }
}
