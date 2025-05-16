const recipeApi = (() => {
  const baseUrl = '/api/recipes';

  function getJwtToken() {
    return localStorage.getItem('jwtToken');
  }

  async function fetchRecipes(page = 1, pageSize = 5) {
    const token = getJwtToken();
    if (!token) throw new Error('Not authenticated');
    const response = await fetch(`${baseUrl}?page=${page}&pageSize=${pageSize}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!response.ok) throw new Error('Failed to fetch recipes');
    return response.json();
  }

  async function fetchRecipeById(id) {
    const token = getJwtToken();
    if (!token) throw new Error('Not authenticated');
    const response = await fetch(`${baseUrl}/${id}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!response.ok) throw new Error('Failed to fetch recipe');
    return response.json();
  }

  async function createRecipe(formData) {
    const token = getJwtToken();
    if (!token) throw new Error('Not authenticated');
    const response = await fetch(`${baseUrl}/create`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData
    });
    if (!response.ok) throw new Error('Failed to create recipe');
    return response.text();
  }

  async function addFeedback(feedback) {
    const token = getJwtToken();
    if (!token) throw new Error('Not authenticated');
    const response = await fetch(`${baseUrl}/feedback`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(feedback)
    });
    if (!response.ok) throw new Error('Failed to add feedback');
    return response.text();
  }

  return {
    fetchRecipes,
    fetchRecipeById,
    createRecipe,
    addFeedback
  };
})();
