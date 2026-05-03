using System.ComponentModel.DataAnnotations;

namespace Subsy.Web.Models;

public sealed class ResetPasswordViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni parola gereklidir.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola en az 8 karakter olmalıdır.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola onayı gereklidir.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Parolalar eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
