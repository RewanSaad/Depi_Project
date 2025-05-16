// Fade in elements on scroll
const faders = document.querySelectorAll('.hero-text, .hero-image, .story h2, .team h2, .team-card, .mission h2');

const appearOptions = {
    threshold: 0.3
};

const appearOnScroll = new IntersectionObserver(function(entries, appearOnScroll) {
    entries.forEach(entry => {
        if (!entry.isIntersecting) {
            return;
        } else {
            entry.target.classList.add('appear');
            appearOnScroll.unobserve(entry.target);
        }
    });
}, appearOptions);

faders.forEach(fader => {
    appearOnScroll.observe(fader);
});

document.addEventListener('DOMContentLoaded', function() {
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    const navbar = document.querySelector('.navbar');
    const logoSpans = document.querySelectorAll('.logo span');
    const body = document.body;

    // Wave animation for logo
    logoSpans.forEach((span, index) => {
        span.style.animationDelay = `${index * 0.15}s`;
    });

    // Handle mobile menu toggle with smooth animation
    navbarToggler.addEventListener('click', function() {
        navbarCollapse.classList.toggle('active');
        body.style.overflow = navbarCollapse.classList.contains('active') ? 'hidden' : '';
        // Toggle aria-expanded for accessibility
        const isExpanded = navbarCollapse.classList.contains('active');
        navbarToggler.setAttribute('aria-expanded', isExpanded);
    });

    // Close mobile menu when clicking outside
    document.addEventListener('click', function(event) {
        if (!navbarCollapse.contains(event.target) && !navbarToggler.contains(event.target)) {
            navbarCollapse.classList.remove('active');
            body.style.overflow = '';
            navbarToggler.setAttribute('aria-expanded', 'false');
        }
    });

    // Handle navbar scroll effect
    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });

    // Close mobile menu when window is resized to desktop view
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            navbarCollapse.classList.remove('active');
            body.style.overflow = '';
            navbarToggler.setAttribute('aria-expanded', 'false');
        }
    });

    // Section appear on scroll
    const appearEls = document.querySelectorAll(
        'section, .team-card, .hero-text, .hero-image'
    );
    const appearOnScroll = new IntersectionObserver(
        (entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('appear');
                    observer.unobserve(entry.target);
                }
            });
        },
        { threshold: 0.15 }
    );
    appearEls.forEach(el => appearOnScroll.observe(el));

    // New code to link backend with frontend

    // Variables for pagination
    let currentPage = 1;
    const pageSize = 5;

    const recipesList = document.getElementById('recipes-list');
    const loadMoreBtn = document.getElementById('load-more-recipes');
    const createRecipeSection = document.getElementById('create-recipe');
    const createRecipeForm = document.getElementById('create-recipe-form');
    const feedbackSection = document.getElementById('feedback');
    const feedbackForm = document.getElementById('feedback-form');
    const subscriptionSection = document.getElementById('subscription');
    const subscribeBtn = document.getElementById('subscribe-btn');

    // Function to fetch recipes from backend
    async function fetchRecipes(page = 1) {
        try {
            const response = await fetch(`/api/recipes?page=${page}&pageSize=${pageSize}`, {
                credentials: 'include'
            });
            if (response.status === 403) {
                alert('You need an active subscription to view recipes.');
                subscriptionSection.style.display = 'block';
                return { data: [], pagination: {} };
            }
            if (!response.ok) {
                throw new Error('Failed to fetch recipes');
            }
            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error fetching recipes:', error);
            return { data: [], pagination: {} };
        }
    }

    // Function to render recipes
    function renderRecipes(recipes) {
        recipes.forEach(recipe => {
            const recipeDiv = document.createElement('div');
            recipeDiv.classList.add('recipe-card');
            recipeDiv.innerHTML = `
                <h3>${recipe.title}</h3>
                <img src="${recipe.imageUrl}" alt="${recipe.title}" style="max-width: 100%; height: auto;" />
                <p>${recipe.description}</p>
                <button class="btn feedback-btn" data-recipe-id="${recipe.id}">Add Feedback</button>
            `;
            recipesList.appendChild(recipeDiv);
        });
        // Add event listeners to feedback buttons
        document.querySelectorAll('.feedback-btn').forEach(button => {
            button.addEventListener('click', () => {
                const recipeId = button.getAttribute('data-recipe-id');
                feedbackSection.style.display = 'block';
                feedbackForm['recipe-id'].value = recipeId;
                window.scrollTo({ top: feedbackSection.offsetTop, behavior: 'smooth' });
            });
        });
    }

    // Load initial recipes
    async function loadInitialRecipes() {
        const result = await fetchRecipes(currentPage);
        renderRecipes(result.data);
        if (currentPage >= result.pagination.totalPages) {
            loadMoreBtn.style.display = 'none';
        } else {
            loadMoreBtn.style.display = 'block';
        }
    }

    loadInitialRecipes();

    // Load more recipes on button click
    loadMoreBtn.addEventListener('click', async () => {
        currentPage++;
        const result = await fetchRecipes(currentPage);
        renderRecipes(result.data);
        if (currentPage >= result.pagination.totalPages) {
            loadMoreBtn.style.display = 'none';
        }
    });

    // Show create recipe form on Discover Recipes button click
    document.getElementById('discover-recipes-btn').addEventListener('click', () => {
        createRecipeSection.style.display = 'block';
        window.scrollTo({ top: createRecipeSection.offsetTop, behavior: 'smooth' });
    });

    // Handle create recipe form submission
    createRecipeForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(createRecipeForm);
        try {
            const response = await fetch('/api/recipes/create', {
                method: 'POST',
                body: formData,
                credentials: 'include'
            });
            if (response.status === 403) {
                alert('You need an active subscription to create recipes.');
                subscriptionSection.style.display = 'block';
                return;
            }
            if (!response.ok) {
                const errorText = await response.text();
                alert('Error creating recipe: ' + errorText);
                return;
            }
            alert('Recipe created successfully!');
            createRecipeForm.reset();
            createRecipeSection.style.display = 'none';
            // Reload recipes list
            currentPage = 1;
            recipesList.innerHTML = '';
            loadInitialRecipes();
        } catch (error) {
            console.error('Error creating recipe:', error);
            alert('Error creating recipe.');
        }
    });

    // Handle feedback form submission
    feedbackForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const recipeId = feedbackForm['recipe-id'].value;
        const comment = feedbackForm['comment'].value.trim();
        if (!comment) {
            alert('Comment cannot be empty.');
            return;
        }
        try {
            const response = await fetch('/api/recipes/feedback', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ recipeId: parseInt(recipeId), comment }),
                credentials: 'include'
            });
            if (response.status === 403) {
                alert('You need an active subscription to submit feedback.');
                subscriptionSection.style.display = 'block';
                return;
            }
            if (!response.ok) {
                const errorText = await response.text();
                alert('Error submitting feedback: ' + errorText);
                return;
            }
            alert('Feedback submitted successfully!');
            feedbackForm.reset();
            feedbackSection.style.display = 'none';
        } catch (error) {
            console.error('Error submitting feedback:', error);
            alert('Error submitting feedback.');
        }
    });

    // Handle subscription button click
    subscribeBtn.addEventListener('click', async () => {
        try {
            // Get publishable key
            const keyResponse = await fetch('/api/subscription/publishable-key');
            if (!keyResponse.ok) {
                alert('Failed to get Stripe publishable key.');
                return;
            }
            const { publishableKey } = await keyResponse.json();

            // Initialize Stripe
            const stripe = Stripe(publishableKey);

            // Create checkout session
            const email = prompt('Please enter your email for subscription:');
            if (!email) {
                alert('Email is required for subscription.');
                return;
            }
            // Assuming a fixed priceId for subscription; adjust as needed
            const priceId = 'price_1234567890abcdef'; // Replace with actual priceId from Stripe dashboard

            const sessionResponse = await fetch('/api/subscription/create-checkout-session', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ customerEmail: email, priceId })
            });
            if (!sessionResponse.ok) {
                alert('Failed to create checkout session.');
                return;
            }
            const { sessionId, url } = await sessionResponse.json();

            // Redirect to Stripe checkout
            window.location.href = url;
        } catch (error) {
            console.error('Subscription error:', error);
            alert('Error during subscription process.');
        }
    });
});
