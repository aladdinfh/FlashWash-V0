using FlashWash.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "First name is required !!!!!!!")]

    [MinLength(2)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required !!!!!!!")]

    [MinLength(2)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email address must be present 😡😡😡")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is very required")]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Password & Confirm Password must match")]

    [NotMapped] 
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPW { get; set; }

    [Required(ErrorMessage = "Enter The Telephone number please ")]
    public string Telephone { get; set; } 

    public int AddressId { get; set; }
    public Address Address { get; set; }

    public List<Request> Requests { get; set; } = new List<Request>();




    public void ChangePassword(string oldPassword, string newPassword)
    {
        // Verify that the old password matches the current hashed password
        PasswordHasher<User> hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(this, Password, oldPassword);

        if (result != PasswordVerificationResult.Success)
        {
            throw new ArgumentException("Invalid old password.");
        }

        // Hash the new password
        Password = hasher.HashPassword(this, newPassword);
    }



}
