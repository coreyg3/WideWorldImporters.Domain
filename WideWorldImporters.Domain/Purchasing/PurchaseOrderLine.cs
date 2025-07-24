using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Purchasing
{
    /// <summary>
    /// Detail lines from supplier purchase orders
    /// </summary>
    public class PurchaseOrderLine
    {
        /// <summary>
        /// Numeric ID used for reference to a line on a purchase order within the database
        /// </summary>
        public int PurchaseOrderLineId { get; private set; }

        /// <summary>
        /// Purchase order that this line is associated with
        /// </summary>
        public int PurchaseOrderId { get; private set; }

        /// <summary>
        /// Stock item for this purchase order line
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Quantity of the stock item that is ordered
        /// </summary>
        public int OrderedOuters { get; private set; }

        /// <summary>
        /// Description of the item to be supplied (Often the stock item name but could be supplier description)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Total quantity of the stock item that has been received so far
        /// </summary>
        public int ReceivedOuters { get; private set; }

        /// <summary>
        /// Type of package received
        /// </summary>
        public int PackageTypeId { get; private set; }

        /// <summary>
        /// The unit price that we expect to be charged
        /// </summary>
        public decimal? ExpectedUnitPricePerOuter { get; private set; }

        /// <summary>
        /// The last date on which this stock item was received for this purchase order
        /// </summary>
        public DateOnly? LastReceiptDate { get; private set; }

        /// <summary>
        /// Is this purchase order line now considered finalized? (Receipted quantities and weights are often not precise)
        /// </summary>
        public bool IsOrderLineFinalized { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private PurchaseOrderLine() { }

        /// <summary>
        /// Creates a new purchase order line
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="orderedOuters">Quantity ordered in outer packages</param>
        /// <param name="description">Description of the item (max 100 characters)</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        /// <param name="expectedUnitPricePerOuter">Expected unit price per outer (optional)</param>
        public PurchaseOrderLine(
            int purchaseOrderId,
            int stockItemId,
            int orderedOuters,
            string description,
            int packageTypeId,
            int lastEditedBy,
            decimal? expectedUnitPricePerOuter = null)
        {
            ValidatePurchaseOrderId(purchaseOrderId);
            ValidateStockItemId(stockItemId);
            ValidateOrderedOuters(orderedOuters);
            ValidateDescription(description);
            ValidatePackageTypeId(packageTypeId);
            ValidateEditor(lastEditedBy);
            ValidateExpectedUnitPrice(expectedUnitPricePerOuter);

            PurchaseOrderId = purchaseOrderId;
            StockItemId = stockItemId;
            OrderedOuters = orderedOuters;
            Description = description.Trim();
            PackageTypeId = packageTypeId;
            ExpectedUnitPricePerOuter = expectedUnitPricePerOuter;
            ReceivedOuters = 0; // New lines start with no receipts
            IsOrderLineFinalized = false; // New lines start as not finalized
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the ordered quantity
        /// </summary>
        /// <param name="newOrderedOuters">New quantity to order</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateOrderedQuantity(int newOrderedOuters, int editedBy)
        {
            if (IsOrderLineFinalized)
                throw new InvalidOperationException("Cannot update ordered quantity for a finalized purchase order line.");

            ValidateOrderedOuters(newOrderedOuters);
            ValidateEditor(editedBy);

            // Ensure new ordered quantity is not less than already received
            if (newOrderedOuters < ReceivedOuters)
                throw new InvalidOperationException($"Cannot reduce ordered quantity below already received quantity ({ReceivedOuters}).");

            OrderedOuters = newOrderedOuters;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the item description
        /// </summary>
        /// <param name="newDescription">New description</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDescription(string newDescription, int editedBy)
        {
            if (IsOrderLineFinalized)
                throw new InvalidOperationException("Cannot update description for a finalized purchase order line.");

            ValidateDescription(newDescription);
            ValidateEditor(editedBy);

            Description = newDescription.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the expected unit price
        /// </summary>
        /// <param name="newExpectedUnitPricePerOuter">New expected unit price per outer</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateExpectedUnitPrice(decimal? newExpectedUnitPricePerOuter, int editedBy)
        {
            if (IsOrderLineFinalized)
                throw new InvalidOperationException("Cannot update expected unit price for a finalized purchase order line.");

            ValidateExpectedUnitPrice(newExpectedUnitPricePerOuter);
            ValidateEditor(editedBy);

            ExpectedUnitPricePerOuter = newExpectedUnitPricePerOuter;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Records a receipt of goods for this line
        /// </summary>
        /// <param name="receivedQuantity">Quantity received in this receipt</param>
        /// <param name="receiptDate">Date of receipt</param>
        /// <param name="editedBy">ID of the person recording the receipt</param>
        public void RecordReceipt(int receivedQuantity, DateOnly receiptDate, int editedBy)
        {
            if (IsOrderLineFinalized)
                throw new InvalidOperationException("Cannot record receipts for a finalized purchase order line.");

            if (receivedQuantity <= 0)
                throw new ArgumentException("Received quantity must be greater than zero.", nameof(receivedQuantity));

            ValidateEditor(editedBy);

            var newTotalReceived = ReceivedOuters + receivedQuantity;
            if (newTotalReceived > OrderedOuters)
                throw new InvalidOperationException($"Cannot receive more than ordered quantity. Ordered: {OrderedOuters}, Already received: {ReceivedOuters}, Attempting to receive: {receivedQuantity}");

            ReceivedOuters = newTotalReceived;
            LastReceiptDate = receiptDate;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Adjusts the received quantity (for corrections)
        /// </summary>
        /// <param name="newReceivedOuters">New total received quantity</param>
        /// <param name="editedBy">ID of the person making the adjustment</param>
        /// <param name="adjustmentReason">Reason for the adjustment</param>
        public void AdjustReceivedQuantity(int newReceivedOuters, int editedBy, string adjustmentReason)
        {
            if (IsOrderLineFinalized)
                throw new InvalidOperationException("Cannot adjust received quantity for a finalized purchase order line.");

            if (newReceivedOuters < 0)
                throw new ArgumentException("Received quantity cannot be negative.", nameof(newReceivedOuters));

            if (newReceivedOuters > OrderedOuters)
                throw new ArgumentException("Received quantity cannot exceed ordered quantity.", nameof(newReceivedOuters));

            if (string.IsNullOrWhiteSpace(adjustmentReason))
                throw new ArgumentException("Adjustment reason must be provided.", nameof(adjustmentReason));

            ValidateEditor(editedBy);

            ReceivedOuters = newReceivedOuters;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Finalizes the purchase order line, preventing further modifications
        /// </summary>
        /// <param name="editedBy">ID of the person finalizing the line</param>
        public void FinalizeOrderLine(int editedBy)
        {
            if (IsOrderLineFinalized)
                throw new InvalidOperationException("Purchase order line is already finalized.");

            ValidateEditor(editedBy);

            IsOrderLineFinalized = true;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Unfinalizes the purchase order line, allowing modifications again
        /// </summary>
        /// <param name="editedBy">ID of the person unfinalizing the line</param>
        public void UnfinalizeOrderLine(int editedBy)
        {
            if (!IsOrderLineFinalized)
                throw new InvalidOperationException("Purchase order line is not finalized.");

            ValidateEditor(editedBy);

            IsOrderLineFinalized = false;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Gets the outstanding quantity (ordered but not yet received)
        /// </summary>
        public int OutstandingQuantity => OrderedOuters - ReceivedOuters;

        /// <summary>
        /// Indicates whether this line is fully received
        /// </summary>
        public bool IsFullyReceived => ReceivedOuters >= OrderedOuters;

        /// <summary>
        /// Gets the total expected cost for this line (if expected unit price is available)
        /// </summary>
        public decimal? TotalExpectedCost => ExpectedUnitPricePerOuter * OrderedOuters;

        /// <summary>
        /// Gets the received cost based on expected unit price (if available)
        /// </summary>
        public decimal? ReceivedCost => ExpectedUnitPricePerOuter * ReceivedOuters;

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (PurchaseOrderLineId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            PurchaseOrderLineId = id;
        }

        internal void SetLastEditedWhen(DateTime lastEditedWhen)
        {
            LastEditedWhen = lastEditedWhen;
        }

        // Validation methods
        private static void ValidatePurchaseOrderId(int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
                throw new ArgumentException("Purchase order ID must be a valid reference.", nameof(purchaseOrderId));
        }

        private static void ValidateStockItemId(int stockItemId)
        {
            if (stockItemId <= 0)
                throw new ArgumentException("Stock item ID must be a valid reference.", nameof(stockItemId));
        }

        private static void ValidateOrderedOuters(int orderedOuters)
        {
            if (orderedOuters <= 0)
                throw new ArgumentException("Ordered quantity must be greater than zero.", nameof(orderedOuters));
        }

        private static void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty.", nameof(description));
            
            if (description.Length > 100)
                throw new ArgumentException("Description cannot exceed 100 characters.", nameof(description));
        }

        private static void ValidatePackageTypeId(int packageTypeId)
        {
            if (packageTypeId <= 0)
                throw new ArgumentException("Package type ID must be a valid reference.", nameof(packageTypeId));
        }

        private static void ValidateExpectedUnitPrice(decimal? expectedUnitPricePerOuter)
        {
            if (expectedUnitPricePerOuter.HasValue && expectedUnitPricePerOuter.Value < 0)
                throw new ArgumentException("Expected unit price cannot be negative.", nameof(expectedUnitPricePerOuter));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            var status = IsOrderLineFinalized ? "Finalized" : "Active";
            var received = IsFullyReceived ? "Fully Received" : $"{ReceivedOuters}/{OrderedOuters} received";
            return $"PurchaseOrderLine {PurchaseOrderLineId}: {Description} - {status}, {received}";
        }

        public override bool Equals(object? obj)
        {
            return obj is PurchaseOrderLine other && PurchaseOrderLineId == other.PurchaseOrderLineId;
        }

        public override int GetHashCode()
        {
            return PurchaseOrderLineId.GetHashCode();
        }
    }
} 