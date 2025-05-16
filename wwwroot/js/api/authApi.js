const authApi = (() => {
  const baseUrl = 'http://localhost:5241/api/auth';

  async function register(user) {
    const response = await fetch(`${baseUrl}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(user)
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Registration failed');
    }

    return await response.json();
  }

  async function login(credentials) {
    const response = await fetch(`${baseUrl}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });

    const result = await response.json();

    if (!response.ok) {
      throw new Error(result.message || 'Login failed');
    }

    if (result.token) {
      localStorage.setItem("token", result.token);
    }

    return result;
  }

  return {
    register,
    login
  };
})();

export { authApi };
