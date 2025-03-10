namespace CaseManagementAPI.Contracts
{
    public class PaymentData
    {
        public int CardNumber { get; set; }
        public int ExpiryDate { get; set; }
        public int CVV { get; set; }
        public string CardHolderName { get; set; }
    }
}
