using System.ComponentModel.DataAnnotations;

namespace CookBook.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Текущий пароль обязателен")]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; }
    
    [Required(ErrorMessage = "Новый пароль обязателен")]
    [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение нового пароля")]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}