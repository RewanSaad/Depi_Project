using System;

namespace RecipeApp.Models
{
    public class RecipeFeedback
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }  // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RecipeId { get; set; }
        public required Recipe Recipe { get; set; }
    }
}
