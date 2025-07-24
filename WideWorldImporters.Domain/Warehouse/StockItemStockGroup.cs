using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Represents the association between stock items and stock groups
    /// This entity captures which stock items belong to which stock groups for categorization and reporting
    /// </summary>
    public class StockItemStockGroup
    {
        /// <summary>
        /// Internal reference for this linking row
        /// </summary>
        public int StockItemStockGroupId { get; private set; }

        /// <summary>
        /// Stock item assigned to this stock group (FK indexed via unique constraint)
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Stock group assigned to this stock item (FK indexed via unique constraint)
        /// </summary>
        public int StockGroupId { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private StockItemStockGroup() { }

        /// <summary>
        /// Creates a new stock item to stock group association
        /// This establishes a categorization relationship between a stock item and a stock group
        /// </summary>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="stockGroupId">Stock group ID reference</param>
        /// <param name="lastEditedBy">ID of the person creating this association</param>
        public StockItemStockGroup(int stockItemId, int stockGroupId, int lastEditedBy)
        {
            ValidateStockItemId(stockItemId);
            ValidateStockGroupId(stockGroupId);
            ValidateEditor(lastEditedBy);

            StockItemId = stockItemId;
            StockGroupId = stockGroupId;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a stock item to stock group association
        /// </summary>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="stockGroupId">Stock group ID reference</param>
        /// <param name="editedBy">ID of the person creating this association</param>
        /// <returns>New stock item stock group association</returns>
        public static StockItemStockGroup Create(int stockItemId, int stockGroupId, int editedBy)
        {
            return new StockItemStockGroup(stockItemId, stockGroupId, editedBy);
        }

        /// <summary>
        /// Updates the audit information when the association is modified
        /// </summary>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateAuditInformation(int editedBy)
        {
            ValidateEditor(editedBy);

            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Indicates if this association represents a valid relationship
        /// (Both stock item and stock group must be valid)
        /// </summary>
        public bool IsValidAssociation => StockItemId > 0 && StockGroupId > 0;

        /// <summary>
        /// Gets a summary description of this association
        /// </summary>
        public string AssociationSummary => $"Stock Item {StockItemId} → Stock Group {StockGroupId}";

        /// <summary>
        /// Gets the number of days since this association was last modified
        /// </summary>
        public int DaysSinceLastEdit => (DateTime.UtcNow - LastEditedWhen).Days;

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (StockItemStockGroupId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            StockItemStockGroupId = id;
        }

        // Validation methods
        private static void ValidateStockItemId(int stockItemId)
        {
            if (stockItemId <= 0)
                throw new ArgumentException("Stock item ID must be a valid reference.", nameof(stockItemId));
        }

        private static void ValidateStockGroupId(int stockGroupId)
        {
            if (stockGroupId <= 0)
                throw new ArgumentException("Stock group ID must be a valid reference.", nameof(stockGroupId));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"StockItemStockGroup: {AssociationSummary} (ID: {StockItemStockGroupId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is StockItemStockGroup other && 
                   StockItemStockGroupId != 0 && 
                   StockItemStockGroupId == other.StockItemStockGroupId;
        }

        public override int GetHashCode()
        {
            return StockItemStockGroupId != 0 ? StockItemStockGroupId.GetHashCode() : 
                   HashCode.Combine(StockItemId, StockGroupId);
        }

        /// <summary>
        /// Checks if two associations represent the same business relationship
        /// (same stock item and stock group combination)
        /// </summary>
        /// <param name="other">Other association to compare</param>
        /// <returns>True if they represent the same business relationship</returns>
        public bool IsSameAssociation(StockItemStockGroup other)
        {
            if (other == null) return false;
            return StockItemId == other.StockItemId && StockGroupId == other.StockGroupId;
        }
    }
} 