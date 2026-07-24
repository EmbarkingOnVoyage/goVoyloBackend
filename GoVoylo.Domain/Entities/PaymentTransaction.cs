namespace GoVoylo.Domain.Entities
{
    public class PaymentTransaction
    {
        // Properties are publicly readable, but can only be modified inside the Domain rules
        public Guid Id { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public string Status { get; private set; }

         public string ReferenceNumber { get; private set; } 
        public string SourceClient { get; private set; } // Tracks: "Web", "Mobile", or "PythonAgent"
        public DateTime CreatedAt { get; private set; }

        // Constructor guarantees a valid object state upon creation
        public PaymentTransaction(Guid id, decimal amount, string currency, string sourceClient)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment Amount must be greater than zero.");
            if (string.IsNullOrEmpty(currency))
                throw new ArgumentException("Currency code must be provided.");

            Id = id;
            Amount = amount;
            Currency = currency.ToUpper();
            Status = "Pending";
            SourceClient = sourceClient;
            CreatedAt = DateTime.UtcNow;
            ReferenceNumber = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}"; 
        }
        // Explicit business action method (Domain behavior)
        public void MarkAsCompleted()
        {
            Status = "Success";
        }
        public void MarkAsFailed()
        {
            Status = "Failed";
        }
        // 2. Expose a public factory method that handles Guid generation internally
        public static PaymentTransaction Create(decimal amount, string currency, string sourceClient)
        {
            // Your defensive invariant checks stay here
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");

            return new PaymentTransaction(Guid.NewGuid(), amount, currency, sourceClient);
        }
    }
}