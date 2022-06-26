using Api.Data;
using Api.DTOs;
using Api.Extensions;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    public class BasketController : BaseApiController
    {
        private readonly StoreContext _context;
        public BasketController(StoreContext context)
        {
            _context = context;
        }

        [HttpGet(Name = "GetBasket")]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            // Get basket from private method.
            var basket = await RetrieveBasket(GetBuyerId());

            // Handle not found.
            if (basket == null) return NotFound();

            return basket.MapBasketToDto();
        }


        // POST => api/basket?productId=3&quantity=2
        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
        {
            // Steps TO DO..
            // Get basket, create the basket, get product, add item, save changes.

            // Step 1. Get the basket
            var basket = await RetrieveBasket(GetBuyerId());

            // Step 2. Check if basket is null, create one if it is.
            if (basket == null) basket = CreateBasket();

            // Step 3. Get the product.
            var product = await _context.Products.FindAsync(productId);

            // Handle not found (404)
            if (product == null) return BadRequest(new ProblemDetails { Title = "Product not found"});

            // Step 4. Add item to basket
            basket.AddItem(product, quantity);

            // Step 5. Save changes
            var result = await _context.SaveChangesAsync() > 0;

            // Return result
            if (result) return CreatedAtRoute("GetBasket", basket.MapBasketToDto());

            // Handle null result.
            return BadRequest(new ProblemDetails { Title = "Problem saving the item to basket." });
        }


        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            // Steps TO DO..
            // Get basket, remove item or reduce quantity, save changes.

            // Step 1. Get basket from private method.
            var basket = await RetrieveBasket(GetBuyerId());

            // Handle not found.
            if (basket == null) return NotFound();

            // Step 2. remove item or reduce quantity.
            basket.RemoveItem(productId, quantity);

            // Step 3. Save changes.
            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            // Handle bad request.
            return BadRequest(new ProblemDetails { Title = "Problem removing item from the basket." });
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

        // Method to check if we have a buyerId.
        // If there is no username, check if there is a cookie for buyerId.
        private string GetBuyerId()
        {
            return User.Identity?.Name ?? Request.Cookies["buyerId"];
        }

        // Method to create a new basket.
        private Basket CreateBasket()
        {
            // Store the buyerId.
            var buyerId = User.Identity?.Name;

            // Handle null buyerId.
            if (string.IsNullOrEmpty(buyerId))
            {
                // Generate a new Guid as buyerId.
                buyerId = Guid.NewGuid().ToString();

                // Create a cookie, set its options and append it.
                var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
                Response.Cookies.Append("buyerId", buyerId, cookieOptions);
            }

            // Create a new basket.
            var basket = new Basket { BuyerId = buyerId };

            // Add the entity.
            _context.Baskets.Add(basket);

            return basket;
        }
    }
}
