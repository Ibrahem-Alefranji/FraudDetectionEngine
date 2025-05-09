namespace FraudDetectionApi.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string BusinessName { get; set; }
        public string Url { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
