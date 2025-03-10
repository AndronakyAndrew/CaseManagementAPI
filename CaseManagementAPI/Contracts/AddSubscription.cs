namespace CaseManagementAPI.Contracts
{
    public class AddSubscription
    {
        public string Plan { get; set; } //'Basic', 'Premium', 'Enterprise'

        public PaymentData PaymentData { get; set; }
    }
}
