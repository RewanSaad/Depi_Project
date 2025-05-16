using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RecipeApp.Models
{
    public class RecipeCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Ingredients { get; set; }
        public List<string> Instructions { get; set; }
        public IFormFile Image { get; set; }
    }
}
