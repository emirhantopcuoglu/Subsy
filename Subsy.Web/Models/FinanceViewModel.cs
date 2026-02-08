namespace Subsy.Web.Models
{
    public class FinanceViewModel
    {
        public decimal TotalMonthlyCost { get; set; }
        public decimal AllTimeSpending { get; set; }
        public List<ServiceSummary> GroupedByService { get; set; }
        public ServiceSummary TopSpendingService { get; set; }
        public int SubscriptionCount { get; set; }
    }

    public class ServiceSummary
    {
        public string SubscriptionName { get; set; }
        public decimal TotalCost { get; set; }
    }
}