using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Purchasing
{
    /// <summary>
    /// Details of supplier purchase orders
    /// </summary>
    public class PurchaseOrder
    {
        /// <summary>
        /// Numeric ID used for reference to a purchase order within the database
        /// </summary>
        public int PurchaseOrderId { get; private set; }

        /// <summary>
        /// Supplier for this purchase order
        /// </summary>
        public int SupplierId { get; private set; }

        /// <summary>
        /// Date that this purchase order was raised
        /// </summary>
        public DateOnly OrderDate { get; private set; }

        /// <summary>
        /// How this purchase order should be delivered
        /// </summary>
        public int DeliveryMethodId { get; private set; }

        /// <summary>
        /// The person who is the primary contact for this purchase order
        /// </summary>
        public int ContactPersonId { get; private set; }

        /// <summary>
        /// Expected delivery date for this purchase order
        /// </summary>
        public DateOnly? ExpectedDeliveryDate { get; private set; }

        /// <summary>
        /// Supplier reference for our organization (might be our account number at the supplier)
        /// </summary>
        [MaxLength(20)]
        public string? SupplierReference { get; private set; }

        /// <summary>
        /// Is this purchase order now considered finalized?
        /// </summary>
        public bool IsOrderFinalized { get; private set; }

        /// <summary>
        /// Any comments related this purchase order (comments sent to the supplier)
        /// </summary>
        public string? Comments { get; private set; }

        /// <summary>
        /// Any internal comments related this purchase order (comments for internal reference only and not sent to the supplier)
        /// </summary>
        public string? InternalComments { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private PurchaseOrder() { }

        /// <summary>
        /// Creates a new purchase order
        /// </summary>
        /// <param name="supplierId">Supplier ID reference</param>
        /// <param name="orderDate">Date the purchase order was raised</param>
        /// <param name="deliveryMethodId">Delivery method ID reference</param>
        /// <param name="contactPersonId">Primary contact person ID reference</param>
        /// <param name="lastEditedBy">ID of the person creating this purchase order</param>
        /// <param name="expectedDeliveryDate">Expected delivery date (optional)</param>
        /// <param name="supplierReference">Supplier reference (optional, max 20 characters)</param>
        /// <param name="comments">External comments (optional)</param>
        /// <param name="internalComments">Internal comments (optional)</param>
        public PurchaseOrder(
            int supplierId,
            DateOnly orderDate,
            int deliveryMethodId,
            int contactPersonId,
            int lastEditedBy,
            DateOnly? expectedDeliveryDate = null,
            string? supplierReference = null,
            string? comments = null,
            string? internalComments = null)
        {
            ValidateSupplierId(supplierId);
            ValidateOrderDate(orderDate);
            ValidateDeliveryMethodId(deliveryMethodId);
            ValidateContactPersonId(contactPersonId);
            ValidateEditor(lastEditedBy);
            ValidateSupplierReference(supplierReference);
            ValidateExpectedDeliveryDate(expectedDeliveryDate, orderDate);

            SupplierId = supplierId;
            OrderDate = orderDate;
            DeliveryMethodId = deliveryMethodId;
            ContactPersonId = contactPersonId;
            ExpectedDeliveryDate = expectedDeliveryDate;
            SupplierReference = string.IsNullOrWhiteSpace(supplierReference) ? null : supplierReference.Trim();
            IsOrderFinalized = false; // New orders start as not finalized
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates delivery information
        /// </summary>
        /// <param name="deliveryMethodId">New delivery method ID</param>
        /// <param name="expectedDeliveryDate">New expected delivery date (optional)</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDeliveryInformation(int deliveryMethodId, DateOnly? expectedDeliveryDate, int editedBy)
        {
            if (IsOrderFinalized)
                throw new InvalidOperationException("Cannot update delivery information for a finalized purchase order.");

            ValidateDeliveryMethodId(deliveryMethodId);
            ValidateExpectedDeliveryDate(expectedDeliveryDate, OrderDate);
            ValidateEditor(editedBy);

            DeliveryMethodId = deliveryMethodId;
            ExpectedDeliveryDate = expectedDeliveryDate;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the contact person
        /// </summary>
        /// <param name="contactPersonId">New contact person ID</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateContactPerson(int contactPersonId, int editedBy)
        {
            if (IsOrderFinalized)
                throw new InvalidOperationException("Cannot update contact person for a finalized purchase order.");

            ValidateContactPersonId(contactPersonId);
            ValidateEditor(editedBy);

            ContactPersonId = contactPersonId;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the supplier reference
        /// </summary>
        /// <param name="supplierReference">New supplier reference</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateSupplierReference(string? supplierReference, int editedBy)
        {
            if (IsOrderFinalized)
                throw new InvalidOperationException("Cannot update supplier reference for a finalized purchase order.");

            ValidateSupplierReference(supplierReference);
            ValidateEditor(editedBy);

            SupplierReference = string.IsNullOrWhiteSpace(supplierReference) ? null : supplierReference.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates external comments (sent to supplier)
        /// </summary>
        /// <param name="comments">New external comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateComments(string? comments, int editedBy)
        {
            if (IsOrderFinalized)
                throw new InvalidOperationException("Cannot update comments for a finalized purchase order.");

            ValidateEditor(editedBy);

            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates internal comments (not sent to supplier)
        /// </summary>
        /// <param name="internalComments">New internal comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateInternalComments(string? internalComments, int editedBy)
        {
            // Internal comments can be updated even when finalized
            ValidateEditor(editedBy);

            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Finalizes the purchase order, preventing further modifications to most fields
        /// </summary>
        /// <param name="editedBy">ID of the person finalizing the order</param>
        public void FinalizeOrder(int editedBy)
        {
            if (IsOrderFinalized)
                throw new InvalidOperationException("Purchase order is already finalized.");

            ValidateEditor(editedBy);

            IsOrderFinalized = true;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Unfinalizes the purchase order, allowing modifications again
        /// </summary>
        /// <param name="editedBy">ID of the person unfinalizing the order</param>
        public void UnfinalizeOrder(int editedBy)
        {
            if (!IsOrderFinalized)
                throw new InvalidOperationException("Purchase order is not finalized.");

            ValidateEditor(editedBy);

            IsOrderFinalized = false;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates both external and internal comments in a single operation
        /// </summary>
        /// <param name="comments">New external comments</param>
        /// <param name="internalComments">New internal comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateAllComments(string? comments, string? internalComments, int editedBy)
        {
            if (IsOrderFinalized)
                throw new InvalidOperationException("Cannot update external comments for a finalized purchase order.");

            ValidateEditor(editedBy);

            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            LastEditedBy = editedBy;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (PurchaseOrderId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            PurchaseOrderId = id;
        }

        internal void SetLastEditedWhen(DateTime lastEditedWhen)
        {
            LastEditedWhen = lastEditedWhen;
        }

        // Validation methods
        private static void ValidateSupplierId(int supplierId)
        {
            if (supplierId <= 0)
                throw new ArgumentException("Supplier ID must be a valid reference.", nameof(supplierId));
        }

        private static void ValidateOrderDate(DateOnly orderDate)
        {
            if (orderDate == default)
                throw new ArgumentException("Order date must be specified.", nameof(orderDate));

            // Prevent orders dated in the far future (more than 1 year ahead)
            if (orderDate > DateOnly.FromDateTime(DateTime.Today.AddYears(1)))
                throw new ArgumentException("Order date cannot be more than one year in the future.", nameof(orderDate));
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

        private static void ValidateSupplierReference(string? supplierReference)
        {
            if (!string.IsNullOrEmpty(supplierReference) && supplierReference.Length > 20)
                throw new ArgumentException("Supplier reference cannot exceed 20 characters.", nameof(supplierReference));
        }

        private static void ValidateExpectedDeliveryDate(DateOnly? expectedDeliveryDate, DateOnly orderDate)
        {
            if (expectedDeliveryDate.HasValue && expectedDeliveryDate.Value < orderDate)
                throw new ArgumentException("Expected delivery date cannot be before the order date.", nameof(expectedDeliveryDate));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            var status = IsOrderFinalized ? "Finalized" : "Draft";
            return $"PurchaseOrder {PurchaseOrderId}: {status} - Supplier {SupplierId} ({OrderDate})";
        }

        public override bool Equals(object? obj)
        {
            return obj is PurchaseOrder other && PurchaseOrderId == other.PurchaseOrderId;
        }

        public override int GetHashCode()
        {
            return PurchaseOrderId.GetHashCode();
        }
    }
} 