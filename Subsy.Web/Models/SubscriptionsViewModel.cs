using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Subsy.Web.Models;

public class SubscriptionsViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Abonelik adı boş olamaz.")]
    [StringLength(100, ErrorMessage = "Abonelik adı 100 karakterden fazla olamaz.")]
    public string Name { get; set; } = default!;

    [Range(0, 1500, ErrorMessage = "Geçersiz abonelik ücret değeri.")]
    public decimal Price { get; set; }

    [Range(7, 365, ErrorMessage = "Geçersiz yenileme sıklığı değeri.")]
    [Display(Name = "Yenileme Sıklığı (Gün)")]
    public string RenewalPeriod { get; set; } = default!;

    [DataType(DataType.Date)]
    [Display(Name = "Yenileme Tarihi")]
    public DateTime RenewalDate { get; set; }

    [BindNever]
    public string? UserId { get; set; }

    public bool IsArchived { get; set; } = false;
}
