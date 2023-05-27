using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4.Api.Models;

namespace Task4.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser {
                    Email = model.Email,
                    UserName = model.Email,
                    FullName = model.FullName,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Optionally, you can sign in the user after registration
                    // await _signInManager.SignInAsync(user, isPersistent: false);

                    // Return a successful registration response
                    return Ok("Registration successful");
                }
                else
                {
                    // If registration fails, add the errors to the model state
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            // Return the model state with validation errors
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (user.IsActive)
                    {
                        // Update last login date
                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        // Authentication successful

                        // Generate and return authentication token or perform other actions

                        return Ok();
                    }
                    else
                    {
                        // User is not active
                        return BadRequest("Your account is inactive. Please contact the administrator.");
                    }
                }
                else
                {
                    // Authentication failed
                    return BadRequest("Invalid email or password.");
                }
            }

            // Invalid model state
            return BadRequest(ModelState);
        }


        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                IsActive = user.IsActive
            }).ToList();

            return Ok(userViewModels);
        }

        [HttpPut("block/{userId}")]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(); // User not found
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            return Ok(); // User blocked successfully
        }

        [HttpPut("bulk-block")]
        public async Task<IActionResult> BulkBlockUsers(List<string> userIds)
        {
            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            foreach (var user in users)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
            }

            return Ok(); // Users blocked successfully
        }

        [HttpPut("bulk-activate")]
        public async Task<IActionResult> BulkActivateUsers(List<string> userIds)
        {
            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            foreach (var user in users)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
            }

            return Ok();
        }

        [HttpPut("activate/{userId}")]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Ok();
        }






    }
}
