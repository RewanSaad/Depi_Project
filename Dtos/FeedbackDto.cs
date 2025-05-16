using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Dtos
{
    public class FeedbackDto
    {
        [Required]
        public int RecipeId { get; set; }

        [Required]
        public required string Comment { get; set; }
    }
}
