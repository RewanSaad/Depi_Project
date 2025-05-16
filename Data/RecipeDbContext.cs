using Microsoft.EntityFrameworkCore;
using RecipeApp.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RecipeApp.Data
{
    public class RecipeDbContext : IdentityDbContext<ApplicationUser>
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options) : base(options) {}

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeFeedback> RecipeFeedbacks { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            modelBuilder.Entity<Recipe>()
                .Property(r => r.Ingredients)
                .HasConversion(
                    v => string.Join("||", v),
                    v => v == null ? new List<string>() : v.Split(new string[] { "||" }, System.StringSplitOptions.None).ToList())
                .Metadata.SetValueComparer(stringListComparer);

            modelBuilder.Entity<Recipe>()
                .Property(r => r.Instructions)
                .HasConversion(
                    v => string.Join("||", v),
                    v => v == null ? new List<string>() : v.Split(new string[] { "||" }, System.StringSplitOptions.None).ToList())
                .Metadata.SetValueComparer(stringListComparer);

            modelBuilder.Entity<RecipeFeedback>()
                .HasOne(rf => rf.Recipe)
                .WithMany(r => r.Feedbacks)
                .HasForeignKey(rf => rf.RecipeId);
        }
    }
}
