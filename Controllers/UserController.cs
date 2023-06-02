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
                    return Ok("Registration successful");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
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
                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        return Ok(user.Id);
                    }
                    else
                    {
                        return BadRequest("Your account is inactive. Please contact the administrator.");
                    }
                }
                else
                {
                    return BadRequest("Invalid email or password.");
                }
            }
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

        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> BulkDeleteUsers(List<string> userIds)
        {
            try
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

                foreach (var user in users)
                {
                    await _userManager.DeleteAsync(user);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error deleting users: " + ex.Message);
            }
        }







    }
}
