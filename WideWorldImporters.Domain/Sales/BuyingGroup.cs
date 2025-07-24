using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a buying group that customers can be members of to exert greater buying power
    /// </summary>
    public class BuyingGroup
    {
        /// <summary>
        /// Numeric ID used for reference to a buying group within the database
        /// </summary>
        public int BuyingGroupId { get; private set; }

        /// <summary>
        /// Full name of a buying group that customers can be members of
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BuyingGroupName { get; private set; } = string.Empty;

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
        private BuyingGroup() { }

        /// <summary>
        /// Creates a new buying group
        /// </summary>
        /// <param name="buyingGroupName">The buying group name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this buying group</param>
        /// <exception cref="ArgumentException">Thrown when buying group name is invalid</exception>
        public BuyingGroup(string buyingGroupName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(buyingGroupName))
                throw new ArgumentException("Buying group name cannot be null or empty.", nameof(buyingGroupName));
            
            if (buyingGroupName.Length > 50)
                throw new ArgumentException("Buying group name cannot exceed 50 characters.", nameof(buyingGroupName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            BuyingGroupName = buyingGroupName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the buying group name
        /// </summary>
        /// <param name="newBuyingGroupName">The new buying group name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when buying group name is invalid</exception>
        public void UpdateBuyingGroupName(string newBuyingGroupName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newBuyingGroupName))
                throw new ArgumentException("Buying group name cannot be null or empty.", nameof(newBuyingGroupName));
            
            if (newBuyingGroupName.Length > 50)
                throw new ArgumentException("Buying group name cannot exceed 50 characters.", nameof(newBuyingGroupName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            BuyingGroupName = newBuyingGroupName.Trim();
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
            if (BuyingGroupId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            BuyingGroupId = id;
        }

        public override string ToString()
        {
            return $"BuyingGroup: {BuyingGroupName} (ID: {BuyingGroupId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is BuyingGroup other && BuyingGroupId == other.BuyingGroupId;
        }

        public override int GetHashCode()
        {
            return BuyingGroupId.GetHashCode();
        }
    }
} 