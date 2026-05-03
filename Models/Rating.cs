using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CookBook.Models;

public class Rating
{
    public int Id { get; set; }
    
    [Required]
    public int RecipeId { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    [Range(0.5, 5.0)]
    public double Value { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [ForeignKey("RecipeId")]
    public Recipe Recipe { get; set; }
    
    [ForeignKey("UserId")]
    public IdentityUser User { get; set; }
}