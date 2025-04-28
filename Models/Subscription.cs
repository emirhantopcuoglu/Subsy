using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Subsy.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string RenewalPeriod { get; set; }
        public DateTime RenewalDate { get; set; }

        [BindNever]
        public string UserId { get; set; }
    }
}
