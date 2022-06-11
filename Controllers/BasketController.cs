using Api.Data;
using Api.DTOs;
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

        [HttpGet]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            // Get basket from private method.
            var basket = await RetrieveBasket();

            // Handle not found.
            if (basket == null) return NotFound();

            return MapBasketToDto(basket);
        }


        // POST => api/basket?productId=3&quantity=2
        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
        {
            // Steps TO DO..
            // Get basket, create the basket, get product, add item, save changes.

            // Step 1. Get the basket
            var basket = await RetrieveBasket();

            // Step 2. Check if basket is null, create one if it is.
            if (basket == null) basket = CreateBasket();

            // Step 3. Get the product.
            var product = await _context.Products.FindAsync(productId);

            // Handle not found (404)
            if (product == null) return NotFound();

            // Step 4. Add item to basket
            basket.AddItem(product, quantity);

            // Step 5. Save changes
            var result = await _context.SaveChangesAsync() > 0;

            // Return result
            if (result) return CreatedAtRoute("GetBasket", MapBasketToDto(basket));

            // Handle null result.
            return BadRequest(new ProblemDetails { Title = "Problem saving the item to basket." });
        }


        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            // Steps TO DO..
            // Get basket, remove item or reduce quantity, save changes.

            // Step 1. Get basket from private method.
            var basket = await RetrieveBasket();

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
        private async Task<Basket> RetrieveBasket()
        {
            return await _context.Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["buyerId"]);
        }

        // Method to create a new basket.
        private Basket CreateBasket()
        {
            // Create a basket id by generating a new Guid.
            var buyerId = Guid.NewGuid().ToString();
            
            // Create a cookie, set its options and append it.
            var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30)};
            Response.Cookies.Append("buyerId", buyerId, cookieOptions);

            // Create a new basket.
            var basket = new Basket { BuyerId = buyerId };

            // Add the entity.
            _context.Baskets.Add(basket);

            return basket;
        }

        // Method to map basket data transfer object.
        private BasketDto MapBasketToDto(Basket basket)
        {
            // Return basket items.
            return new BasketDto
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = basket.Items.Select(item => new BasketItemDto
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    PictureUrl = item.Product.PictureUrl,
                    Type = item.Product.Type,
                    Brand = item.Product.Brand,
                    Quantity = item.Quantity
                }).ToList()
            };
        }

    }
}
