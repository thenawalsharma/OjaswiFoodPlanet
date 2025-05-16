using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OFP.API.Models;

namespace OFP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        IConfiguration _configuration;

        public ProductsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                string configValue = _configuration["config1"]; // Example of using IConfiguration
                var products =await _context.Product.ToListAsync();// ✅ EF Core async method
                return Ok(products);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                    return BadRequest("Invalid product");

                product.CreatedDate = DateTime.UtcNow;

                await _context.Product.AddAsync(product); // ✅ EF Core async method
                await _context.SaveChangesAsync();         // ✅ Save changes

                return Ok(product);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
