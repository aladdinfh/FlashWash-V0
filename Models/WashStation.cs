using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace FlashWash.Models
{
    public class WashStation
    {
        [Key]
        public int WashStationId { get; set; }

        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The AddressId field is required.")]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "The Address field is required.")]
        public Address Address { get; set; }

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Password field is required.")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPW { get; set; }

        [Required(ErrorMessage = "The OwnerName field is required.")]
        public string OwnerName { get; set; }

        [Required(ErrorMessage = "The MorningStartTime field is required.")]
        public TimeSpan MorningStartTime { get; set; }

        [Required(ErrorMessage = "The MorningEndTime field is required.")]
        public TimeSpan MorningEndTime { get; set; }

        [Required(ErrorMessage = "The EveningStartTime field is required.")]
        public TimeSpan EveningStartTime { get; set; }

        [Required(ErrorMessage = "The EveningEndTime field is required.")]
        public TimeSpan EveningEndTime { get; set; }

        [Required(ErrorMessage = "The AcceptCreditCard field is required.")]
        public bool AcceptCreditCard { get; set; }

        [Required(ErrorMessage = "The AcceptCash field is required.")]
        public bool AcceptCash { get; set; }

        [Required(ErrorMessage = "The AcceptMobilePayment field is required.")]
        public bool AcceptMobilePayment { get; set; }

        [Required(ErrorMessage = "The ReservedTimes field is required.")]
        public List<int> ReservedTimes { get; set; } = new List<int>();

        [Required(ErrorMessage = "The Telephone field is required.")]
        public string Telephone { get; set; }

        public List<Offer> Offers { get; set; } = new List<Offer>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public void ChangePassword(string oldPassword, string newPassword)
        {
            // Verify that the old password matches the current hashed password
            PasswordHasher<WashStation> hasher = new PasswordHasher<WashStation>();
            var result = hasher.VerifyHashedPassword(this, Password, oldPassword);

            if (result != PasswordVerificationResult.Success)
            {
                throw new ArgumentException("Invalid old password.");
            }

            // Hash the new password
            Password = hasher.HashPassword(this, newPassword);
        }
    }
}
