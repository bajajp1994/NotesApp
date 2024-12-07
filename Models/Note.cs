using NotesApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace NotesApi.Models
{
    public class Note
    {
        [SwaggerSchema(ReadOnly = true, Description = "This property is hidden from Swagger.")]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
       
        [SwaggerSchema(ReadOnly = true, WriteOnly = false, Description = "This property is hidden from Swagger.")]
        public int UserId { get; set; }
        
        [SwaggerSchema(ReadOnly = true, WriteOnly = false, Description = "This property is hidden from Swagger.")]
        public DateTime CreatedAt { get; set; }
      
        [SwaggerSchema(ReadOnly = true, WriteOnly = false, Description = "This property is hidden from Swagger.")]
        public DateTime UpdatedAt { get; set; }

    }
}
