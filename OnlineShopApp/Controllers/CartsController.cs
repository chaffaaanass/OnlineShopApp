using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShopApp.Models;

namespace OnlineShopApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly DataContext _context;

        public CartsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        {
            return await _context.Carts.ToListAsync();
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(int id, Cart cart)
        {
            if (id != cart.CartId)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Carts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cart>> PostCart(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCart", new { id = cart.CartId }, cart);
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }

        [HttpPost("AddToCart/{productId}"), Authorize]
        public async Task<IActionResult> AddToCart(int productId)
        {
            try
            {
                // Get the authenticated user's ID
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(); // Return 401 Unauthorized if the user is not authenticated
                }

                // Find the user and the product
                var user = await _context.Users
                    .Include(u => u.Carts)
                    .ThenInclude(c => c.Products)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                var product = await _context.Products.FindAsync(productId);

                if (user == null || product == null)
                {
                    return NotFound();
                }

                // Check if the user already has a cart
                var cart = user.Carts.FirstOrDefault();

                if (cart == null)
                {
                    // If the user doesn't have a cart, create a new one
                    cart = new Cart
                    {
                        UserId = userId,
                        User = user,
                        Products = new List<Product> { product } // Add the product to the new cart
                    };
                    _context.Carts.Add(cart);
                }
                else
                {
                    // Check if the product is already in the cart
                    if (cart.Products.Any(p => p.ProductId == productId))
                    {
                        return BadRequest("Product is already in the cart");
                    }

                    // Add the product to the existing cart
                    cart.Products.Add(product);
                }

                await _context.SaveChangesAsync();

                return Ok("Product added to the cart successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log, return error response, etc.)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding product to the cart");
            }
        }




    }
}
