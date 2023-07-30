using ChatWebServer.DBContext;
using ChatWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Index(string username, string password)
        {
            var hashedPassword = PasswordHasher.HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user == null)
            {
                return View("AuthenticateUser");
            }

            // Authentication successful. Sign in the user with cookies
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to the appropriate page based on the user's role
            if (user.Role == "ADMIN")
            {
                return RedirectToAction("AdminPage");
            }
            else
            {
                return RedirectToAction("UserPage");
            }
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminPage()
        {
            var userList = _context.Users.ToList();
            return View(userList);
        }

        [Authorize]
        public IActionResult UserPage()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserID == user.UserID);
            if (existingUser == null)
            {
                return NotFound("User with ID " + user.UserID + " was not found"); // User with the specified ID not found
            }

            existingUser.Username = user.Username;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            _context.SaveChanges();

            return Ok(new { Message = "User updated successfully.", UserId = user.UserID });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return NotFound("User with ID " + userId + " was not found"); // User with the specified ID not found
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { Message = "User deleted successfully.", UserId = userId });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AddUser(string username, string password, string role)
        {

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                return BadRequest("User with the same username already exists.");
            }

            // Hash the password before storing it
            string hashedPassword = PasswordHasher.HashPassword(password);

            var newUser = new User { Username = username, Password = hashedPassword, Role = role, IsActive = true };

            

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { Message = "User added successfully.", UserId = user.UserID });
        }



        [HttpPost]
        public IActionResult SaveMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return BadRequest("Message cannot be empty.");

            // Get the ID of the current authenticated user
            var userIDClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIDClaim == null || !int.TryParse(userIDClaim.Value, out int userID))
                return BadRequest("User ID not found or invalid.");

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
