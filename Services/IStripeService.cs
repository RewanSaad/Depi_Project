using System.Threading.Tasks;
using Stripe.Checkout;

namespace RecipeApp.Services
{
    public interface IStripeService
    {
        Task<Session> CreateCheckoutSessionAsync(string customerEmail, string priceId);
        Task<bool> VerifyWebhookSignatureAsync(string json, string stripeSignature, string webhookSecret);
    }
}
