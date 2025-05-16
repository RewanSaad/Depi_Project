using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeApp.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [NotMapped]
        public List<string> Ingredients { get; set; } = new List<string>();

        [Required]
        public string IngredientsJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Ingredients);
            set => Ingredients = System.Text.Json.JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
        }

        [NotMapped]
        public List<string> Instructions { get; set; } = new List<string>();

        [Required]
        public string InstructionsJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Instructions);
            set => Instructions = System.Text.Json.JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
        }

        public string? ImageUrl { get; set; }

        public int CookingTime { get; set; } // in minutes

        public int Servings { get; set; }

        public string? Difficulty { get; set; }

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? UserId { get; set; } // For user who created the recipe

        public List<RecipeFeedback> Feedbacks { get; set; } = new List<RecipeFeedback>();
    }
}
