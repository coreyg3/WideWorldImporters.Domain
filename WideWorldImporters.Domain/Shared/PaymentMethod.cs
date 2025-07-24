using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents ways that payments can be made (i.e: cash, check, EFT, etc.)
    /// </summary>
    public class PaymentMethod
    {
        /// <summary>
        /// Numeric ID used for reference to a payment type within the database
        /// </summary>
        public int PaymentMethodId { get; private set; }

        /// <summary>
        /// Full name of ways that customers can make payments or that suppliers can be paid
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PaymentMethodName { get; private set; } = string.Empty;

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// System-versioned temporal table start date
        /// </summary>
        public DateTime ValidFrom { get; private set; }

        /// <summary>
        /// System-versioned temporal table end date
        /// </summary>
        public DateTime ValidTo { get; private set; }

        // Private parameterless constructor for EF Core
        private PaymentMethod() { }

        /// <summary>
        /// Creates a new payment method
        /// </summary>
        /// <param name="paymentMethodName">The payment method name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this payment method</param>
        /// <exception cref="ArgumentException">Thrown when payment method name is invalid</exception>
        public PaymentMethod(string paymentMethodName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(paymentMethodName))
                throw new ArgumentException("Payment method name cannot be null or empty.", nameof(paymentMethodName));
            
            if (paymentMethodName.Length > 50)
                throw new ArgumentException("Payment method name cannot exceed 50 characters.", nameof(paymentMethodName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            PaymentMethodName = paymentMethodName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the payment method name
        /// </summary>
        /// <param name="newPaymentMethodName">The new payment method name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when payment method name is invalid</exception>
        public void UpdatePaymentMethodName(string newPaymentMethodName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newPaymentMethodName))
                throw new ArgumentException("Payment method name cannot be null or empty.", nameof(newPaymentMethodName));
            
            if (newPaymentMethodName.Length > 50)
                throw new ArgumentException("Payment method name cannot exceed 50 characters.", nameof(newPaymentMethodName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            PaymentMethodName = newPaymentMethodName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Sets the temporal table properties (typically called by infrastructure layer)
        /// </summary>
        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (PaymentMethodId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            PaymentMethodId = id;
        }

        public override string ToString()
        {
            return $"PaymentMethod: {PaymentMethodName} (ID: {PaymentMethodId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is PaymentMethod other && PaymentMethodId == other.PaymentMethodId;
        }

        public override int GetHashCode()
        {
            return PaymentMethodId.GetHashCode();
        }
    }
} 