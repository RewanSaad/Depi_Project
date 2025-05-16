const subscriptionApi = (() => {
  const baseUrl = '/api/subscription';

  async function getPublishableKey() {
    const response = await fetch(`${baseUrl}/publishable-key`);
    if (!response.ok) throw new Error('Failed to get publishable key');
    return response.json();
  }

  async function createCheckoutSession(customerEmail, priceId) {
    const response = await fetch(`${baseUrl}/create-checkout-session`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ customerEmail, priceId })
    });
    if (!response.ok) throw new Error('Failed to create checkout session');
    return response.json();
  }

  return {
    getPublishableKey,
    createCheckoutSession
  };
})();
