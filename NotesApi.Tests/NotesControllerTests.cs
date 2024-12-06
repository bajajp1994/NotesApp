using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NotesApi.Controllers;
using NotesApi.Data;
using NotesApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;

public class NotesControllerTests
{
    private readonly NotesDbContext _context;
    private readonly NotesController _controller;

    public NotesControllerTests()
    {
        // Using an in-memory database for testing
        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb") // Name of the in-memory database
            .Options;

        _context = new NotesDbContext(options);
        _controller = new NotesController(_context);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("id", "1"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }))
            }
        };


        // Clear any existing data before tests run
        _context.Notes.RemoveRange(_context.Notes);
        _context.SaveChanges();
    }

    [Fact]
    public void GetAllNotes_ReturnsOkResult_WithUserNotes()
    {
        // Arrange
        var userId = 1;
        var notes = new List<Note>
    {
        new Note { Id = 1, Title = "Test Note 1", Content = "Content 1", UserId = userId },
        new Note { Id = 2, Title = "Test Note 2", Content = "Content 2", UserId = userId }
    };

        _context.Notes.AddRange(notes);
        _context.SaveChanges();

        // Act
        var result = _controller.GetAllNotes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedNotes = Assert.IsAssignableFrom<IEnumerable<Note>>(okResult.Value);
        Assert.Equal(2, returnedNotes.Count());
    }


    [Fact]
    public async Task GetNoteById_ReturnsNote_WhenNoteExists()
    {
        // Arrange
        var userId = 1;
        var note = new Note { Id = 1, Title = "Test Note", Content = "Content", UserId = userId };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = _controller.GetNoteById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); 
        var returnedNote = Assert.IsType<Note>(okResult.Value);
        Assert.Equal("Test Note", returnedNote.Title);
    }


    [Fact]
    public Task CreateNote_ReturnsCreatedNote()
    {
        // Arrange
        var note = new Note { Title = "New Note", Content = "New Content" };

        // Act
        var result = _controller.CreateNote(note);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdNote = Assert.IsType<Note>(createdResult.Value);
        Assert.Equal("New Note", createdNote.Title);
        return Task.CompletedTask;
    }


    [Fact]
    public async Task DeleteNote_ReturnsSuccess_WhenNoteExists()
    {
        // Arrange
        var userId = 1;
        var note = new Note { Id = 1, Title = "Test Note", Content = "Content", UserId = userId };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = _controller.DeleteNote(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); 
        var resultMessage = Assert.IsType<string>(okResult.Value);
        Assert.Equal("Note successfully deleted", resultMessage);
    }


    [Fact]
    public async Task SearchNotes_ReturnsMatchingNotes()
    {
        // Arrange
        var userId = 1;
        var notes = new List<Note>
    {
        new Note { Id = 1, Title = "Test Note", Content = "Matching Content", UserId = userId },
        new Note { Id = 2, Title = "Other Note", Content = "Non-matching Content", UserId = userId }
    };

        _context.Notes.AddRange(notes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SearchNotesAsync("Matching");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var matchingNotes = Assert.IsAssignableFrom<IEnumerable<Note>>(okResult.Value);
        Assert.Single(matchingNotes);
    }

}
