using System.ComponentModel.DataAnnotations;

namespace Subsy.Web.Models;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta adresi gereklidir.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
