using System.ComponentModel.DataAnnotations;

namespace LoginPage.Models
{
    public class User
    {
        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; }

        [Key]
        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Password Must be at least 4 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Password Must be at least 4 characters.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password & Confirm Password do not Match.")]
        public string ConfirmPassword { get; set; }

        //[Display(Name = "Password")]
        //public string? MaskedPassword => Password != null ? new string('*', Password.Length) : null;

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiration { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndDate { get; set; }
        public string? UnlockAccountToken { get; set; }
        public DateTime? UnlockAccountTokenExpiration { get; set; }
        public bool ChangePassword { get; set; } = false;

    }
}
