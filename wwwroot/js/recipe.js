import { recipeApi } from './api/recipeApi.js';

let currentPage = 1;
const pageSize = 5;

const recipeListEl = document.getElementById('recipe-list');
const loadMoreBtn = document.getElementById('load-more-btn');
const recipeDetailsEl = document.getElementById('recipe-details');
const backToListBtn = document.getElementById('back-to-list');
const recipeTitleEl = document.getElementById('recipe-title');
const recipeImageEl = document.getElementById('recipe-image');
const recipeDescriptionEl = document.getElementById('recipe-description');
const recipeIngredientsEl = document.getElementById('recipe-ingredients');
const recipeInstructionsEl = document.getElementById('recipe-instructions');

function getJwtToken() {
  return localStorage.getItem('jwtToken');
}

async function fetchRecipes(page) {
  try {
    const data = await recipeApi.fetchRecipes(page, pageSize);
    return data;
  } catch (error) {
    alert(error.message);
    return null;
  }
}

function renderRecipeList(recipes) {
  recipes.forEach(recipe => {
    const li = document.createElement('li');
    li.className = 'recipe-item';
    li.textContent = recipe.title;
    li.addEventListener('click', () => showRecipeDetails(recipe.id));
    recipeListEl.appendChild(li);
  });
}

async function showRecipeDetails(id) {
  try {
    const recipe = await recipeApi.fetchRecipeById(id);
    recipeTitleEl.textContent = recipe.title;
    recipeImageEl.src = recipe.imageUrl || '';
    recipeImageEl.alt = recipe.title;
    recipeDescriptionEl.textContent = recipe.description;

    recipeIngredientsEl.innerHTML = '';
    if (recipe.ingredients && recipe.ingredients.length > 0) {
      recipe.ingredients.forEach(ingredient => {
        const li = document.createElement('li');
        li.textContent = ingredient;
        recipeIngredientsEl.appendChild(li);
      });
    }

    recipeInstructionsEl.innerHTML = '';
    if (recipe.instructions && recipe.instructions.length > 0) {
      recipe.instructions.forEach(instruction => {
        const li = document.createElement('li');
        li.textContent = instruction;
        recipeInstructionsEl.appendChild(li);
      });
    }

    recipeListEl.style.display = 'none';
    loadMoreBtn.style.display = 'none';
    recipeDetailsEl.style.display = 'block';
  } catch (error) {
    alert(error.message);
  }
}

backToListBtn.addEventListener('click', () => {
  recipeDetailsEl.style.display = 'none';
  recipeListEl.style.display = 'block';
  loadMoreBtn.style.display = 'inline-block';
});

loadMoreBtn.addEventListener('click', async () => {
  currentPage++;
  const data = await fetchRecipes(currentPage);
  if (data && data.data.length > 0) {
    renderRecipeList(data.data);
    if (currentPage >= data.pagination.totalPages) {
      loadMoreBtn.style.display = 'none';
    }
  } else {
    loadMoreBtn.style.display = 'none';
  }
});

// Initial load
(async () => {
  const data = await fetchRecipes(currentPage);
  if (data) {
    renderRecipeList(data.data);
    if (currentPage >= data.pagination.totalPages) {
      loadMoreBtn.style.display = 'none';
    }
  }
})();
