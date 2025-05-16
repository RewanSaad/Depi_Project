using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RecipeApp.Models;
using RecipeApp.Services;
using RecipeApp.Dtos;
using RecipeCreateDtoAlias = RecipeApp.Dtos.RecipeCreateDto;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;

namespace RecipeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly Data.RecipeDbContext _context;
        private readonly Microsoft.Extensions.Logging.ILogger<RecipesController> _logger;

        public RecipesController(IRecipeService recipeService, Data.RecipeDbContext context, Microsoft.Extensions.Logging.ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _context = context;
            _logger = logger;
        }

        // GET: api/recipes?searchTerm=xxx
        [HttpGet]
        public async Task<ActionResult> GetRecipes(
            [FromQuery] string searchTerm = "",
            [FromQuery] string category = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !await _recipeService.HasActiveSubscriptionAsync(userId))
            {
                return Forbid();
            }
            // var userId = "test-user"; // Temporary userId to fix build error
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            try
            {
                int totalCount;
                IEnumerable<Recipe> recipes;

                if (string.IsNullOrEmpty(searchTerm))
                {
                    (recipes, totalCount) = await _recipeService.GetAllRecipesWithCountAsync(page, pageSize);
                }
                else
                {
                    (recipes, totalCount) = await _recipeService.SearchRecipesWithCountAsync(searchTerm, category, page, pageSize);
                }

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new
                {
                    Data = recipes,
                    Pagination = new
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception here if logging is set up
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/recipes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !await _recipeService.HasActiveSubscriptionAsync(userId))
            {
                return Forbid();
            }
            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe == null)
                {
                    return NotFound();
                }
                return Ok(recipe);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRecipe([FromForm] RecipeCreateDtoAlias recipeDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !await _recipeService.HasActiveSubscriptionAsync(userId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(recipeDto.Title) || string.IsNullOrWhiteSpace(recipeDto.Description))
            {
                return BadRequest("Title and Description are required.");
            }

            if (recipeDto.Image == null || recipeDto.Image.Length == 0)
            {
                return BadRequest("Image is required.");
            }

            try
            {
                var imageUrl = await SaveImageAsync(recipeDto.Image);

                var recipe = new Recipe
                {
                    Title = recipeDto.Title,
                    Description = recipeDto.Description,
                    Ingredients = recipeDto.Ingredients?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                    Instructions = recipeDto.Instructions?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                    ImageUrl = imageUrl,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _recipeService.CreateRecipeAsync(recipe);

                return Ok("Recipe created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                return StatusCode(500, "An error occurred while creating the recipe.");
            }
        }

        [Authorize]
        [HttpPost("feedback")]
        public async Task<IActionResult> AddFeedback([FromBody] Dtos.FeedbackDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(dto.Comment))
            {
                return BadRequest("Comment cannot be empty.");
            }

            try
            {
                var recipe = await _context.Recipes.FindAsync(dto.RecipeId);
                if (recipe == null)
                {
                    return NotFound("Recipe not found.");
                }

                var feedback = new Models.RecipeFeedback
                {
                    RecipeId = dto.RecipeId,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    Recipe = recipe
                };

                _context.Add(feedback);
                await _context.SaveChangesAsync();

                return Ok("Feedback added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding feedback");
                return StatusCode(500, "An error occurred while adding feedback.");
            }
        }

        private async Task<string?> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }
    }
}
