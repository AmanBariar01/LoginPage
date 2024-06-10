using System.ComponentModel.DataAnnotations;

namespace LoginPage.Models.ViewModels
{
    public class PasswordViewModel
    {
        [Required]
        public string ResetToken { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Password Must be of at least 4 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Password & Confirm Password do not Match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }


    }
}
