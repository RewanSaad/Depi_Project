const text = "Discover, Cook, Enjoy!";
let index = 0;
let isDeleting = false;

function typeText() {
  const display = document.getElementById("typing-text");

  if (isDeleting) {
    display.textContent = text.substring(0, index--);
  } else {
    display.textContent = text.substring(0, index++);
  }

  if (index === text.length + 1) {
    isDeleting = true;
    setTimeout(typeText, 1000);
    return;
  } else if (index < 0) {
    isDeleting = false;
  }

  setTimeout(typeText, isDeleting ? 50 : 100);
}

window.onload = typeText;
document.getElementById("logo-recipe").onclick = function() {
    this.classList.add("shake");
  
    setTimeout(() => {
      this.classList.remove("shake");
    }, 300);
  };
let images = document.querySelectorAll(".recipe-card img");

// نلف عليهم
images.forEach(function(img) {
  img.addEventListener("mouseenter", function() {
    img.style.transform = "translateY(-10px)";
    img.style.transition = "transform 0.3s ease";
  });

  img.addEventListener("mouseleave", function() {
    img.style.transform = "translateY(0)";
  });
});
let title = document.getElementById("title");

title.addEventListener("mouseenter", function() {
  title.style.transform = "translateX(20px)";
  title.style.color = "red";
});

title.addEventListener("mouseleave", function() {
  title.style.transform = "translateX(0)";
  title.style.color = "#333";
});
let tex = document.getElementById("tex");

tex.addEventListener("mouseenter", function() {
  tex.style.transform = "translateX(20px)";
  tex.style.color = "red";
});

tex.addEventListener("mouseleave", function() {
  tex.style.transform = "translateX(0)";
  tex.style.color = "#333";
});

let te = document.getElementById("te");

te.addEventListener("mouseenter", function() {
  te.style.transform = "translateX(20px)";
  te.style.color = "red";
});

te.addEventListener("mouseleave", function() {
  te.style.transform = "translateX(0)";
  te.style.color = "#333";
});
let reviewCards = document.querySelectorAll(".review-card");

reviewCards.forEach(function(card) {
  card.addEventListener("mouseenter", function() {
    card.classList.add("neon");
  });

  card.addEventListener("mouseleave", function() {
    card.classList.remove("neon");
  });
});

document.addEventListener('DOMContentLoaded', function() {
  const tabs = document.querySelectorAll('.ingredient-tab');
  const ingredientLists = document.querySelectorAll('.ingredient-list');

  tabs.forEach(tab => {
      tab.addEventListener('click', function() {
          const tabId = this.dataset.tab;

          tabs.forEach(t => t.classList.remove('active'));
          ingredientLists.forEach(list => list.classList.remove('active'));

          this.classList.add('active');
          document.getElementById(`${tabId}-ingredients`).classList.add('active');
      });
  });

  // Fetch and display featured recipes dynamically
  async function fetchFeaturedRecipes() {
    try {
      const response = await fetch('/api/recipes?page=1&pageSize=3', { credentials: 'include' });
      if (response.status === 403) {
        alert('You need an active subscription to view recipes.');
        return;
      }
      if (!response.ok) {
        throw new Error('Failed to fetch recipes');
      }
      const result = await response.json();
      const recipeGrid = document.querySelector('.recipe-grid');
      recipeGrid.innerHTML = ''; // Clear existing static recipes

      result.data.forEach(recipe => {
        const card = document.createElement('div');
        card.classList.add('recipe-card');
        card.innerHTML = `
          <a href="recipe.html?id=${recipe.id}">
            <img src="${recipe.imageUrl}" alt="${recipe.title}">
          </a>
          <div class="recipe-text">
            <h3>${recipe.title}</h3>
            <p class="tag">Featured</p>
            <p>${recipe.description.substring(0, 100)}...</p>
          </div>
        `;
        recipeGrid.appendChild(card);
      });
    } catch (error) {
      console.error('Error fetching featured recipes:', error);
    }
  }

  fetchFeaturedRecipes();
});
