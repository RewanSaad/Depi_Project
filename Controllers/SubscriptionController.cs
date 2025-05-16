using Microsoft.AspNetCore.Mvc;
using RecipeApp.Services;
using System.Threading.Tasks;

namespace RecipeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        public SubscriptionController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpGet("publishable-key")]
        public IActionResult GetPublishableKey()
        {
            var configuration = HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
            var key = configuration["Stripe:PublishableKey"];
            return Ok(new { publishableKey = key });
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var session = await _stripeService.CreateCheckoutSessionAsync(request.CustomerEmail, request.PriceId);
            return Ok(new { sessionId = session.Id, url = session.Url });
        }
    }

    public class CreateCheckoutSessionRequest
    {
        public string CustomerEmail { get; set; }
        public string PriceId { get; set; }
    }
}
