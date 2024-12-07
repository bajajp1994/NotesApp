using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace NotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Ensure the user is authenticated
    public class NotesController : ControllerBase
    {
        private readonly NotesDbContext _context;

        public NotesController(NotesDbContext context)
        {
            _context = context;
        }

        // GET: api/notes
        [HttpGet]
        public IActionResult GetAllNotes()
        {
            var userId = GetUserIdFromToken();  // Get userId from JWT token
            var notes = _context.Notes
                .Where(n => n.UserId == userId)
                .ToList();

            return Ok(notes);
        }

        // GET: api/notes/{id}
        [HttpGet("{id}")]
        public IActionResult GetNoteById(int id)
        {
            var userId = GetUserIdFromToken();  // Get userId from JWT token
            var note = _context.Notes
                .FirstOrDefault(n => n.Id == id && n.UserId == userId);

            if (note == null)
                return NotFound();

            return Ok(note);
        }

        // POST: api/notes
        [HttpPost]
        public IActionResult CreateNote([FromBody] Note note)
        {
            if (note == null)
                return BadRequest();

            var userId = GetUserIdFromToken();  // Get userId from JWT token
            Console.WriteLine("userId= ",userId);
            note.UserId = userId;
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            _context.Notes.Add(note);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        // PUT: api/notes/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateNote(int id, [FromBody] Note updatedNote)
        {
            var userId = GetUserIdFromToken();  // Get userId from JWT token
            var note = _context.Notes
                .FirstOrDefault(n => n.Id == id && n.UserId == userId);

            if (note == null)
                return NotFound();

            note.Title = updatedNote.Title;
            note.Content = updatedNote.Content;
            note.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(note);
        }

        // DELETE: api/notes/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteNote(int id)
        {
            var userId = GetUserIdFromToken();  // Get userId from JWT token
            var note = _context.Notes
                .FirstOrDefault(n => n.Id == id && n.UserId == userId);

            if (note == null)
                return NotFound();

            _context.Notes.Remove(note);
            _context.SaveChanges();

            return Ok("Note successfully deleted");
        }

        [HttpPost("{id}/share")] // Route for sharing a note with another user
        public async Task<IActionResult> ShareNoteAsync(int id, [FromBody] ShareNoteRequest request)
        {
            var userId = GetUserIdFromToken();  // Get userId from JWT token

            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Validate the note exists and belongs to the authenticated user
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound(new { message = "Note not found or you do not have permission to share this note" });
            }

            // Check if recipient user exists
            var recipient = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.RecipientUserId);

            if (recipient == null)
            {
                return NotFound(new { message = "Recipient user not found" });
            }

            // Add logic to share the note with the recipient
            var sharedNote = new SharedNote
            {
                NoteId = note.Id,
                UserId = recipient.Id, // Recipient user ID
                SharedByUserId = userId, // The current user who is sharing the note
                SharedAt = DateTime.UtcNow
            };

            // Save the shared note entry to the database
            _context.SharedNotes.Add(sharedNote);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note shared successfully" });
        }

        // Route for searching a note with query

        [HttpGet("search")]
        public async Task<IActionResult> SearchNotesAsync([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest(new { message = "Search query cannot be empty" });
            }

            var userId = GetUserIdFromToken();  // Get userId from JWT token
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Find notes owned by the user and notes shared with the user
            var matchingNotes = await _context.Notes
                                              .Where(n => n.UserId == userId &&
                                                          (n.Title.Contains(q) || n.Content.Contains(q)))
                                              .ToListAsync();

            var sharedNotes = await _context.SharedNotes
                                             .Where(sn => sn.UserId == userId)
                                             .Join(_context.Notes,
                                                   sn => sn.NoteId,
                                                   n => n.Id,
                                                   (sn, n) => new { n.Id, n.Title, n.Content, n.UserId, n.CreatedAt, n.UpdatedAt })
                                             .Where(n => n.Title.Contains(q) || n.Content.Contains(q))
                                             .ToListAsync();

            // Combine both owned and shared notes
            matchingNotes.AddRange(sharedNotes.Select(sn => new Note
            {
                Id = sn.Id,
                Title = sn.Title,
                Content = sn.Content,
                UserId = sn.UserId,
                CreatedAt = sn.CreatedAt,
                UpdatedAt = sn.UpdatedAt
            }));

            if (!matchingNotes.Any())
            {
                return NotFound(new { message = "No notes found matching your search" });
            }

            return Ok(matchingNotes);
        }

        // Helper method to extract user ID from JWT token (you can customize this to your token)
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new ArgumentNullException("User ID is missing in the token");
            }

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new ArgumentException("Invalid user ID in token");
        }
    }
}


