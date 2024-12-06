using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NotesApi.Data;
using NotesApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace NotesApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly NotesDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(NotesDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: /api/auth/signup
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User user)
        {
            // Check if username already exists
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest(new { message = "User already exists." });

            // Hash the password before saving
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User created successfully." });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] User login)
        {
            // Find user in the database
            var user = _context.Users.SingleOrDefault(u => u.Username == login.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.PasswordHash, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password." });

            // Generate JWT token
            var token = GenerateJwtToken(user.Id);

            return Ok(new { token });
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            // Get the token expiration time from the configuration
            int tokenExpiryMinutes = int.Parse(_configuration["JwtSettings:TokenExpiryMinutes"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("id", userId.ToString())
        }),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
                Audience = _configuration["JwtSettings:Audience"], 
                Issuer = _configuration["JwtSettings:Issuer"],     
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
