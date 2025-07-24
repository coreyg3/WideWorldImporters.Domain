using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Purchasing
{
    /// <summary>
    /// Represents categories for suppliers (i.e novelties, toys, clothing, packaging, etc.)
    /// </summary>
    public class SupplierCategory
    {
        /// <summary>
        /// Numeric ID used for reference to a supplier category within the database
        /// </summary>
        public int SupplierCategoryId { get; private set; }

        /// <summary>
        /// Full name of the category that suppliers can be assigned to
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SupplierCategoryName { get; private set; } = string.Empty;

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
        private SupplierCategory() { }

        /// <summary>
        /// Creates a new supplier category
        /// </summary>
        /// <param name="supplierCategoryName">The supplier category name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this supplier category</param>
        /// <exception cref="ArgumentException">Thrown when supplier category name is invalid</exception>
        public SupplierCategory(string supplierCategoryName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(supplierCategoryName))
                throw new ArgumentException("Supplier category name cannot be null or empty.", nameof(supplierCategoryName));
            
            if (supplierCategoryName.Length > 50)
                throw new ArgumentException("Supplier category name cannot exceed 50 characters.", nameof(supplierCategoryName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            SupplierCategoryName = supplierCategoryName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the supplier category name
        /// </summary>
        /// <param name="newSupplierCategoryName">The new supplier category name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when supplier category name is invalid</exception>
        public void UpdateSupplierCategoryName(string newSupplierCategoryName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newSupplierCategoryName))
                throw new ArgumentException("Supplier category name cannot be null or empty.", nameof(newSupplierCategoryName));
            
            if (newSupplierCategoryName.Length > 50)
                throw new ArgumentException("Supplier category name cannot exceed 50 characters.", nameof(newSupplierCategoryName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            SupplierCategoryName = newSupplierCategoryName.Trim();
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
            if (SupplierCategoryId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            SupplierCategoryId = id;
        }

        public override string ToString()
        {
            return $"SupplierCategory: {SupplierCategoryName} (ID: {SupplierCategoryId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is SupplierCategory other && SupplierCategoryId == other.SupplierCategoryId;
        }

        public override int GetHashCode()
        {
            return SupplierCategoryId.GetHashCode();
        }
    }
} 