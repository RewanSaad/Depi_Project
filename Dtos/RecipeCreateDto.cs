using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Dtos
{
    public class RecipeCreateDto
    {
        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required string Ingredients { get; set; }

        [Required]
        public required string Instructions { get; set; }

        public required IFormFile Image { get; set; }
    }
}
