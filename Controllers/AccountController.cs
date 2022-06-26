using Api.Data;
using Api.DTOs;
using Api.Extensions;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private readonly StoreContext _context;
        public AccountController(UserManager<User> userManager, TokenService tokenService, StoreContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // Check if user exists in the database.
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            // Handle user null and invalid password.
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password)) 
                return Unauthorized();

            // Check if user has a basket.
            var userBasket = await RetrieveBasket(loginDto.Username);
            // Check if there is an anonymous basket in the cookies.
            var anonBasket = await RetrieveBasket(Request.Cookies["buyerId"]);

            // If the anonymous basket is not null
            if (anonBasket != null)
            {
                // If the user already had a basket, remove it.
                if (userBasket != null) _context.Baskets.Remove(userBasket);
                // Now associate the anonymous basket to the user name.
                anonBasket.BuyerId = user.UserName;
                // Save changes
                await _context.SaveChangesAsync();
            }

            return new UserDto 
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = anonBasket != null ? anonBasket.MapBasketToDto() : userBasket?.MapBasketToDto()
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            // Store new user properties.
            var user  = new User { UserName = registerDto.Username, Email = registerDto.Email };

            // Create the new user.
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            // Handle error
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            return StatusCode(201);
        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Check if user has a basket.
            var userBasket = await RetrieveBasket(User.Identity.Name);

            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = userBasket?.MapBasketToDto()
            };
        }

        // Method to retrieve a basket.
        private async Task<Basket> RetrieveBasket(string buyerId)
        {
            // Handle null buyerId.
            if (string.IsNullOrEmpty(buyerId))
            {
                Response.Cookies.Delete("buyerId");
                return null;
            }

            // Return cookie associated to the buyerId.
            return await _context.Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == buyerId);
        }
    }
}
