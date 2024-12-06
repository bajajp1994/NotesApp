using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NotesApi.Controllers;
using NotesApi.Data;
using NotesApi.Models;
using Xunit;

namespace NotesApi.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly NotesDbContext _context;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public AuthControllerTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<NotesDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb") // Use a different database name for each test
                .Options;

            _context = new NotesDbContext(options);
            _context.Database.EnsureDeleted();  // Clear the database before each test
            _context.Database.EnsureCreated();  // Recreate the database

            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new AuthController(_context, _mockConfiguration.Object);
        }

        [Fact]
        public void Signup_ReturnsOk_WhenUserIsCreatedSuccessfully()
        {
            // Arrange
            var user = new User { Username = "newuser", PasswordHash = "password" };

            // Act
            var result = _controller.Signup(user);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.Contains("User created successfully.", response.ToString());
        }

        [Fact]
        public void Signup_ReturnsBadRequest_WhenUserAlreadyExists()
        {
            // Arrange
            var user = new User { Username = "existinguser", PasswordHash = "password" };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _controller.Signup(user);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.Contains("User already exists.", response.ToString());
        }

        [Fact]
        public void Login_ReturnsOkWithToken_WhenValidCredentials()
        {
            // Arrange
            var user = new User { Id = 1, Username = "validuser", PasswordHash = BCrypt.Net.BCrypt.HashPassword("validpassword") };
            _context.Users.Add(user);
            _context.SaveChanges();

            var loginUser = new User { Username = "validuser", PasswordHash = "validpassword" };

            // Configure JWT settings with a valid secret key
            _mockConfiguration.Setup(x => x["JwtSettings:SecretKey"]).Returns("thisisaverystrongsecretkey1234567890abcdef1234567890abcdef");
            _mockConfiguration.Setup(x => x["JwtSettings:TokenExpiryMinutes"]).Returns("60");
            _mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("audience");
            _mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("issuer");

            // Act
            var result = _controller.Login(loginUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
        }



        [Fact]
        public void Login_ReturnsUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var user = new User { Username = "invaliduser", PasswordHash = "wrongpassword" };

            // Act
            var result = _controller.Login(user);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value;
            Assert.Contains("Invalid username or password.", response.ToString());
        }
    }
}
