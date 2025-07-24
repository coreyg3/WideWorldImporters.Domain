using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Purchasing
{
    /// <summary>
    /// Main entity table for suppliers (organizations)
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Numeric ID used for reference to a supplier within the database
        /// </summary>
        public int SupplierId { get; private set; }

        /// <summary>
        /// Supplier's full name (usually a trading name)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SupplierName { get; private set; } = string.Empty;

        /// <summary>
        /// Supplier's category
        /// </summary>
        public int SupplierCategoryId { get; private set; }

        /// <summary>
        /// Primary contact person ID
        /// </summary>
        public int PrimaryContactPersonId { get; private set; }

        /// <summary>
        /// Alternate contact person ID (required for suppliers)
        /// </summary>
        public int AlternateContactPersonId { get; private set; }

        /// <summary>
        /// Standard delivery method for stock items received from this supplier (optional)
        /// </summary>
        public int? DeliveryMethodId { get; private set; }

        /// <summary>
        /// Delivery address for the supplier
        /// </summary>
        public Address DeliveryAddress { get; private set; }

        /// <summary>
        /// Postal address for the supplier
        /// </summary>
        public Address PostalAddress { get; private set; }

        /// <summary>
        /// Geographic location for the supplier's office/warehouse
        /// </summary>
        public byte[]? DeliveryLocation { get; private set; }

        /// <summary>
        /// Supplier reference for our organization (might be our account number at the supplier)
        /// </summary>
        [MaxLength(20)]
        public string? SupplierReference { get; private set; }

        /// <summary>
        /// Bank account information for payments
        /// </summary>
        public BankAccount BankAccount { get; private set; }

        /// <summary>
        /// Number of days for payment of an invoice (payment terms)
        /// </summary>
        public int PaymentDays { get; private set; }

        /// <summary>
        /// Internal comments (not exposed outside organization)
        /// </summary>
        public string? InternalComments { get; private set; }

        /// <summary>
        /// Contact information (phone, fax, website)
        /// </summary>
        public ContactInformation ContactInformation { get; private set; }

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
        private Supplier() 
        {
            DeliveryAddress = null!;
            PostalAddress = null!;
            ContactInformation = null!;
            BankAccount = null!;
        }

        /// <summary>
        /// Creates a new supplier
        /// </summary>
        /// <param name="supplierName">Supplier's full name (required, max 100 characters)</param>
        /// <param name="supplierCategoryId">Supplier's category ID</param>
        /// <param name="primaryContactPersonId">Primary contact person ID</param>
        /// <param name="alternateContactPersonId">Alternate contact person ID (required)</param>
        /// <param name="deliveryAddress">Delivery address</param>
        /// <param name="postalAddress">Postal address</param>
        /// <param name="contactInformation">Contact information</param>
        /// <param name="paymentDays">Payment terms in days</param>
        /// <param name="lastEditedBy">ID of the person creating this supplier</param>
        /// <param name="deliveryMethodId">Optional delivery method ID</param>
        /// <param name="supplierReference">Optional supplier reference</param>
        /// <param name="bankAccount">Optional bank account information</param>
        /// <param name="internalComments">Optional internal comments</param>
        /// <param name="deliveryLocation">Optional geographic location</param>
        public Supplier(
            string supplierName,
            int supplierCategoryId,
            int primaryContactPersonId,
            int alternateContactPersonId,
            Address deliveryAddress,
            Address postalAddress,
            ContactInformation contactInformation,
            int paymentDays,
            int lastEditedBy,
            int? deliveryMethodId = null,
            string? supplierReference = null,
            BankAccount? bankAccount = null,
            string? internalComments = null,
            byte[]? deliveryLocation = null)
        {
            ValidateSupplierName(supplierName);
            ValidatePaymentDays(paymentDays);
            ValidateSupplierReference(supplierReference);
            ValidateContactPersonIds(primaryContactPersonId, alternateContactPersonId);
            ValidateEditor(lastEditedBy);

            if (supplierCategoryId <= 0)
                throw new ArgumentException("Supplier category ID must be a valid reference.", nameof(supplierCategoryId));

            SupplierName = supplierName.Trim();
            SupplierCategoryId = supplierCategoryId;
            PrimaryContactPersonId = primaryContactPersonId;
            AlternateContactPersonId = alternateContactPersonId;
            DeliveryMethodId = deliveryMethodId;
            DeliveryAddress = deliveryAddress ?? throw new ArgumentNullException(nameof(deliveryAddress));
            PostalAddress = postalAddress ?? throw new ArgumentNullException(nameof(postalAddress));
            ContactInformation = contactInformation ?? throw new ArgumentNullException(nameof(contactInformation));
            PaymentDays = paymentDays;
            SupplierReference = string.IsNullOrWhiteSpace(supplierReference) ? null : supplierReference.Trim();
            BankAccount = bankAccount ?? new BankAccount();
            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            DeliveryLocation = deliveryLocation;
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the supplier name
        /// </summary>
        /// <param name="newSupplierName">New supplier name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateSupplierName(string newSupplierName, int editedBy)
        {
            ValidateSupplierName(newSupplierName);
            ValidateEditor(editedBy);

            SupplierName = newSupplierName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates contact information
        /// </summary>
        /// <param name="newContactInformation">New contact information</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateContactInformation(ContactInformation newContactInformation, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newContactInformation);
            ValidateEditor(editedBy);

            ContactInformation = newContactInformation;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates delivery address and location
        /// </summary>
        /// <param name="newDeliveryAddress">New delivery address</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <param name="newDeliveryLocation">New geographic location</param>
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
        /// <param name="newPostalAddress">New postal address</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePostalAddress(Address newPostalAddress, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newPostalAddress);
            ValidateEditor(editedBy);

            PostalAddress = newPostalAddress;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates contact persons
        /// </summary>
        /// <param name="primaryContactPersonId">Primary contact person ID</param>
        /// <param name="alternateContactPersonId">Alternate contact person ID</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateContactPersons(int primaryContactPersonId, int alternateContactPersonId, int editedBy)
        {
            ValidateContactPersonIds(primaryContactPersonId, alternateContactPersonId);
            ValidateEditor(editedBy);

            PrimaryContactPersonId = primaryContactPersonId;
            AlternateContactPersonId = alternateContactPersonId;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates payment terms
        /// </summary>
        /// <param name="paymentDays">Payment terms in days</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePaymentTerms(int paymentDays, int editedBy)
        {
            ValidatePaymentDays(paymentDays);
            ValidateEditor(editedBy);

            PaymentDays = paymentDays;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates bank account information
        /// </summary>
        /// <param name="newBankAccount">New bank account information</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateBankAccount(BankAccount newBankAccount, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newBankAccount);
            ValidateEditor(editedBy);

            BankAccount = newBankAccount;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates supplier reference
        /// </summary>
        /// <param name="newSupplierReference">New supplier reference</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateSupplierReference(string? newSupplierReference, int editedBy)
        {
            ValidateSupplierReference(newSupplierReference);
            ValidateEditor(editedBy);

            SupplierReference = string.IsNullOrWhiteSpace(newSupplierReference) ? null : newSupplierReference.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates internal comments
        /// </summary>
        /// <param name="newInternalComments">New internal comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateInternalComments(string? newInternalComments, int editedBy)
        {
            ValidateEditor(editedBy);

            InternalComments = string.IsNullOrWhiteSpace(newInternalComments) ? null : newInternalComments.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates delivery method
        /// </summary>
        /// <param name="deliveryMethodId">Delivery method ID (optional)</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDeliveryMethod(int? deliveryMethodId, int editedBy)
        {
            ValidateEditor(editedBy);

            DeliveryMethodId = deliveryMethodId;
            LastEditedBy = editedBy;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (SupplierId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            SupplierId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        // Validation methods
        private static void ValidateSupplierName(string supplierName)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                throw new ArgumentException("Supplier name cannot be null or empty.", nameof(supplierName));
            
            if (supplierName.Length > 100)
                throw new ArgumentException("Supplier name cannot exceed 100 characters.", nameof(supplierName));
        }

        private static void ValidatePaymentDays(int paymentDays)
        {
            if (paymentDays < 0)
                throw new ArgumentException("Payment days cannot be negative.", nameof(paymentDays));
        }

        private static void ValidateSupplierReference(string? supplierReference)
        {
            if (!string.IsNullOrEmpty(supplierReference) && supplierReference.Length > 20)
                throw new ArgumentException("Supplier reference cannot exceed 20 characters.", nameof(supplierReference));
        }

        private static void ValidateContactPersonIds(int primaryContactPersonId, int alternateContactPersonId)
        {
            if (primaryContactPersonId <= 0)
                throw new ArgumentException("Primary contact person ID must be a valid reference.", nameof(primaryContactPersonId));
                
            if (alternateContactPersonId <= 0)
                throw new ArgumentException("Alternate contact person ID must be a valid reference.", nameof(alternateContactPersonId));

            if (primaryContactPersonId == alternateContactPersonId)
                throw new ArgumentException("Primary and alternate contact persons must be different.", nameof(alternateContactPersonId));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"Supplier: {SupplierName} (ID: {SupplierId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Supplier other && SupplierId == other.SupplierId;
        }

        public override int GetHashCode()
        {
            return SupplierId.GetHashCode();
        }
    }
} 