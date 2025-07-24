using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a customer invoice - a comprehensive business document for sales transactions
    /// This entity handles complex billing scenarios including head office billing, credit notes, and delivery tracking
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Numeric ID used for reference to an invoice within the database
        /// </summary>
        public int InvoiceId { get; private set; }

        /// <summary>
        /// Customer for this invoice
        /// </summary>
        public int CustomerId { get; private set; }

        /// <summary>
        /// Bill to customer for this invoice (invoices might be billed to a head office)
        /// </summary>
        public int BillToCustomerId { get; private set; }

        /// <summary>
        /// Sales order (if any) for this invoice
        /// </summary>
        public int? OrderId { get; private set; }

        /// <summary>
        /// How these stock items are being delivered
        /// </summary>
        public int DeliveryMethodId { get; private set; }

        /// <summary>
        /// Customer contact for this invoice
        /// </summary>
        public int ContactPersonId { get; private set; }

        /// <summary>
        /// Customer accounts contact for this invoice
        /// </summary>
        public int AccountsPersonId { get; private set; }

        /// <summary>
        /// Salesperson for this invoice
        /// </summary>
        public int SalespersonPersonId { get; private set; }

        /// <summary>
        /// Person who packed this shipment (or checked the packing)
        /// </summary>
        public int PackedByPersonId { get; private set; }

        /// <summary>
        /// Date that this invoice was raised
        /// </summary>
        public DateOnly InvoiceDate { get; private set; }

        /// <summary>
        /// Purchase Order Number received from customer
        /// </summary>
        [MaxLength(20)]
        public string? CustomerPurchaseOrderNumber { get; private set; }

        /// <summary>
        /// Is this a credit note (rather than an invoice)
        /// </summary>
        public bool IsCreditNote { get; private set; }

        /// <summary>
        /// Reason that this credit note needed to be generated (if applicable)
        /// </summary>
        public string? CreditNoteReason { get; private set; }

        /// <summary>
        /// Any comments related to this invoice (sent to customer)
        /// </summary>
        public string? Comments { get; private set; }

        /// <summary>
        /// Any comments related to delivery (sent to customer)
        /// </summary>
        public string? DeliveryInstructions { get; private set; }

        /// <summary>
        /// Any internal comments related to this invoice (not sent to the customer)
        /// </summary>
        public string? InternalComments { get; private set; }

        /// <summary>
        /// Total number of dry packages (information for the delivery driver)
        /// </summary>
        public int TotalDryItems { get; private set; }

        /// <summary>
        /// Total number of chiller packages (information for the delivery driver)
        /// </summary>
        public int TotalChillerItems { get; private set; }

        /// <summary>
        /// Delivery run for this shipment
        /// </summary>
        [MaxLength(5)]
        public string? DeliveryRun { get; private set; }

        /// <summary>
        /// Position in the delivery run for this shipment
        /// </summary>
        [MaxLength(5)]
        public string? RunPosition { get; private set; }

        /// <summary>
        /// Delivery confirmation data returned from delivery devices
        /// </summary>
        public string? ReturnedDeliveryData { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private Invoice() 
        {
        }

        /// <summary>
        /// Creates a new invoice
        /// </summary>
        /// <param name="customerId">Customer ID reference</param>
        /// <param name="billToCustomerId">Bill to customer ID reference</param>
        /// <param name="deliveryMethodId">Delivery method ID reference</param>
        /// <param name="contactPersonId">Customer contact person ID</param>
        /// <param name="accountsPersonId">Customer accounts person ID</param>
        /// <param name="salespersonPersonId">Salesperson ID</param>
        /// <param name="packedByPersonId">Person who packed the shipment</param>
        /// <param name="invoiceDate">Invoice date</param>
        /// <param name="totalDryItems">Total dry package count</param>
        /// <param name="totalChillerItems">Total chiller package count</param>
        /// <param name="lastEditedBy">ID of the person creating this invoice</param>
        /// <param name="orderId">Optional sales order reference</param>
        /// <param name="customerPurchaseOrderNumber">Optional customer PO number</param>
        /// <param name="comments">Optional customer comments</param>
        /// <param name="deliveryInstructions">Optional delivery instructions</param>
        /// <param name="internalComments">Optional internal comments</param>
        /// <param name="deliveryRun">Optional delivery run</param>
        /// <param name="runPosition">Optional run position</param>
        public Invoice(
            int customerId,
            int billToCustomerId,
            int deliveryMethodId,
            int contactPersonId,
            int accountsPersonId,
            int salespersonPersonId,
            int packedByPersonId,
            DateOnly invoiceDate,
            int totalDryItems,
            int totalChillerItems,
            int lastEditedBy,
            int? orderId = null,
            string? customerPurchaseOrderNumber = null,
            string? comments = null,
            string? deliveryInstructions = null,
            string? internalComments = null,
            string? deliveryRun = null,
            string? runPosition = null)
        {
            ValidateCustomerId(customerId);
            ValidateBillToCustomerId(billToCustomerId);
            ValidateDeliveryMethodId(deliveryMethodId);
            ValidateContactPersonId(contactPersonId);
            ValidateAccountsPersonId(accountsPersonId);
            ValidateSalespersonPersonId(salespersonPersonId);
            ValidatePackedByPersonId(packedByPersonId);
            ValidateInvoiceDate(invoiceDate);
            ValidateTotalItems(totalDryItems, nameof(totalDryItems));
            ValidateTotalItems(totalChillerItems, nameof(totalChillerItems));
            ValidateEditor(lastEditedBy);
            ValidateOrderId(orderId);
            ValidateCustomerPurchaseOrderNumber(customerPurchaseOrderNumber);
            ValidateDeliveryRun(deliveryRun);
            ValidateRunPosition(runPosition);

            CustomerId = customerId;
            BillToCustomerId = billToCustomerId;
            OrderId = orderId;
            DeliveryMethodId = deliveryMethodId;
            ContactPersonId = contactPersonId;
            AccountsPersonId = accountsPersonId;
            SalespersonPersonId = salespersonPersonId;
            PackedByPersonId = packedByPersonId;
            InvoiceDate = invoiceDate;
            CustomerPurchaseOrderNumber = string.IsNullOrWhiteSpace(customerPurchaseOrderNumber) ? null : customerPurchaseOrderNumber.Trim();
            IsCreditNote = false; // New invoices start as invoices, not credit notes
            CreditNoteReason = null;
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions.Trim();
            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            TotalDryItems = totalDryItems;
            TotalChillerItems = totalChillerItems;
            DeliveryRun = string.IsNullOrWhiteSpace(deliveryRun) ? null : deliveryRun.Trim().ToUpperInvariant();
            RunPosition = string.IsNullOrWhiteSpace(runPosition) ? null : runPosition.Trim().ToUpperInvariant();
            ReturnedDeliveryData = null;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a credit note based on this invoice
        /// </summary>
        /// <param name="creditNoteReason">Reason for the credit note</param>
        /// <param name="editedBy">ID of the person creating the credit note</param>
        /// <returns>New credit note invoice</returns>
        public Invoice CreateCreditNote(string creditNoteReason, int editedBy)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(creditNoteReason);
            ValidateEditor(editedBy);

            if (IsCreditNote)
                throw new InvalidOperationException("Cannot create a credit note from an existing credit note.");

            var creditNote = new Invoice(
                CustomerId,
                BillToCustomerId,
                DeliveryMethodId,
                ContactPersonId,
                AccountsPersonId,
                SalespersonPersonId,
                PackedByPersonId,
                DateOnly.FromDateTime(DateTime.Today), // Credit note gets today's date
                TotalDryItems,
                TotalChillerItems,
                editedBy,
                OrderId,
                CustomerPurchaseOrderNumber,
                Comments,
                DeliveryInstructions,
                InternalComments,
                DeliveryRun,
                RunPosition);

            // Set credit note properties
            creditNote.IsCreditNote = true;
            creditNote.CreditNoteReason = creditNoteReason.Trim();

            return creditNote;
        }

        /// <summary>
        /// Updates delivery information
        /// </summary>
        /// <param name="deliveryMethodId">New delivery method</param>
        /// <param name="deliveryInstructions">New delivery instructions</param>
        /// <param name="deliveryRun">New delivery run</param>
        /// <param name="runPosition">New run position</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDeliveryInformation(int deliveryMethodId, string? deliveryInstructions, string? deliveryRun, string? runPosition, int editedBy)
        {
            ValidateDeliveryMethodId(deliveryMethodId);
            ValidateDeliveryRun(deliveryRun);
            ValidateRunPosition(runPosition);
            ValidateEditor(editedBy);

            DeliveryMethodId = deliveryMethodId;
            DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions.Trim();
            DeliveryRun = string.IsNullOrWhiteSpace(deliveryRun) ? null : deliveryRun.Trim().ToUpperInvariant();
            RunPosition = string.IsNullOrWhiteSpace(runPosition) ? null : runPosition.Trim().ToUpperInvariant();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the package counts (for delivery driver information)
        /// </summary>
        /// <param name="totalDryItems">Total dry packages</param>
        /// <param name="totalChillerItems">Total chiller packages</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePackageCounts(int totalDryItems, int totalChillerItems, int editedBy)
        {
            ValidateTotalItems(totalDryItems, nameof(totalDryItems));
            ValidateTotalItems(totalChillerItems, nameof(totalChillerItems));
            ValidateEditor(editedBy);

            TotalDryItems = totalDryItems;
            TotalChillerItems = totalChillerItems;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates customer comments
        /// </summary>
        /// <param name="comments">New customer comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateComments(string? comments, int editedBy)
        {
            ValidateEditor(editedBy);

            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates internal comments
        /// </summary>
        /// <param name="internalComments">New internal comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateInternalComments(string? internalComments, int editedBy)
        {
            ValidateEditor(editedBy);

            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Records delivery confirmation data from delivery device
        /// </summary>
        /// <param name="deliveryData">Delivery confirmation data</param>
        /// <param name="editedBy">ID of the person recording the delivery</param>
        public void RecordDeliveryConfirmation(string returnedDeliveryData, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(returnedDeliveryData);
            ValidateEditor(editedBy);

            ReturnedDeliveryData = returnedDeliveryData;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (InvoiceId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            InvoiceId = id;
        }

        // Validation methods
        private static void ValidateCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be a valid reference.", nameof(customerId));
        }

        private static void ValidateBillToCustomerId(int billToCustomerId)
        {
            if (billToCustomerId <= 0)
                throw new ArgumentException("Bill to customer ID must be a valid reference.", nameof(billToCustomerId));
        }

        private static void ValidateDeliveryMethodId(int deliveryMethodId)
        {
            if (deliveryMethodId <= 0)
                throw new ArgumentException("Delivery method ID must be a valid reference.", nameof(deliveryMethodId));
        }

        private static void ValidateContactPersonId(int contactPersonId)
        {
            if (contactPersonId <= 0)
                throw new ArgumentException("Contact person ID must be a valid reference.", nameof(contactPersonId));
        }

        private static void ValidateAccountsPersonId(int accountsPersonId)
        {
            if (accountsPersonId <= 0)
                throw new ArgumentException("Accounts person ID must be a valid reference.", nameof(accountsPersonId));
        }

        private static void ValidateSalespersonPersonId(int salespersonPersonId)
        {
            if (salespersonPersonId <= 0)
                throw new ArgumentException("Salesperson person ID must be a valid reference.", nameof(salespersonPersonId));
        }

        private static void ValidatePackedByPersonId(int packedByPersonId)
        {
            if (packedByPersonId <= 0)
                throw new ArgumentException("Packed by person ID must be a valid reference.", nameof(packedByPersonId));
        }

        private static void ValidateInvoiceDate(DateOnly invoiceDate)
        {
            if (invoiceDate == default)
                throw new ArgumentException("Invoice date cannot be default value.", nameof(invoiceDate));
            
            if (invoiceDate > DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
                throw new ArgumentException("Invoice date cannot be in the future.", nameof(invoiceDate));
        }

        private static void ValidateOrderId(int? orderId)
        {
            if (orderId.HasValue && orderId.Value <= 0)
                throw new ArgumentException("Order ID must be a valid reference when provided.", nameof(orderId));
        }

        private static void ValidateCustomerPurchaseOrderNumber(string? customerPurchaseOrderNumber)
        {
            if (!string.IsNullOrEmpty(customerPurchaseOrderNumber) && customerPurchaseOrderNumber.Length > 20)
                throw new ArgumentException("Customer purchase order number cannot exceed 20 characters.", nameof(customerPurchaseOrderNumber));
        }

        private static void ValidateTotalItems(int totalItems, string parameterName)
        {
            if (totalItems < 0)
                throw new ArgumentException("Total items cannot be negative.", parameterName);
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
            var typeDescription = IsCreditNote ? "Credit Note" : "Invoice";
            var orderRef = OrderId.HasValue ? $" (Order {OrderId})" : "";
            return $"{typeDescription} {InvoiceId}: Customer {CustomerId}{orderRef}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Invoice other && InvoiceId != 0 && InvoiceId == other.InvoiceId;
        }

        public override int GetHashCode()
        {
            return InvoiceId.GetHashCode();
        }
    }
} 