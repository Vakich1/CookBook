using System.ComponentModel.DataAnnotations;

namespace CookBook.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Recipe>? Recipes { get; set; }
}