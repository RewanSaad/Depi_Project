using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        public required string UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public bool IsActive => DateTime.UtcNow < ExpiryDate;
    }
}
