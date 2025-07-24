using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Represents the operational warehouse state and inventory holdings for a stock item
    /// This entity contains non-temporal attributes that change frequently during warehouse operations
    /// </summary>
    public class StockItemHolding
    {
        /// <summary>
        /// ID of the stock item that this holding relates to (this table holds non-temporal columns for stock)
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Quantity currently on hand (if tracked)
        /// </summary>
        public int QuantityOnHand { get; private set; }

        /// <summary>
        /// Bin location (i.e location of this stock item within the depot)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string BinLocation { get; private set; } = string.Empty;

        /// <summary>
        /// Quantity at last stocktake (if tracked)
        /// </summary>
        public int LastStocktakeQuantity { get; private set; }

        /// <summary>
        /// Unit cost price the last time this stock item was purchased
        /// </summary>
        public decimal LastCostPrice { get; private set; }

        /// <summary>
        /// Quantity below which reordering should take place
        /// </summary>
        public int ReorderLevel { get; private set; }

        /// <summary>
        /// Typical quantity ordered
        /// </summary>
        public int TargetStockLevel { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private StockItemHolding() { }

        /// <summary>
        /// Creates a new stock item holding record
        /// This represents the initial warehouse setup for a stock item
        /// </summary>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="binLocation">Bin location within the warehouse (required, max 20 characters)</param>
        /// <param name="reorderLevel">Reorder threshold quantity</param>
        /// <param name="targetStockLevel">Target inventory level</param>
        /// <param name="lastEditedBy">ID of the person creating this holding record</param>
        /// <param name="quantityOnHand">Initial quantity on hand</param>
        /// <param name="lastStocktakeQuantity">Last stocktake quantity</param>
        /// <param name="lastCostPrice">Last cost price per unit</param>
        public StockItemHolding(
            int stockItemId,
            string binLocation,
            int reorderLevel,
            int targetStockLevel,
            int lastEditedBy,
            int quantityOnHand = 0,
            int lastStocktakeQuantity = 0,
            decimal lastCostPrice = 0m)
        {
            ValidateStockItemId(stockItemId);
            ValidateBinLocation(binLocation);
            ValidateReorderLevel(reorderLevel);
            ValidateTargetStockLevel(targetStockLevel);
            ValidateQuantity(quantityOnHand, nameof(quantityOnHand));
            ValidateQuantity(lastStocktakeQuantity, nameof(lastStocktakeQuantity));
            ValidateLastCostPrice(lastCostPrice);
            ValidateEditor(lastEditedBy);

            StockItemId = stockItemId;
            BinLocation = binLocation.Trim().ToUpperInvariant(); // Normalize bin location
            ReorderLevel = reorderLevel;
            TargetStockLevel = targetStockLevel;
            QuantityOnHand = quantityOnHand;
            LastStocktakeQuantity = lastStocktakeQuantity;
            LastCostPrice = lastCostPrice;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Adjusts the quantity on hand (for receipts, shipments, adjustments)
        /// </summary>
        /// <param name="quantityChange">Change in quantity (positive for receipts, negative for shipments)</param>
        /// <param name="reason">Reason for the adjustment</param>
        /// <param name="editedBy">ID of the person making the adjustment</param>
        public void AdjustQuantity(int quantityChange, string reason, int editedBy)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            ValidateEditor(editedBy);

            var newQuantity = QuantityOnHand + quantityChange;
            if (newQuantity < 0)
                throw new InvalidOperationException($"Cannot adjust quantity by {quantityChange}: would result in negative inventory ({newQuantity}).");

            QuantityOnHand = newQuantity;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the bin location for this stock item
        /// </summary>
        /// <param name="newBinLocation">New bin location</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateBinLocation(string newBinLocation, int editedBy)
        {
            ValidateBinLocation(newBinLocation);
            ValidateEditor(editedBy);

            BinLocation = newBinLocation.Trim().ToUpperInvariant();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the cost price (typically after receiving new stock)
        /// </summary>
        /// <param name="newCostPrice">New cost price per unit</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCostPrice(decimal newCostPrice, int editedBy)
        {
            ValidateLastCostPrice(newCostPrice);
            ValidateEditor(editedBy);

            LastCostPrice = newCostPrice;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates reorder parameters
        /// </summary>
        /// <param name="newReorderLevel">New reorder level threshold</param>
        /// <param name="newTargetStockLevel">New target stock level</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateReorderParameters(int newReorderLevel, int newTargetStockLevel, int editedBy)
        {
            ValidateReorderLevel(newReorderLevel);
            ValidateTargetStockLevel(newTargetStockLevel);
            ValidateEditor(editedBy);

            if (newReorderLevel >= newTargetStockLevel)
                throw new ArgumentException("Reorder level must be less than target stock level.", nameof(newReorderLevel));

            ReorderLevel = newReorderLevel;
            TargetStockLevel = newTargetStockLevel;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Performs a stocktake (physical inventory count)
        /// </summary>
        /// <param name="countedQuantity">Physically counted quantity</param>
        /// <param name="editedBy">ID of the person performing the stocktake</param>
        public void PerformStocktake(int countedQuantity, int editedBy)
        {
            ValidateQuantity(countedQuantity, nameof(countedQuantity));
            ValidateEditor(editedBy);

            var discrepancy = countedQuantity - QuantityOnHand;
            
            LastStocktakeQuantity = countedQuantity;
            QuantityOnHand = countedQuantity; // Adjust to physical count
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;

            // Could potentially raise a domain event here for significant discrepancies
            if (Math.Abs(discrepancy) > TargetStockLevel * 0.1m) // 10% variance threshold
            {
                // Domain event or logging for significant stocktake discrepancies
            }
        }

        /// <summary>
        /// Indicates if this item needs to be reordered based on current quantity
        /// </summary>
        public bool NeedsReordering => QuantityOnHand <= ReorderLevel;

        /// <summary>
        /// Indicates if this item is overstocked based on target level
        /// </summary>
        public bool IsOverstocked => QuantityOnHand > TargetStockLevel * 1.5m; // 50% over target

        /// <summary>
        /// Indicates if there's a significant discrepancy from last stocktake
        /// </summary>
        public bool HasStocktakeDiscrepancy => Math.Abs(QuantityOnHand - LastStocktakeQuantity) > 0;

        /// <summary>
        /// Gets the current inventory value based on last cost price
        /// </summary>
        public decimal CurrentInventoryValue => QuantityOnHand * LastCostPrice;

        /// <summary>
        /// Gets the number of days since last edit (inventory activity indicator)
        /// </summary>
        public int DaysSinceLastActivity => (DateTime.UtcNow - LastEditedWhen).Days;

        /// <summary>
        /// Categorizes the inventory status for reporting
        /// </summary>
        public string InventoryStatus
        {
            get
            {
                if (QuantityOnHand == 0) return "Out of Stock";
                if (NeedsReordering) return "Low Stock";
                if (IsOverstocked) return "Overstocked";
                return "Normal";
            }
        }

        // Internal methods for infrastructure layer
        internal void SetStockItemId(int stockItemId)
        {
            if (StockItemId != 0)
                throw new InvalidOperationException("Stock item ID can only be set once.");
            
            StockItemId = stockItemId;
        }

        // Validation methods
        private static void ValidateStockItemId(int stockItemId)
        {
            if (stockItemId <= 0)
                throw new ArgumentException("Stock item ID must be a valid reference.", nameof(stockItemId));
        }

        private static void ValidateBinLocation(string binLocation)
        {
            if (string.IsNullOrWhiteSpace(binLocation))
                throw new ArgumentException("Bin location cannot be null or empty.", nameof(binLocation));
            
            if (binLocation.Length > 20)
                throw new ArgumentException("Bin location cannot exceed 20 characters.", nameof(binLocation));
        }

        private static void ValidateReorderLevel(int reorderLevel)
        {
            if (reorderLevel < 0)
                throw new ArgumentException("Reorder level cannot be negative.", nameof(reorderLevel));
        }

        private static void ValidateTargetStockLevel(int targetStockLevel)
        {
            if (targetStockLevel < 0)
                throw new ArgumentException("Target stock level cannot be negative.", nameof(targetStockLevel));
        }

        private static void ValidateQuantity(int quantity, string parameterName)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.", parameterName);
        }

        private static void ValidateLastCostPrice(decimal lastCostPrice)
        {
            if (lastCostPrice < 0)
                throw new ArgumentException("Last cost price cannot be negative.", nameof(lastCostPrice));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"Stock Item {StockItemId}: {QuantityOnHand} units @ {BinLocation} ({InventoryStatus})";
        }

        public override bool Equals(object? obj)
        {
            return obj is StockItemHolding other && StockItemId == other.StockItemId;
        }

        public override int GetHashCode()
        {
            return StockItemId.GetHashCode();
        }
    }
} 