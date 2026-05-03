using System.ComponentModel.DataAnnotations;

namespace CookBook.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
    
    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}