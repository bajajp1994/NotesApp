using NotesApi.Models;
using System.Text.Json.Serialization;

namespace NotesApi.Models
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}

