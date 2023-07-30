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
using Microsoft.AspNetCore.Cors;

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
            _logger.LogInformation("GET request received for Index page.");
            return View("AuthenticateUser");
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            _logger.LogInformation("POST request received for Index page.");

            var hashedPassword = PasswordHasher.HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Authentication failed for user: {Username}", username);
                return View("AuthenticateUser");
            }

            _logger.LogInformation("User authenticated successfully: {Username}", username);

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
                _logger.LogInformation("Redirecting to AdminPage for user: {Username}", username);
                return RedirectToAction("AdminPage");
            }
            else
            {
                _logger.LogInformation("Redirecting to UserPage for user: {Username}", username);
                return RedirectToAction("UserPage");
            }
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminPage()
        {
            _logger.LogInformation("Accessing AdminPage.");

            var userList = _context.Users.ToList();
            return View(userList);
        }

        [Authorize]
        public IActionResult UserPage()
        {
            _logger.LogInformation("Accessing UserPage.");

            return View();
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult UpdateUser(User user)
        {
            _logger.LogInformation("Updating user.");

            var existingUser = _context.Users.FirstOrDefault(u => u.UserID == user.UserID);
            if (existingUser == null)
            {
                _logger.LogWarning("User with ID {UserID} was not found.", user.UserID);
                return NotFound("User with ID " + user.UserID + " was not found");
            }

            existingUser.Username = user.Username;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            _context.SaveChanges();

            _logger.LogInformation("User updated successfully: {UserID}", user.UserID);

            return Ok(new { Message = "User updated successfully.", UserId = user.UserID });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteUser(int userId)
        {
            _logger.LogInformation("Deleting user with ID {UserID}.", userId);

            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserID} was not found.", userId);
                return NotFound("User with ID " + userId + " was not found");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            _logger.LogInformation("User deleted successfully: {UserID}", userId);

            return Ok(new { Message = "User deleted successfully.", UserId = userId });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AddUser(string username, string password, string role)
        {
            _logger.LogInformation("Adding new user.");

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                _logger.LogWarning("User with the same username already exists.");
                return BadRequest("User with the same username already exists.");
            }

            // Hash the password before storing it
            string hashedPassword = PasswordHasher.HashPassword(password);

            var newUser = new User { Username = username, Password = hashedPassword, Role = role, IsActive = true };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            _logger.LogInformation("User added successfully.");

            return Ok(new { Message = "User added successfully." });
        }


        [HttpPost]
        [EnableCors("AllowSpecificOrigin")]
        [Authorize]
        public IActionResult SaveMessage(string message)
        {
            _logger.LogInformation("Received a message: {Message}", message);

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Message is empty or null.");
                return BadRequest("Message cannot be empty.");
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser == null)
            {
                _logger.LogWarning("User with username {Username} not found.", User.Identity.Name);
                return NotFound("User with username " + User.Identity.Name + " not found");
            }

            var newMessage = new Message
            {
                Value = message,
                Timestamp = DateTime.UtcNow, 
                FK_userID = currentUser.UserID
            };

            _context.Messages.Add(newMessage);
            _context.SaveChanges();

            _logger.LogInformation("Message saved successfully.");

            return Ok("Message saved successfully.");
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetLastMessages(int count)
        {
            var messages = _context.Messages
                .Include(m => m.User)
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .Select(m => new
                {
                    Username = m.User.Username,
                    Message = m.Value,
                    Timestamp = DateTime.SpecifyKind(m.Timestamp, DateTimeKind.Utc) // Specify the DateTimeKind as Utc
                })
                .ToList();

            return Json(messages);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Logging out user: {Username}", User.Identity.Name);

            // Perform the sign-out operation
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect the user to the login page
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("An error occurred. Request ID: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
