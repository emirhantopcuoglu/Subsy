using System.ComponentModel.DataAnnotations;

namespace Subsy.Web.Models;

public sealed class LoginWith2faViewModel
{
    [Required(ErrorMessage = "Doğrulama kodu gereklidir.")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "6 haneli kodu girin.")]
    [DataType(DataType.Text)]
    public string TwoFactorCode { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
