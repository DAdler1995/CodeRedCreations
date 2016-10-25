using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
