#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;

namespace FlashWash.Models;
public class LoginUser
{


    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")] 
    [Display(Name = "Email")]
    public string LoginEmail { get; set; }

    
    [Required(ErrorMessage = "Please enter your password.")]
    [MinLength(6, ErrorMessage = "Please enter a valid password.")]
    [DataType(DataType.Password)]  
    [Display(Name = "Password")]
    public string LoginPassword { get; set; }
}