using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CookBook.Models;

public class Recipe
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Введите название рецепта")]
    [StringLength(50, ErrorMessage = "Название не должно превышать 50 символов")]
    public string Title { get; set; }
    
    [Required(ErrorMessage = "Введите инструкцию")]
    [DataType(DataType.MultilineText)]
    public string Instruction { get; set; }
    
    [Required(ErrorMessage = "Введите ингредиенты")]
    [DataType(DataType.MultilineText)]
    public string Ingredients { get; set; }
    
    [Required(ErrorMessage = "Введите время приготовления")]
    [Range(1, 1440, ErrorMessage = "Время приготовления должно быть от 1 до 1440 минут")]
    public int CookingTime { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [Required(ErrorMessage = "Выберите категорию")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public string? UserId { get; set; }
    public IdentityUser? User { get; set; }
    
    public string? ImagePath { get; set; }
    
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    
    [NotMapped]
    public string? ThumbnailPath => !string.IsNullOrEmpty(ImagePath) 
        ? $"/uploads/recipes/thumb_{Path.GetFileName(ImagePath)}" 
        : null;
    
    public double? Rating { get; set; }
    
    public int RatingCount { get; set; }
    
    public ICollection<Rating>? Ratings { get; set; }
}