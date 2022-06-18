using Api.Data;
using Api.Extensions;
using Api.Models;
using Api.RequestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Api.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly StoreContext _context;
        public ProductsController(StoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery]ProductParams productParams)
        {
            // Query to get products.
            var query = _context.Products
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Brands, productParams.Types)
                .AsQueryable();  
            
            // Store paged list.
            var products = await PagedList<Product>.ToPagedList(query, productParams.PageNumber, productParams.PageSize);

            // Use class extension to create a custom http header.
            Response.AddPaginationHeader(products.MetaData);

            return products;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            return product;
        }

        // Get data from related tables.
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            // Get products by brand.
            var brand = await _context.Products.Select(p => p.Brand).Distinct().ToListAsync();

            // Get products by types.
            var types = await _context.Products.Select(p => p.Type).Distinct().ToListAsync();

            return Ok(new {brand, types});
        }
    }
}
