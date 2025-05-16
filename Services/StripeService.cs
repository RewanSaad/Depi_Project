using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace RecipeApp.Services
{
    public class StripeService : IStripeService
    {
        private readonly string _stripeApiKey;
        private readonly string _webhookSecret;

        public StripeService(IConfiguration configuration)
        {
            _stripeApiKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe secret key is not configured.");
            _webhookSecret = configuration["Stripe:WebhookSecret"] ?? throw new ArgumentNullException("Stripe webhook secret is not configured.");
            StripeConfiguration.ApiKey = _stripeApiKey;
        }

        public async Task<Session> CreateCheckoutSessionAsync(string customerEmail, string priceId)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                CustomerEmail = customerEmail,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                SuccessUrl = "https://yourdomain.com/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://yourdomain.com/cancel",
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }

        public async Task<bool> VerifyWebhookSignatureAsync(string json, string stripeSignature, string webhookSecret)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
                return stripeEvent != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
