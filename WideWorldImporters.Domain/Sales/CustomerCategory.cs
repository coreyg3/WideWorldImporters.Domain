using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a customer category (e.g., restaurants, cafes, supermarkets, etc.)
    /// </summary>
    public class CustomerCategory
    {
        public int CustomerCategoryId { get; private set; }

        [Required]
        [MaxLength(50)]
        public string CustomerCategoryName { get; private set; } = string.Empty;

        public int LastEditedBy { get; private set; }

        public DateTime ValidFrom { get; private set; }

        public DateTime ValidTo { get; private set; }

        // Private parameterless constructor for EF Core
        private CustomerCategory() { }

        /// <summary>
        /// Creates a new customer category
        /// </summary>
        /// <param name="customerCategoryName">The category name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this category</param>
        /// <exception cref="ArgumentException">Thrown when category name is invalid</exception>
        public CustomerCategory(string customerCategoryName, int lastEditedBy)
        {
            // Always check input parameters first
            if (string.IsNullOrWhiteSpace(customerCategoryName))
                throw new ArgumentException("Customer category name cannot be null or empty.", nameof(customerCategoryName));

            if (customerCategoryName.Length > 50)
                throw new ArgumentException("Customer category name cannot exceed 50 characters.", nameof(customerCategoryName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            CustomerCategoryName = customerCategoryName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the category name
        /// </summary>
        /// <param name="newCategoryName">The new category name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when category name is invalid</exception>
        public void UpdateCategoryName(string newCategoryName, int editedBy)
        {
            // Always check input parameters first
            if (string.IsNullOrWhiteSpace(newCategoryName))
                throw new ArgumentException("Customer category name cannot be null or empty.", nameof(newCategoryName));

            if (newCategoryName.Length > 50)
                throw new ArgumentException("Customer category name cannot exceed 50 characters.", nameof(newCategoryName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            CustomerCategoryName = newCategoryName.Trim();
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
            if (CustomerCategoryId != 0)
                throw new InvalidOperationException("ID can only be set once.");

            CustomerCategoryId = id;
        }

        public override string ToString()
        {
            return $"CustomerCategory: {CustomerCategoryName} (ID: {CustomerCategoryId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is CustomerCategory other && CustomerCategoryId == other.CustomerCategoryId;
        }

        public override int GetHashCode()
        {
            return CustomerCategoryId.GetHashCode();
        }
    }
}