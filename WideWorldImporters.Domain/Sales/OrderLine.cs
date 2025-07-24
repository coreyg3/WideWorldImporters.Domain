using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a detail line from a customer order
    /// This entity manages individual items through the order-to-fulfillment workflow including warehouse picking
    /// </summary>
    public class OrderLine
    {
        /// <summary>
        /// Numeric ID used for reference to a line on an order within the database
        /// </summary>
        public int OrderLineId { get; private set; }

        /// <summary>
        /// Order that this line is associated with
        /// </summary>
        public int OrderId { get; private set; }

        /// <summary>
        /// Stock item for this order line
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Description of the item to be supplied (Usually the stock item name but can be overridden)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Type of package to be supplied
        /// </summary>
        public int PackageTypeId { get; private set; }

        /// <summary>
        /// Financial information for this order line (quantity, pricing, tax)
        /// </summary>
        public OrderLineFinancials Financials { get; private set; }

        /// <summary>
        /// Quantity picked from stock (starts at 0, updated during picking)
        /// </summary>
        public int PickedQuantity { get; private set; }

        /// <summary>
        /// When was picking of this line completed?
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
        private OrderLine() { Financials = null!; }

        /// <summary>
        /// Creates a new order line
        /// </summary>
        /// <param name="orderId">Order ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="description">Description of the item (max 100 characters)</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="financials">Financial information for this line</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        public OrderLine(
            int orderId,
            int stockItemId,
            string description,
            int packageTypeId,
            OrderLineFinancials financials,
            int lastEditedBy)
        {
            ValidateOrderId(orderId);
            ValidateStockItemId(stockItemId);
            ValidateDescription(description);
            ValidatePackageTypeId(packageTypeId);
            ValidateEditor(lastEditedBy);

            OrderId = orderId;
            StockItemId = stockItemId;
            Description = description.Trim();
            PackageTypeId = packageTypeId;
            Financials = financials ?? throw new ArgumentNullException(nameof(financials));
            PickedQuantity = 0; // New lines start with no items picked
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new order line with calculated financials
        /// </summary>
        /// <param name="orderId">Order ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="description">Description of the item</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="quantity">Quantity to be supplied</param>
        /// <param name="unitPrice">Unit price to be charged (null for free items)</param>
        /// <param name="taxRate">Tax rate to be applied</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        /// <returns>New order line with calculated financials</returns>
        public static OrderLine Create(
            int orderId,
            int stockItemId,
            string description,
            int packageTypeId,
            int quantity,
            decimal? unitPrice,
            decimal taxRate,
            int lastEditedBy)
        {
            var financials = new OrderLineFinancials(quantity, unitPrice, taxRate);
            return new OrderLine(orderId, stockItemId, description, packageTypeId, financials, lastEditedBy);
        }

        /// <summary>
        /// Creates a new order line for free/promotional items
        /// </summary>
        /// <param name="orderId">Order ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="description">Description of the item</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="quantity">Quantity of free items</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        /// <returns>New order line for free items</returns>
        public static OrderLine CreateFreeItem(
            int orderId,
            int stockItemId,
            string description,
            int packageTypeId,
            int quantity,
            int lastEditedBy)
        {
            var financials = OrderLineFinancials.CreateFreeItems(quantity);
            return new OrderLine(orderId, stockItemId, description, packageTypeId, financials, lastEditedBy);
        }

        /// <summary>
        /// Updates the quantity and recalculates financials
        /// </summary>
        /// <param name="newQuantity">New quantity to be supplied</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateQuantity(int newQuantity, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot update quantity for a completed order line.");

            ValidateEditor(editedBy);

            // Cannot reduce quantity below what's already been picked
            if (newQuantity < PickedQuantity)
                throw new InvalidOperationException($"Cannot reduce quantity below already picked quantity ({PickedQuantity}).");

            Financials = Financials.WithQuantity(newQuantity);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the unit price and recalculates financials
        /// </summary>
        /// <param name="newUnitPrice">New unit price (null for free items)</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateUnitPrice(decimal? newUnitPrice, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot update unit price for a completed order line.");

            ValidateEditor(editedBy);

            Financials = Financials.WithUnitPrice(newUnitPrice);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the tax rate and recalculates financials
        /// </summary>
        /// <param name="newTaxRate">New tax rate</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateTaxRate(decimal newTaxRate, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot update tax rate for a completed order line.");

            ValidateEditor(editedBy);

            Financials = Financials.WithTaxRate(newTaxRate);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the description
        /// </summary>
        /// <param name="newDescription">New description</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDescription(string newDescription, int editedBy)
        {
            ValidateDescription(newDescription);
            ValidateEditor(editedBy);

            Description = newDescription.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the package type
        /// </summary>
        /// <param name="newPackageTypeId">New package type ID</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePackageType(int newPackageTypeId, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot update package type for a completed order line.");

            ValidatePackageTypeId(newPackageTypeId);
            ValidateEditor(editedBy);

            PackageTypeId = newPackageTypeId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Records picked quantity for this line (partial picking)
        /// </summary>
        /// <param name="pickedQuantity">Quantity picked in this operation</param>
        /// <param name="editedBy">ID of the person recording the pick</param>
        public void RecordPickedQuantity(int pickedQuantity, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot record picks for a completed order line.");

            if (pickedQuantity <= 0)
                throw new ArgumentException("Picked quantity must be positive.", nameof(pickedQuantity));

            ValidateEditor(editedBy);

            var newTotalPicked = PickedQuantity + pickedQuantity;
            if (newTotalPicked > Quantity)
                throw new InvalidOperationException($"Cannot pick more than ordered quantity. Ordered: {Quantity}, Already picked: {PickedQuantity}, Attempting to pick: {pickedQuantity}");

            PickedQuantity = newTotalPicked;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Adjusts the picked quantity (for corrections)
        /// </summary>
        /// <param name="newPickedQuantity">New total picked quantity</param>
        /// <param name="editedBy">ID of the person making the adjustment</param>
        /// <param name="adjustmentReason">Reason for the adjustment</param>
        public void AdjustPickedQuantity(int newPickedQuantity, int editedBy, string adjustmentReason)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot adjust picked quantity for a completed order line.");

            if (newPickedQuantity < 0)
                throw new ArgumentException("Picked quantity cannot be negative.", nameof(newPickedQuantity));

            if (newPickedQuantity > Quantity)
                throw new ArgumentException("Picked quantity cannot exceed ordered quantity.", nameof(newPickedQuantity));

            if (string.IsNullOrWhiteSpace(adjustmentReason))
                throw new ArgumentException("Adjustment reason must be provided.", nameof(adjustmentReason));

            ValidateEditor(editedBy);

            PickedQuantity = newPickedQuantity;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Completes picking for this line
        /// </summary>
        /// <param name="pickingCompletedWhen">When picking was completed</param>
        /// <param name="editedBy">ID of the person completing the pick</param>
        public void CompletePicking(DateTime pickingCompletedWhen, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Order line picking has already been completed.");

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
        /// Reopens picking for this line (undoes completion)
        /// </summary>
        /// <param name="editedBy">ID of the person reopening the pick</param>
        public void ReopenPicking(int editedBy)
        {
            if (!IsPickingCompleted)
                throw new InvalidOperationException("Cannot reopen picking for an order line that is not completed.");

            ValidateEditor(editedBy);

            PickingCompletedWhen = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the complete financial information
        /// </summary>
        /// <param name="newFinancials">New financial calculations</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateFinancials(OrderLineFinancials newFinancials, int editedBy)
        {
            if (IsPickingCompleted)
                throw new InvalidOperationException("Cannot update financials for a completed order line.");

            ArgumentNullException.ThrowIfNull(newFinancials);
            ValidateEditor(editedBy);

            // Cannot reduce quantity below what's already been picked
            if (newFinancials.Quantity < PickedQuantity)
                throw new InvalidOperationException($"Cannot reduce quantity below already picked quantity ({PickedQuantity}).");

            Financials = newFinancials;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Convenience properties delegating to financials
        /// </summary>
        public int Quantity => Financials.Quantity;
        public decimal? UnitPrice => Financials.UnitPrice;
        public decimal TaxRate => Financials.TaxRate;
        public decimal ExtendedPrice => Financials.ExtendedPrice;
        public decimal TaxAmount => Financials.TaxAmount;
        public decimal TotalIncludingTax => Financials.TotalIncludingTax;

        /// <summary>
        /// Indicates if picking has been completed for this line
        /// </summary>
        public bool IsPickingCompleted => PickingCompletedWhen.HasValue;

        /// <summary>
        /// Indicates if this line has been fully picked
        /// </summary>
        public bool IsFullyPicked => PickedQuantity >= Quantity;

        /// <summary>
        /// Indicates if this line has been partially picked
        /// </summary>
        public bool IsPartiallyPicked => PickedQuantity > 0 && PickedQuantity < Quantity;

        /// <summary>
        /// Gets the outstanding quantity (ordered but not yet picked)
        /// </summary>
        public int OutstandingQuantity => Quantity - PickedQuantity;

        /// <summary>
        /// Gets the pick completion percentage
        /// </summary>
        public decimal PickCompletionPercentage => Quantity > 0 ? (decimal)PickedQuantity / Quantity * 100m : 0m;

        /// <summary>
        /// Indicates if this line represents free/promotional items
        /// </summary>
        public bool IsFreeItem => Financials.IsFreeItem;

        /// <summary>
        /// Indicates if this line has taxable amount
        /// </summary>
        public bool IsTaxable => Financials.IsTaxable;

        /// <summary>
        /// Gets the picking status for workflow tracking
        /// </summary>
        public string PickingStatus
        {
            get
            {
                if (IsPickingCompleted) return "Completed";
                if (IsFullyPicked) return "Fully Picked";
                if (IsPartiallyPicked) return "Partially Picked";
                return "Pending";
            }
        }

        /// <summary>
        /// Gets the line value category for reporting
        /// </summary>
        public string ValueCategory => Financials.ValueCategory;

        /// <summary>
        /// Gets a comprehensive status summary for reporting
        /// </summary>
        public string StatusSummary
        {
            get
            {
                var parts = new List<string> { PickingStatus };
                
                if (IsPartiallyPicked || IsFullyPicked)
                    parts.Add($"{PickedQuantity}/{Quantity} picked ({PickCompletionPercentage:F0}%)");
                    
                if (IsFreeItem)
                    parts.Add("Promotional");
                    
                return string.Join(" | ", parts);
            }
        }

        /// <summary>
        /// Indicates if this line requires attention (not fully picked when completed)
        /// </summary>
        public bool RequiresAttention => IsPickingCompleted && !IsFullyPicked;

        /// <summary>
        /// Gets the time spent picking (if completed)
        /// </summary>
        public TimeSpan? PickingDuration
        {
            get
            {
                if (!IsPickingCompleted) return null;
                // This is an approximation - in practice, you'd track when picking started
                return PickingCompletedWhen!.Value - LastEditedWhen;
            }
        }

        // Validation methods
        private static void ValidateOrderId(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be a valid reference.", nameof(orderId));
        }

        private static void ValidateStockItemId(int stockItemId)
        {
            if (stockItemId <= 0)
                throw new ArgumentException("Stock item ID must be a valid reference.", nameof(stockItemId));
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

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("Editor must be a valid person ID.", nameof(editedBy));
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (OrderLineId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            OrderLineId = id;
        }

        public override string ToString()
        {
            return $"OrderLine {OrderLineId}: {Description} - {StatusSummary}";
        }

        public override bool Equals(object? obj)
        {
            return obj is OrderLine other && OrderLineId == other.OrderLineId;
        }

        public override int GetHashCode()
        {
            return OrderLineId.GetHashCode();
        }
    }
} 