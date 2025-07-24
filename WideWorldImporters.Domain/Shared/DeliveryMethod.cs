using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents ways that stock items can be delivered (i.e: truck/van, post, pickup, courier, etc.)
    /// </summary>
    public class DeliveryMethod
    {
        /// <summary>
        /// Numeric ID used for reference to a delivery method within the database
        /// </summary>
        public int DeliveryMethodId { get; private set; }

        /// <summary>
        /// Full name of methods that can be used for delivery of customer orders
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string DeliveryMethodName { get; private set; } = string.Empty;

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
        private DeliveryMethod() { }

        /// <summary>
        /// Creates a new delivery method
        /// </summary>
        /// <param name="deliveryMethodName">The delivery method name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this delivery method</param>
        /// <exception cref="ArgumentException">Thrown when delivery method name is invalid</exception>
        public DeliveryMethod(string deliveryMethodName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(deliveryMethodName))
                throw new ArgumentException("Delivery method name cannot be null or empty.", nameof(deliveryMethodName));
            
            if (deliveryMethodName.Length > 50)
                throw new ArgumentException("Delivery method name cannot exceed 50 characters.", nameof(deliveryMethodName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            DeliveryMethodName = deliveryMethodName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the delivery method name
        /// </summary>
        /// <param name="newDeliveryMethodName">The new delivery method name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when delivery method name is invalid</exception>
        public void UpdateDeliveryMethodName(string newDeliveryMethodName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newDeliveryMethodName))
                throw new ArgumentException("Delivery method name cannot be null or empty.", nameof(newDeliveryMethodName));
            
            if (newDeliveryMethodName.Length > 50)
                throw new ArgumentException("Delivery method name cannot exceed 50 characters.", nameof(newDeliveryMethodName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            DeliveryMethodName = newDeliveryMethodName.Trim();
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
            if (DeliveryMethodId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            DeliveryMethodId = id;
        }

        public override string ToString()
        {
            return $"DeliveryMethod: {DeliveryMethodName} (ID: {DeliveryMethodId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is DeliveryMethod other && DeliveryMethodId == other.DeliveryMethodId;
        }

        public override int GetHashCode()
        {
            return DeliveryMethodId.GetHashCode();
        }
    }
} 