﻿using ChatWebServer.DBContext;
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
        public IActionResult AddOrUpdateUser(User user)
        {
            if (user.UserID == 0)
            {
                // Add new user
                _context.Users.Add(user);
            }
            else
            {
                // Update existing user
                var existingUser = _context.Users.FirstOrDefault(u => u.UserID == user.UserID);
                if (existingUser == null)
                {
                    return NotFound(); 
                }

                existingUser.Username = user.Username;
                existingUser.Password = user.Password;
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;
            }

            _context.SaveChanges();

            return Ok(new { Message = "User added/updated successfully.", UserId = user.UserID });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return NotFound(); 
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { Message = "User deleted successfully." });
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return NotFound(); 
            }

            return PartialView("_UserFormPartial", user);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult UserForm()
        {
            return PartialView("_UserFormPartial", new User());
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
