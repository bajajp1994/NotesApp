namespace NotesApi.Models
{
    public class SharedNote
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int UserId { get; set; } // The user receiving the shared note
        public int SharedByUserId { get; set; } // The user who shared the note
        public DateTime SharedAt { get; set; }
    }
}
