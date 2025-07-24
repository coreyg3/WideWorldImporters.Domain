using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Represents groups used to categorize stock items (e.g., novelties, toys, edible novelties, etc.)
    /// </summary>
    public class StockGroup
    {
        /// <summary>
        /// Numeric ID used for reference to a stock group within the database
        /// </summary>
        public int StockGroupId { get; private set; }

        /// <summary>
        /// Full name of groups used to categorize stock items
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string StockGroupName { get; private set; } = string.Empty;

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
        private StockGroup() { }

        /// <summary>
        /// Creates a new stock group
        /// </summary>
        /// <param name="stockGroupName">The stock group name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this stock group</param>
        /// <exception cref="ArgumentException">Thrown when stock group name is invalid</exception>
        public StockGroup(string stockGroupName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(stockGroupName))
                throw new ArgumentException("Stock group name cannot be null or empty.", nameof(stockGroupName));
            
            if (stockGroupName.Length > 50)
                throw new ArgumentException("Stock group name cannot exceed 50 characters.", nameof(stockGroupName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            StockGroupName = stockGroupName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the stock group name
        /// </summary>
        /// <param name="newStockGroupName">The new stock group name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when stock group name is invalid</exception>
        public void UpdateStockGroupName(string newStockGroupName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newStockGroupName))
                throw new ArgumentException("Stock group name cannot be null or empty.", nameof(newStockGroupName));
            
            if (newStockGroupName.Length > 50)
                throw new ArgumentException("Stock group name cannot exceed 50 characters.", nameof(newStockGroupName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            StockGroupName = newStockGroupName.Trim();
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
            if (StockGroupId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            StockGroupId = id;
        }

        public override string ToString()
        {
            return $"StockGroup: {StockGroupName} (ID: {StockGroupId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is StockGroup other && StockGroupId == other.StockGroupId;
        }

        public override int GetHashCode()
        {
            return StockGroupId.GetHashCode();
        }
    }
} 