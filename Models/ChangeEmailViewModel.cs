using System.ComponentModel.DataAnnotations;

namespace CookBook.Models;

public class ChangeEmailViewModel
{
    [Required(ErrorMessage = "Новый email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Новый email")]
    public string NewEmail { get; set; }

    [Required(ErrorMessage = "Текущий пароль обязателен")]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; }
}