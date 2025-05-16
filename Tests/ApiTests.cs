using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using RecipeApp.Models;
using RecipeApp.Dtos;
using System.Collections.Generic;

namespace RecipeApp.Tests
{
    [TestFixture]
    public class ApiTests
    {
        private static HttpClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            // Setup HttpClient to point to your running API server
            _client = new HttpClient();
            _client.BaseAddress = new System.Uri("https://localhost:5001/api/");
        }

        [Test]
        public async Task Test_GetPublishableKey()
        {
            var response = await _client.GetAsync("subscription/publishable-key");
            response.EnsureSuccessStatusCode();
            var key = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrEmpty(key));
        }

        [Test]
        public async Task Test_CreateCheckoutSession()
        {
            var request = new { PriceId = "price_test", CustomerEmail = "test@example.com" };
            var response = await _client.PostAsJsonAsync("subscription/create-checkout-session", request);
            response.EnsureSuccessStatusCode();
            var session = await response.Content.ReadFromJsonAsync<object>();
            Assert.IsNotNull(session);
        }

        [Test]
        public async Task Test_GetRecipes_Unauthorized()
        {
            var response = await _client.GetAsync("recipes");
            Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task Test_CreateRecipe_Validation()
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(""), "Title");
            formData.Add(new StringContent(""), "Description");
            var response = await _client.PostAsync("recipes/create", formData);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Test_AddFeedback_Validation()
        {
            var feedback = new FeedbackDto { RecipeId = 1, Comment = "" };
            var response = await _client.PostAsJsonAsync("recipes/feedback", feedback);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Add more tests for happy paths, edge cases, and error handling as needed
    }
}
