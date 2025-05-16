using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeApp.Data;
using RecipeApp.Models;
using RecipeApp.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<RecipeDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key not found")))
    };
});

// Add application services
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IStripeService, StripeService>();

var app = builder.Build();

// Seed sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<RecipeDbContext>();

    if (!dbContext.Recipes.Any())
    {
        dbContext.Recipes.AddRange(
            new Recipe
            {
                Title = "Oreo Mini Cheesecake",
                Category = "Dessert",
                Description = "Delicious mini cheesecakes made with Oreo cookies",
                Instructions = new List<string>
                {
                    "Mix crushed Oreos with melted butter for the base",
                    "Press into mini cheesecake molds",
                    "Mix cream cheese, sugar, and vanilla",
                    "Pour over bases and bake",
                    "Chill and serve"
                },
                Ingredients = new List<string>
                {
                    "Oreo Cookies",
                    "Cream Cheese",
                    "Sugar",
                    "Butter",
                    "Vanilla Extract"
                },
                CookingTime = 30,
                Servings = 12,
                Difficulty = "Easy"
            },
            new Recipe
            {
                Title = "Lemon Curd Layer Cake",
                Category = "Dessert",
                Description = "Light and fluffy cake layers filled with tangy lemon curd",
                Instructions = new List<string>
                {
                    "Bake three layers of sponge cake",
                    "Make lemon curd",
                    "Prepare buttercream frosting",
                    "Layer cakes with lemon curd",
                    "Frost with buttercream"
                },
                Ingredients = new List<string>
                {
                    "Cake Mix",
                    "Eggs",
                    "Butter",
                    "Milk",
                    "Lemons",
                    "Sugar",
                    "Heavy Cream"
                },
                CookingTime = 90,
                Servings = 8,
                Difficulty = "Medium"
            }
        );
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowFrontend");

app.Run();
