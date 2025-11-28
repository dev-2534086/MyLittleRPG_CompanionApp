using Microsoft.AspNetCore.Mvc;
using API_Pokemon.Data.Context;
using API_Pokemon.Models;
using static API_Pokemon.Models.DTO;

namespace API_Pokemon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MonsterContext _context;

        public UsersController(MonsterContext context)
        {
            _context = context;
        }

        // POST: api/Users/register
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest(new { message = "Email is required." });

            if (_context.isExistingEmail(request.Email)) return Conflict(new { message = "This email is already registered." });

            User user = new User(request.Username, request.Email, request.Password, false);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.UserId });
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest(new { message = "Email is required." });

            if (!_context.isExistingEmail(request.Email)) return NotFound(new { message = "No user found with this email." });

            User currentUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);

            if (currentUser.Password != request.Password) return Unauthorized(new { message = "Invalid email or password." });

            currentUser.IsConnected = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Login successful.", user = currentUser });
        }

        // POST: api/Users/logout
        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest(new { message = "Email is required." });

            if (!_context.isExistingEmail(request.Email)) return NotFound(new { message = "No user found with this email." });

            User currentUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);
            currentUser.IsConnected = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logout successful." });
        }

    }
}
