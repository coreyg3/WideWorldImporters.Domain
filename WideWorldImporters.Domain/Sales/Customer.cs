using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Main entity for customers (organizations or individuals)
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Numeric ID used for reference to a customer within the database
        /// </summary>
        public int CustomerId { get; private set; }

        /// <summary>
        /// Customer's full name (usually a trading name)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; private set; } = string.Empty;

        /// <summary>
        /// Customer that this is billed to (usually the same customer but can be another parent company)
        /// </summary>
        public int BillToCustomerId { get; private set; }

        /// <summary>
        /// Customer's category
        /// </summary>
        public int CustomerCategoryId { get; private set; }

        /// <summary>
        /// Customer's buying group (optional)
        /// </summary>
        public int? BuyingGroupId { get; private set; }

        /// <summary>
        /// Primary contact person ID
        /// </summary>
        public int PrimaryContactPersonId { get; private set; }

        /// <summary>
        /// Alternate contact person ID (optional)
        /// </summary>
        public int? AlternateContactPersonId { get; private set; }

        /// <summary>
        /// Standard delivery method for stock items sent to this customer
        /// </summary>
        public int DeliveryMethodId { get; private set; }

        /// <summary>
        /// Delivery address for the customer
        /// </summary>
        public Address DeliveryAddress { get; private set; }

        /// <summary>
        /// Postal address for the customer
        /// </summary>
        public Address PostalAddress { get; private set; }

        /// <summary>
        /// Geographic location for the customer's office/warehouse (stored as binary geography data)
        /// </summary>
        public byte[]? DeliveryLocation { get; private set; }

        /// <summary>
        /// Credit limit for this customer (NULL if unlimited)
        /// </summary>
        public decimal? CreditLimit { get; private set; }

        /// <summary>
        /// Date this customer account was opened
        /// </summary>
        public DateOnly AccountOpenedDate { get; private set; }

        /// <summary>
        /// Standard discount offered to this customer
        /// </summary>
        public decimal StandardDiscountPercentage { get; private set; }

        /// <summary>
        /// Is a statement sent to this customer? (Or do they just pay on each invoice?)
        /// </summary>
        public bool IsStatementSent { get; private set; }

        /// <summary>
        /// Is this customer on credit hold? (Prevents further deliveries to this customer)
        /// </summary>
        public bool IsOnCreditHold { get; private set; }

        /// <summary>
        /// Number of days for payment of an invoice (payment terms)
        /// </summary>
        public int PaymentDays { get; private set; }

        /// <summary>
        /// Contact information (phone, fax, website)
        /// </summary>
        public ContactInformation ContactInformation { get; private set; }

        /// <summary>
        /// Normal delivery run for this customer (optional)
        /// </summary>
        [MaxLength(5)]
        public string? DeliveryRun { get; private set; }

        /// <summary>
        /// Normal position in the delivery run for this customer (optional)
        /// </summary>
        [MaxLength(5)]
        public string? RunPosition { get; private set; }

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
        private Customer() 
        {
            DeliveryAddress = null!;
            PostalAddress = null!;
            ContactInformation = null!;
        }

        /// <summary>
        /// Creates a new customer
        /// </summary>
        /// <param name="customerName">Customer's full name (required, max 100 characters)</param>
        /// <param name="billToCustomerId">Customer ID that this customer is billed to</param>
        /// <param name="customerCategoryId">Customer's category ID</param>
        /// <param name="primaryContactPersonId">Primary contact person ID</param>
        /// <param name="deliveryMethodId">Delivery method ID</param>
        /// <param name="deliveryAddress">Delivery address</param>
        /// <param name="postalAddress">Postal address</param>
        /// <param name="contactInformation">Contact information</param>
        /// <param name="accountOpenedDate">Date the account was opened</param>
        /// <param name="standardDiscountPercentage">Standard discount percentage</param>
        /// <param name="paymentDays">Payment terms in days</param>
        /// <param name="lastEditedBy">ID of the person creating this customer</param>
        /// <param name="buyingGroupId">Optional buying group ID</param>
        /// <param name="alternateContactPersonId">Optional alternate contact person ID</param>
        /// <param name="creditLimit">Optional credit limit</param>
        /// <param name="isStatementSent">Whether statements are sent</param>
        /// <param name="isOnCreditHold">Whether customer is on credit hold</param>
        /// <param name="deliveryRun">Optional delivery run</param>
        /// <param name="runPosition">Optional run position</param>
        /// <param name="deliveryLocation">Optional geographic location</param>
        public Customer(
            string customerName,
            int billToCustomerId,
            int customerCategoryId,
            int primaryContactPersonId,
            int deliveryMethodId,
            Address deliveryAddress,
            Address postalAddress,
            ContactInformation contactInformation,
            DateOnly accountOpenedDate,
            decimal standardDiscountPercentage,
            int paymentDays,
            int lastEditedBy,
            int? buyingGroupId = null,
            int? alternateContactPersonId = null,
            decimal? creditLimit = null,
            bool isStatementSent = true,
            bool isOnCreditHold = false,
            string? deliveryRun = null,
            string? runPosition = null,
            byte[]? deliveryLocation = null)
        {
            ValidateCustomerName(customerName);
            ValidateDiscountPercentage(standardDiscountPercentage);
            ValidatePaymentDays(paymentDays);
            ValidateCreditLimit(creditLimit);
            ValidateDeliveryRun(deliveryRun);
            ValidateRunPosition(runPosition);

            if (billToCustomerId <= 0)
                throw new ArgumentException("Bill to customer ID must be a valid customer reference.", nameof(billToCustomerId));
            
            if (customerCategoryId <= 0)
                throw new ArgumentException("Customer category ID must be a valid reference.", nameof(customerCategoryId));
                
            if (primaryContactPersonId <= 0)
                throw new ArgumentException("Primary contact person ID must be a valid reference.", nameof(primaryContactPersonId));
                
            if (deliveryMethodId <= 0)
                throw new ArgumentException("Delivery method ID must be a valid reference.", nameof(deliveryMethodId));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            CustomerName = customerName.Trim();
            BillToCustomerId = billToCustomerId;
            CustomerCategoryId = customerCategoryId;
            BuyingGroupId = buyingGroupId;
            PrimaryContactPersonId = primaryContactPersonId;
            AlternateContactPersonId = alternateContactPersonId;
            DeliveryMethodId = deliveryMethodId;
            DeliveryAddress = deliveryAddress ?? throw new ArgumentNullException(nameof(deliveryAddress));
            PostalAddress = postalAddress ?? throw new ArgumentNullException(nameof(postalAddress));
            ContactInformation = contactInformation ?? throw new ArgumentNullException(nameof(contactInformation));
            CreditLimit = creditLimit;
            AccountOpenedDate = accountOpenedDate;
            StandardDiscountPercentage = standardDiscountPercentage;
            IsStatementSent = isStatementSent;
            IsOnCreditHold = isOnCreditHold;
            PaymentDays = paymentDays;
            DeliveryRun = string.IsNullOrWhiteSpace(deliveryRun) ? null : deliveryRun.Trim();
            RunPosition = string.IsNullOrWhiteSpace(runPosition) ? null : runPosition.Trim();
            DeliveryLocation = deliveryLocation;
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the customer name
        /// </summary>
        public void UpdateCustomerName(string newCustomerName, int editedBy)
        {
            ValidateCustomerName(newCustomerName);
            ValidateEditor(editedBy);

            CustomerName = newCustomerName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates contact information
        /// </summary>
        public void UpdateContactInformation(ContactInformation newContactInformation, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newContactInformation);
            ValidateEditor(editedBy);

            ContactInformation = newContactInformation;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates delivery address
        /// </summary>
        public void UpdateDeliveryAddress(Address newDeliveryAddress, int editedBy, byte[]? newDeliveryLocation = null)
        {
            ArgumentNullException.ThrowIfNull(newDeliveryAddress);
            ValidateEditor(editedBy);

            DeliveryAddress = newDeliveryAddress;
            DeliveryLocation = newDeliveryLocation;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates postal address
        /// </summary>
        public void UpdatePostalAddress(Address newPostalAddress, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newPostalAddress);
            ValidateEditor(editedBy);

            PostalAddress = newPostalAddress;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Sets the credit limit
        /// </summary>
        public void SetCreditLimit(decimal? creditLimit, int editedBy)
        {
            ValidateCreditLimit(creditLimit);
            ValidateEditor(editedBy);

            CreditLimit = creditLimit;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the standard discount percentage
        /// </summary>
        public void UpdateStandardDiscount(decimal discountPercentage, int editedBy)
        {
            ValidateDiscountPercentage(discountPercentage);
            ValidateEditor(editedBy);

            StandardDiscountPercentage = discountPercentage;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Places customer on credit hold
        /// </summary>
        public void PlaceOnCreditHold(int editedBy)
        {
            ValidateEditor(editedBy);

            if (IsOnCreditHold)
                throw new InvalidOperationException("Customer is already on credit hold.");

            IsOnCreditHold = true;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Removes customer from credit hold
        /// </summary>
        public void RemoveFromCreditHold(int editedBy)
        {
            ValidateEditor(editedBy);

            if (!IsOnCreditHold)
                throw new InvalidOperationException("Customer is not on credit hold.");

            IsOnCreditHold = false;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates payment terms
        /// </summary>
        public void UpdatePaymentTerms(int paymentDays, int editedBy)
        {
            ValidatePaymentDays(paymentDays);
            ValidateEditor(editedBy);

            PaymentDays = paymentDays;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates delivery logistics
        /// </summary>
        public void UpdateDeliveryLogistics(int deliveryMethodId, string? deliveryRun, string? runPosition, int editedBy)
        {
            if (deliveryMethodId <= 0)
                throw new ArgumentException("Delivery method ID must be a valid reference.", nameof(deliveryMethodId));
                
            ValidateDeliveryRun(deliveryRun);
            ValidateRunPosition(runPosition);
            ValidateEditor(editedBy);

            DeliveryMethodId = deliveryMethodId;
            DeliveryRun = string.IsNullOrWhiteSpace(deliveryRun) ? null : deliveryRun.Trim();
            RunPosition = string.IsNullOrWhiteSpace(runPosition) ? null : runPosition.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates statement preference
        /// </summary>
        public void UpdateStatementPreference(bool isStatementSent, int editedBy)
        {
            ValidateEditor(editedBy);

            IsStatementSent = isStatementSent;
            LastEditedBy = editedBy;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (CustomerId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            CustomerId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        // Validation methods
        private static void ValidateCustomerName(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name cannot be null or empty.", nameof(customerName));
            
            if (customerName.Length > 100)
                throw new ArgumentException("Customer name cannot exceed 100 characters.", nameof(customerName));
        }

        private static void ValidateDiscountPercentage(decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));
        }

        private static void ValidatePaymentDays(int paymentDays)
        {
            if (paymentDays < 0)
                throw new ArgumentException("Payment days cannot be negative.", nameof(paymentDays));
        }

        private static void ValidateCreditLimit(decimal? creditLimit)
        {
            if (creditLimit.HasValue && creditLimit.Value < 0)
                throw new ArgumentException("Credit limit cannot be negative.", nameof(creditLimit));
        }

        private static void ValidateDeliveryRun(string? deliveryRun)
        {
            if (!string.IsNullOrEmpty(deliveryRun) && deliveryRun.Length > 5)
                throw new ArgumentException("Delivery run cannot exceed 5 characters.", nameof(deliveryRun));
        }

        private static void ValidateRunPosition(string? runPosition)
        {
            if (!string.IsNullOrEmpty(runPosition) && runPosition.Length > 5)
                throw new ArgumentException("Run position cannot exceed 5 characters.", nameof(runPosition));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"Customer: {CustomerName} (ID: {CustomerId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Customer other && CustomerId == other.CustomerId;
        }

        public override int GetHashCode()
        {
            return CustomerId.GetHashCode();
        }
    }
}
