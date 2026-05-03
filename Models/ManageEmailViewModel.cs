using System.ComponentModel.DataAnnotations;

namespace CookBook.Models;

public class ManageEmailViewModel
{
    [Display(Name = "Текущий email")]
    public string CurrentEmail { get; set; }
    
    [Display(Name = "Статус подтверждения")]
    public bool IsEmailConfirmed { get; set; }
}