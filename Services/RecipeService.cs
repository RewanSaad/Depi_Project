using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RecipeApp.Data;
using RecipeApp.Models;
using System.Net.Http.Json;

namespace RecipeApp.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly RecipeDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private const string SPOONACULAR_API_KEY = "60a57740c38a475a8123a0c371905caf";
        private const string TASTY_API_KEY = "7628e990d1mshc37ebe975c77b10p116b6fjsn37133bf70f75";

        public RecipeService(
            RecipeDbContext context,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Recipes
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Recipe>, int)> GetAllRecipesWithCountAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Recipes.AsQueryable();

            var totalCount = await query.CountAsync();

            var recipes = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (recipes, totalCount);
        }

        public async Task<(IEnumerable<Recipe>, int)> SearchRecipesWithCountAsync(string query, string category, int page = 1, int pageSize = 10)
        {
            var recipesQuery = _context.Recipes.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                recipesQuery = recipesQuery.Where(r => r.Title.Contains(query) || r.Description.Contains(query));
            }

            if (!string.IsNullOrEmpty(category))
            {
                recipesQuery = recipesQuery.Where(r => r.Category == category);
            }

            var totalCount = await recipesQuery.CountAsync();

            var recipes = await recipesQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (recipes, totalCount);
        }

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        public async Task<Recipe?> GetRecipeByIdAsync(int id)
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        {
            return await _context.Recipes.FindAsync(id);
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            var subscription = await _context.Subscriptions
                .Where(s => s.UserId == recipe.UserId && s.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                throw new InvalidOperationException("User does not have an active subscription.");
            }

            recipe.CreatedAt = DateTime.UtcNow;
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<bool> UpdateRecipeAsync(Recipe recipe)
        {
            var existingRecipe = await _context.Recipes.FindAsync(recipe.Id);
            if (existingRecipe == null)
            {
                return false;
            }

            existingRecipe.Title = recipe.Title ?? existingRecipe.Title;
            existingRecipe.Description = recipe.Description ?? existingRecipe.Description;
            existingRecipe.Ingredients = recipe.Ingredients ?? existingRecipe.Ingredients;
            existingRecipe.Instructions = recipe.Instructions ?? existingRecipe.Instructions;
            existingRecipe.ImageUrl = recipe.ImageUrl ?? existingRecipe.ImageUrl;
            existingRecipe.CookingTime = recipe.CookingTime;
            existingRecipe.Servings = recipe.Servings;
            existingRecipe.Difficulty = recipe.Difficulty ?? existingRecipe.Difficulty;
            existingRecipe.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRecipeAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return false;
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, string category, int page = 1, int pageSize = 10)
        {
            string cacheKey = $"recipes_{query}_{category}_{page}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<Recipe> cachedRecipes))
            {
                return cachedRecipes;
            }

            try
            {
                var recipes = await FetchFromSpoonacular(query, category, page, pageSize);
                if (recipes == null)
                {
                    return new List<Recipe>();
                }
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.Set(cacheKey, recipes, cacheOptions);
                return recipes;
            }
            catch (Exception)
            {
                // If Spoonacular fails, try Tasty API
                var recipes = await FetchFromTasty(query, category, page, pageSize);
                if (recipes == null)
                {
                    return new List<Recipe>();
                }
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.Set(cacheKey, recipes, cacheOptions);
                return recipes;
            }
        }

        public async Task<Recipe> GetRecipeFromExternalSourceAsync(int id, string source)
        {
            string cacheKey = $"recipe_{source}_{id}";

            if (_cache.TryGetValue(cacheKey, out Recipe cachedRecipe))
            {
                return cachedRecipe;
            }

            Recipe recipe;
            if (source.ToLower() == "spoonacular")
            {
                recipe = await GetRecipeFromSpoonacular(id);
            }
            else
            {
                recipe = await GetRecipeFromTasty(id);
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
            _cache.Set(cacheKey, recipe, cacheOptions);

            return recipe;
        }

        private async Task<IEnumerable<Recipe>> FetchFromSpoonacular(string query, string category, int page, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.spoonacular.com/recipes/complexSearch?apiKey={SPOONACULAR_API_KEY}&query={query}&diet={category}&addRecipeInformation=true&offset={(page - 1) * pageSize}&number={pageSize}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SpoonacularResponse>();
            if (result?.Results == null || !result.Results.Any())
            {
                throw new Exception("No recipes found in Spoonacular API.");
            }

            return result.Results.Select(r => new Recipe
            {
                Title = r.Title!,
                Category = r.DishTypes?.FirstOrDefault() ?? "",
                Description = r.Summary ?? "",
                Instructions = r.Instructions?.Split('\n').ToList() ?? new List<string>(),
                Ingredients = r.ExtendedIngredients?.Select(i => i.Name!).ToList() ?? new List<string>(),
                ImageUrl = r.Image,
                CookingTime = r.ReadyInMinutes,
                Servings = r.Servings,
                Difficulty = r.DishTypes?.FirstOrDefault() ?? "Medium"
            });
        }

        private async Task<IEnumerable<Recipe>> FetchFromTasty(string query, string category, int page, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", TASTY_API_KEY);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "tasty.p.rapidapi.com");

            var url = $"https://tasty.p.rapidapi.com/recipes/list?tags={category}&q={query}&from={(page - 1) * pageSize}&size={pageSize}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TastyResponse>();
            if (result?.Results == null || !result.Results.Any())
            {
                throw new Exception("No recipes found in Tasty API.");
            }

            return result.Results.Select(r => new Recipe
            {
                Title = r.Name!,
                Category = r.Tags?.FirstOrDefault()?.Name ?? "",
                Description = r.Description ?? "",
                Instructions = r.Instructions?.Select(i => i.DisplayText!).ToList() ?? new List<string>(),
                Ingredients = r.Sections?.SelectMany(s => s.Components)
                    .Where(c => !string.IsNullOrEmpty(c.RawText))
                    .Select(c => c.RawText!)
                    .ToList() ?? new List<string>(),
                ImageUrl = r.ThumbnailUrl,
                CookingTime = r.TotalTimeMinutes,
                Servings = r.NumServings,
                Difficulty = r.Tags?.FirstOrDefault()?.Name ?? "Medium"
            }) ?? new List<Recipe>();
        }

        private async Task<Recipe> GetRecipeFromSpoonacular(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.spoonacular.com/recipes/{id}/information?apiKey={SPOONACULAR_API_KEY}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var recipe = await response.Content.ReadFromJsonAsync<SpoonacularRecipe>();
            if (recipe == null)
            {
                throw new Exception("Recipe not found in Spoonacular API.");
            }

            return new Recipe
            {
                Title = recipe.Title!,
                Category = recipe.DishTypes?.FirstOrDefault() ?? "",
                Description = recipe.Summary ?? "",
                Instructions = recipe.Instructions?.Split('\n').ToList() ?? new List<string>(),
                Ingredients = recipe.ExtendedIngredients?.Select(i => i.Name!).ToList() ?? new List<string>(),
                ImageUrl = recipe.Image,
                CookingTime = recipe.ReadyInMinutes,
                Servings = recipe.Servings,
                Difficulty = recipe.DishTypes?.FirstOrDefault() ?? "Medium"
            };
        }

        private async Task<Recipe> GetRecipeFromTasty(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", TASTY_API_KEY);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "tasty.p.rapidapi.com");

            var url = $"https://tasty.p.rapidapi.com/recipes/get-more-info?id={id}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var recipe = await response.Content.ReadFromJsonAsync<TastyRecipe>();
            if (recipe == null)
            {
                throw new Exception("Recipe not found in Tasty API.");
            }

            return new Recipe
            {
                Title = recipe.Name!,
                Category = recipe.Tags?.FirstOrDefault()?.Name ?? "",
                Description = recipe.Description ?? "",
                Instructions = recipe.Instructions?.Select(i => i.DisplayText!).ToList() ?? new List<string>(),
                Ingredients = recipe.Sections?.SelectMany(s => s.Components)
                    .Where(c => !string.IsNullOrEmpty(c.RawText))
                    .Select(c => c.RawText!)
                    .ToList() ?? new List<string>(),
                ImageUrl = recipe.ThumbnailUrl,
                CookingTime = recipe.TotalTimeMinutes,
                Servings = recipe.NumServings,
                Difficulty = recipe.Tags?.FirstOrDefault()?.Name ?? "Medium"
            };
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
        {
            var sub = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ExpiryDate > DateTime.UtcNow);

            return sub != null;
        }

        public async Task SubscribeUserAsync(string userId, int days)
        {
            var existingSub = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSub != null)
            {
                existingSub.ExpiryDate = DateTime.UtcNow.AddDays(days);
            }
            else
            {
                var sub = new Subscription
                {
                    UserId = userId,
                    StartDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(days)
                };
                _context.Subscriptions.Add(sub);
            }

            await _context.SaveChangesAsync();
        }
    }

    // Spoonacular Models
    public class SpoonacularResponse
    {
        public List<SpoonacularRecipe>? Results { get; set; }
    }

    public class SpoonacularRecipe
    {
        public string? Title { get; set; }
        public List<string>? DishTypes { get; set; }
        public string? Instructions { get; set; }
        public string? Summary { get; set; }
        public List<Ingredient>? ExtendedIngredients { get; set; }
        public string? Image { get; set; }
        public int ReadyInMinutes { get; set; }
        public int Servings { get; set; }
    }

    public class Ingredient
    {
        public string? Name { get; set; }
    }

    // Tasty Models
    public class TastyResponse
    {
        public List<TastyRecipe>? Results { get; set; }
    }

    public class TastyRecipe
    {
        public string? Name { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<Instruction>? Instructions { get; set; }
        public List<Section>? Sections { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int TotalTimeMinutes { get; set; }
        public int NumServings { get; set; }
    }

    public class Tag
    {
        public string? Name { get; set; }
    }

    public class Instruction
    {
        public string? DisplayText { get; set; }
    }

    public class Section
    {
        public List<Component>? Components { get; set; }
    }

    public class Component
    {
        public string? RawText { get; set; }
    }
}
