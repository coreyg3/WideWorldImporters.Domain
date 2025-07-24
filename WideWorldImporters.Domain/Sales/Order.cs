using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a customer sales order - the operational heart of the order-to-cash process
    /// This entity manages the complete order lifecycle from placement through picking and delivery preparation
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Numeric ID used for reference to an order within the database
        /// </summary>
        public int OrderId { get; private set; }

        /// <summary>
        /// Customer for this order
        /// </summary>
        public int CustomerId { get; private set; }

        /// <summary>
        /// Salesperson for this order
        /// </summary>
        public int SalespersonPersonId { get; private set; }

        /// <summary>
        /// Person who picked this shipment (null until picking is assigned)
        /// </summary>
        public int? PickedByPersonId { get; private set; }

        /// <summary>
        /// Customer contact for this order
        /// </summary>
        public int ContactPersonId { get; private set; }

        /// <summary>
        /// If this order is a backorder, this holds the original order number
        /// </summary>
        public int? BackorderOrderId { get; private set; }

        /// <summary>
        /// Date that this order was raised
        /// </summary>
        public DateOnly OrderDate { get; private set; }

        /// <summary>
        /// Expected delivery date
        /// </summary>
        public DateOnly ExpectedDeliveryDate { get; private set; }

        /// <summary>
        /// Purchase Order Number received from customer
        /// </summary>
        [MaxLength(20)]
        public string? CustomerPurchaseOrderNumber { get; private set; }

        /// <summary>
        /// If items cannot be supplied are they backordered?
        /// </summary>
        public bool IsUndersupplyBackordered { get; private set; }

        /// <summary>
        /// Any comments related to this order (sent to customer)
        /// </summary>
        public string? Comments { get; private set; }

        /// <summary>
        /// Any comments related to order delivery (sent to customer)
        /// </summary>
        public string? DeliveryInstructions { get; private set; }

        /// <summary>
        /// Any internal comments related to this order (not sent to the customer)
        /// </summary>
        public string? InternalComments { get; private set; }

        /// <summary>
        /// When was picking of the entire order completed?
        /// </summary>
        public DateTime? PickingCompletedWhen { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private Order() { }

        /// <summary>
        /// Creates a new sales order
        /// </summary>
        /// <param name="customerId">Customer ID reference</param>
        /// <param name="salespersonPersonId">Salesperson ID reference</param>
        /// <param name="contactPersonId">Customer contact person ID reference</param>
        /// <param name="orderDate">Date the order was placed</param>
        /// <param name="expectedDeliveryDate">Expected delivery date</param>
        /// <param name="lastEditedBy">ID of the person creating this order</param>
        /// <param name="customerPurchaseOrderNumber">Customer's PO number (optional, max 20 characters)</param>
        /// <param name="isUndersupplyBackordered">Whether to backorder undersupplied items</param>
        /// <param name="comments">Customer-facing comments (optional)</param>
        /// <param name="deliveryInstructions">Delivery instructions for customer (optional)</param>
        /// <param name="internalComments">Internal comments (optional)</param>
        /// <param name="backorderOrderId">Original order ID if this is a backorder (optional)</param>
        public Order(
            int customerId,
            int salespersonPersonId,
            int contactPersonId,
            DateOnly orderDate,
            DateOnly expectedDeliveryDate,
            int lastEditedBy,
            string? customerPurchaseOrderNumber = null,
            bool isUndersupplyBackordered = true,
            string? comments = null,
            string? deliveryInstructions = null,
            string? internalComments = null,
            int? backorderOrderId = null)
        {
            ValidateCustomerId(customerId);
            ValidateSalespersonPersonId(salespersonPersonId);
            ValidateContactPersonId(contactPersonId);
            ValidateOrderDate(orderDate);
            ValidateExpectedDeliveryDate(expectedDeliveryDate, orderDate);
            ValidateEditor(lastEditedBy);
            ValidateCustomerPurchaseOrderNumber(customerPurchaseOrderNumber);
            ValidateBackorderOrderId(backorderOrderId);

            CustomerId = customerId;
            SalespersonPersonId = salespersonPersonId;
            ContactPersonId = contactPersonId;
            OrderDate = orderDate;
            ExpectedDeliveryDate = expectedDeliveryDate;
            CustomerPurchaseOrderNumber = string.IsNullOrWhiteSpace(customerPurchaseOrderNumber) ? null : customerPurchaseOrderNumber.Trim();
            IsUndersupplyBackordered = isUndersupplyBackordered;
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions.Trim();
            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            BackorderOrderId = backorderOrderId;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a standard sales order
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="salespersonPersonId">Salesperson ID</param>
        /// <param name="contactPersonId">Customer contact ID</param>
        /// <param name="orderDate">Order date</param>
        /// <param name="expectedDeliveryDate">Expected delivery date</param>
        /// <param name="lastEditedBy">Person creating the order</param>
        /// <param name="customerPurchaseOrderNumber">Customer PO number (optional)</param>
        /// <returns>New sales order</returns>
        public static Order CreateStandardOrder(
            int customerId,
            int salespersonPersonId,
            int contactPersonId,
            DateOnly orderDate,
            DateOnly expectedDeliveryDate,
            int lastEditedBy,
            string? customerPurchaseOrderNumber = null)
        {
            return new Order(
                customerId,
                salespersonPersonId,
                contactPersonId,
                orderDate,
                expectedDeliveryDate,
                lastEditedBy,
                customerPurchaseOrderNumber: customerPurchaseOrderNumber,
                isUndersupplyBackordered: true);
        }

        /// <summary>
        /// Creates a backorder from an existing order
        /// </summary>
        /// <param name="originalOrderId">Original order ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="salespersonPersonId">Salesperson ID</param>
        /// <param name="contactPersonId">Customer contact ID</param>
        /// <param name="orderDate">Backorder date</param>
        /// <param name="expectedDeliveryDate">Expected delivery date for backorder</param>
        /// <param name="lastEditedBy">Person creating the backorder</param>
        /// <param name="customerPurchaseOrderNumber">Customer PO number (optional)</param>
        /// <returns>New backorder</returns>
        public static Order CreateBackorder(
            int originalOrderId,
            int customerId,
            int salespersonPersonId,
            int contactPersonId,
            DateOnly orderDate,
            DateOnly expectedDeliveryDate,
            int lastEditedBy,
            string? customerPurchaseOrderNumber = null)
        {
            return new Order(
                customerId,
                salespersonPersonId,
                contactPersonId,
                orderDate,
                expectedDeliveryDate,
                lastEditedBy,
                customerPurchaseOrderNumber: customerPurchaseOrderNumber,
                isUndersupplyBackordered: true,
                backorderOrderId: originalOrderId);
        }

        /// <summary>
        /// Assigns a picker to this order to begin warehouse fulfillment
        /// </summary>
        /// <param name="pickedByPersonId">Person ID who will pick this order</param>
        /// <param name="editedBy">Person making the assignment</param>
        public void AssignPicker(int pickedByPersonId, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot assign picker to an order that has already been picked.");

            if (pickedByPersonId <= 0)
                throw new ArgumentException("Picker person ID must be a valid reference.", nameof(pickedByPersonId));

            ValidateEditor(editedBy);

            PickedByPersonId = pickedByPersonId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the picker assignment (before picking is completed)
        /// </summary>
        /// <param name="editedBy">Person removing the assignment</param>
        public void UnassignPicker(int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot unassign picker from an order that has already been picked.");

            if (!IsPickerAssigned)
                throw new InvalidOperationException("No picker is currently assigned to this order.");

            ValidateEditor(editedBy);

            PickedByPersonId = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks picking as completed for this order
        /// </summary>
        /// <param name="pickingCompletedWhen">When picking was completed</param>
        /// <param name="editedBy">Person completing the picking</param>
        public void CompletePicking(DateTime pickingCompletedWhen, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Order picking has already been completed.");

            if (!IsPickerAssigned)
                throw new InvalidOperationException("Cannot complete picking without an assigned picker.");

            if (pickingCompletedWhen < DateTime.UtcNow.AddDays(-1))
                throw new ArgumentException("Picking completion time cannot be more than 1 day in the past.", nameof(pickingCompletedWhen));

            if (pickingCompletedWhen > DateTime.UtcNow.AddMinutes(5))
                throw new ArgumentException("Picking completion time cannot be in the future.", nameof(pickingCompletedWhen));

            ValidateEditor(editedBy);

            PickingCompletedWhen = pickingCompletedWhen;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Reopens picking for this order (undoes completion)
        /// </summary>
        /// <param name="editedBy">Person reopening the picking</param>
        public void ReopenPicking(int editedBy)
        {
            if (!IsPickingCompleted)
                throw new InvalidOperationException("Cannot reopen picking for an order that is not completed.");

            ValidateEditor(editedBy);

            PickingCompletedWhen = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the expected delivery date
        /// </summary>
        /// <param name="newExpectedDeliveryDate">New expected delivery date</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateExpectedDeliveryDate(DateOnly newExpectedDeliveryDate, int editedBy)
        {
            ValidateExpectedDeliveryDate(newExpectedDeliveryDate, OrderDate);
            ValidateEditor(editedBy);

            ExpectedDeliveryDate = newExpectedDeliveryDate;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the customer contact person
        /// </summary>
        /// <param name="newContactPersonId">New customer contact person ID</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateContactPerson(int newContactPersonId, int editedBy)
        {
            ValidateContactPersonId(newContactPersonId);
            ValidateEditor(editedBy);

            ContactPersonId = newContactPersonId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the salesperson for this order
        /// </summary>
        /// <param name="newSalespersonPersonId">New salesperson person ID</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateSalesperson(int newSalespersonPersonId, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot change salesperson for a picked order.");

            ValidateSalespersonPersonId(newSalespersonPersonId);
            ValidateEditor(editedBy);

            SalespersonPersonId = newSalespersonPersonId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the customer purchase order number
        /// </summary>
        /// <param name="newCustomerPurchaseOrderNumber">New customer PO number</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateCustomerPurchaseOrderNumber(string? newCustomerPurchaseOrderNumber, int editedBy)
        {
            ValidateCustomerPurchaseOrderNumber(newCustomerPurchaseOrderNumber);
            ValidateEditor(editedBy);

            CustomerPurchaseOrderNumber = string.IsNullOrWhiteSpace(newCustomerPurchaseOrderNumber) ? null : newCustomerPurchaseOrderNumber.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the backorder policy for this order
        /// </summary>
        /// <param name="isUndersupplyBackordered">Whether to backorder undersupplied items</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateBackorderPolicy(bool isUndersupplyBackordered, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot change backorder policy for a picked order.");

            ValidateEditor(editedBy);

            IsUndersupplyBackordered = isUndersupplyBackordered;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates customer-facing comments
        /// </summary>
        /// <param name="newComments">New customer comments</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateComments(string? newComments, int editedBy)
        {
            ValidateEditor(editedBy);

            Comments = string.IsNullOrWhiteSpace(newComments) ? null : newComments.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates delivery instructions for the customer
        /// </summary>
        /// <param name="newDeliveryInstructions">New delivery instructions</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateDeliveryInstructions(string? newDeliveryInstructions, int editedBy)
        {
            ValidateEditor(editedBy);

            DeliveryInstructions = string.IsNullOrWhiteSpace(newDeliveryInstructions) ? null : newDeliveryInstructions.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates internal comments (not sent to customer)
        /// </summary>
        /// <param name="newInternalComments">New internal comments</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateInternalComments(string? newInternalComments, int editedBy)
        {
            ValidateEditor(editedBy);

            InternalComments = string.IsNullOrWhiteSpace(newInternalComments) ? null : newInternalComments.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates all comment fields in a single operation
        /// </summary>
        /// <param name="newComments">New customer comments</param>
        /// <param name="newDeliveryInstructions">New delivery instructions</param>
        /// <param name="newInternalComments">New internal comments</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateAllComments(string? newComments, string? newDeliveryInstructions, string? newInternalComments, int editedBy)
        {
            ValidateEditor(editedBy);

            Comments = string.IsNullOrWhiteSpace(newComments) ? null : newComments.Trim();
            DeliveryInstructions = string.IsNullOrWhiteSpace(newDeliveryInstructions) ? null : newDeliveryInstructions.Trim();
            InternalComments = string.IsNullOrWhiteSpace(newInternalComments) ? null : newInternalComments.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Indicates if a picker has been assigned to this order
        /// </summary>
        public bool IsPickerAssigned => PickedByPersonId.HasValue;

        /// <summary>
        /// Indicates if picking has been completed for this order
        /// </summary>
        public bool IsPickingCompleted => PickingCompletedWhen.HasValue;

        /// <summary>
        /// Indicates if this order is a backorder from another order
        /// </summary>
        public bool IsBackorder => BackorderOrderId.HasValue;

        /// <summary>
        /// Indicates if the expected delivery date has passed
        /// </summary>
        public bool IsDeliveryOverdue => DateOnly.FromDateTime(DateTime.Today) > ExpectedDeliveryDate && !IsPickingCompleted;

        /// <summary>
        /// Gets the age of this order in days from order date
        /// </summary>
        public int AgeDays => DateOnly.FromDateTime(DateTime.Today).DayNumber - OrderDate.DayNumber;

        /// <summary>
        /// Gets the number of days until expected delivery (negative if overdue)
        /// </summary>
        public int DaysToDelivery => ExpectedDeliveryDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

        /// <summary>
        /// Gets the time spent picking (if completed) or time since picking started
        /// </summary>
        public TimeSpan? PickingDuration
        {
            get
            {
                if (!IsPickerAssigned) return null;
                if (IsPickingCompleted) return PickingCompletedWhen!.Value - LastEditedWhen; // Approximation
                return DateTime.UtcNow - LastEditedWhen; // Time since picker assignment
            }
        }

        /// <summary>
        /// Gets the current order status for workflow tracking
        /// </summary>
        public string OrderStatus
        {
            get
            {
                if (IsPickingCompleted) return "Picked";
                if (IsPickerAssigned) return "Picking";
                if (IsDeliveryOverdue) return "Overdue";
                return "Pending";
            }
        }

        /// <summary>
        /// Gets the order workflow stage
        /// </summary>
        public string WorkflowStage
        {
            get
            {
                if (IsPickingCompleted) return "Ready for Dispatch";
                if (IsPickerAssigned) return "Warehouse Fulfillment";
                return "Order Processing";
            }
        }

        /// <summary>
        /// Gets the delivery urgency level
        /// </summary>
        public string DeliveryUrgency
        {
            get
            {
                var daysToDelivery = DaysToDelivery;
                if (daysToDelivery < 0) return "OVERDUE";
                if (daysToDelivery == 0) return "Due Today";
                if (daysToDelivery <= 1) return "Urgent";
                if (daysToDelivery <= 3) return "Normal";
                return "Low Priority";
            }
        }

        /// <summary>
        /// Gets a comprehensive status summary for reporting
        /// </summary>
        public string StatusSummary
        {
            get
            {
                var parts = new List<string> { OrderStatus, WorkflowStage };
                
                if (IsBackorder)
                    parts.Add($"Backorder from Order {BackorderOrderId}");
                    
                if (IsDeliveryOverdue)
                    parts.Add($"OVERDUE ({Math.Abs(DaysToDelivery)} days)");
                else if (DaysToDelivery <= 1)
                    parts.Add($"Due in {DaysToDelivery} day(s)");
                    
                return string.Join(" | ", parts);
            }
        }

        /// <summary>
        /// Indicates if this order has any customer-facing communications
        /// </summary>
        public bool HasCustomerCommunications => !string.IsNullOrEmpty(Comments) || !string.IsNullOrEmpty(DeliveryInstructions);

        /// <summary>
        /// Indicates if this order has internal notes
        /// </summary>
        public bool HasInternalNotes => !string.IsNullOrEmpty(InternalComments);

        /// <summary>
        /// Indicates if this order has a customer purchase order number
        /// </summary>
        public bool HasCustomerPurchaseOrder => !string.IsNullOrEmpty(CustomerPurchaseOrderNumber);

        // Validation methods
        private static void ValidateCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be a valid customer reference.", nameof(customerId));
        }

        private static void ValidateSalespersonPersonId(int salespersonPersonId)
        {
            if (salespersonPersonId <= 0)
                throw new ArgumentException("Salesperson person ID must be a valid reference.", nameof(salespersonPersonId));
        }

        private static void ValidateContactPersonId(int contactPersonId)
        {
            if (contactPersonId <= 0)
                throw new ArgumentException("Contact person ID must be a valid reference.", nameof(contactPersonId));
        }

        private static void ValidateOrderDate(DateOnly orderDate)
        {
            if (orderDate == default)
                throw new ArgumentException("Order date must be a valid date.", nameof(orderDate));

            var today = DateOnly.FromDateTime(DateTime.Today);
            if (orderDate > today.AddDays(1)) // Allow for timezone differences
                throw new ArgumentException("Order date cannot be in the future.", nameof(orderDate));
        }

        private static void ValidateExpectedDeliveryDate(DateOnly expectedDeliveryDate, DateOnly orderDate)
        {
            if (expectedDeliveryDate == default)
                throw new ArgumentException("Expected delivery date must be a valid date.", nameof(expectedDeliveryDate));

            if (expectedDeliveryDate < orderDate)
                throw new ArgumentException("Expected delivery date cannot be before order date.", nameof(expectedDeliveryDate));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("Editor must be a valid person ID.", nameof(editedBy));
        }

        private static void ValidateCustomerPurchaseOrderNumber(string? customerPurchaseOrderNumber)
        {
            if (!string.IsNullOrEmpty(customerPurchaseOrderNumber) && customerPurchaseOrderNumber.Length > 20)
                throw new ArgumentException("Customer purchase order number cannot exceed 20 characters.", nameof(customerPurchaseOrderNumber));
        }

        private static void ValidateBackorderOrderId(int? backorderOrderId)
        {
            if (backorderOrderId.HasValue && backorderOrderId.Value <= 0)
                throw new ArgumentException("Backorder order ID must be a valid order reference.", nameof(backorderOrderId));
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (OrderId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            OrderId = id;
        }

        /// <summary>
        /// Sets the last edited timestamp (typically called by infrastructure layer)
        /// </summary>
        internal void SetLastEditedWhen(DateTime lastEditedWhen)
        {
            LastEditedWhen = lastEditedWhen;
        }

        public override string ToString()
        {
            var orderType = IsBackorder ? "Backorder" : "Order";
            var status = StatusSummary;
            return $"{orderType} {OrderId}: Customer {CustomerId} - {status}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Order other && OrderId == other.OrderId;
        }

        public override int GetHashCode()
        {
            return OrderId.GetHashCode();
        }
    }
} 