using RecipeApp.Models;

namespace RecipeApp.Services
{
    public interface IRecipeService
    {
        // Local database operations
        Task<IEnumerable<Recipe>> GetAllRecipesAsync(int page = 1, int pageSize = 10);
        Task<(IEnumerable<Recipe>, int)> GetAllRecipesWithCountAsync(int page = 1, int pageSize = 10);
        Task<Recipe> GetRecipeByIdAsync(int id);
        Task<Recipe> CreateRecipeAsync(Recipe recipe);
        Task<bool> UpdateRecipeAsync(Recipe recipe);
        Task<bool> DeleteRecipeAsync(int id);

        // External API operations
        Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, string category, int page = 1, int pageSize = 10);
        Task<(IEnumerable<Recipe>, int)> SearchRecipesWithCountAsync(string query, string category, int page = 1, int pageSize = 10);
        Task<Recipe> GetRecipeFromExternalSourceAsync(int id, string source);
        Task<bool> HasActiveSubscriptionAsync(string userId);
        Task SubscribeUserAsync(string userId, int days);
}
    }
