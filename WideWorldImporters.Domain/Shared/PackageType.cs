using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents ways that stock items can be packaged (i.e: each, box, carton, pallet, kg, etc.)
    /// </summary>
    public class PackageType
    {
        /// <summary>
        /// Numeric ID used for reference to a package type within the database
        /// </summary>
        public int PackageTypeId { get; private set; }

        /// <summary>
        /// Full name of package types that stock items can be purchased in or sold in
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PackageTypeName { get; private set; } = string.Empty;

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
        private PackageType() { }

        /// <summary>
        /// Creates a new package type
        /// </summary>
        /// <param name="packageTypeName">The package type name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this package type</param>
        /// <exception cref="ArgumentException">Thrown when package type name is invalid</exception>
        public PackageType(string packageTypeName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(packageTypeName))
                throw new ArgumentException("Package type name cannot be null or empty.", nameof(packageTypeName));
            
            if (packageTypeName.Length > 50)
                throw new ArgumentException("Package type name cannot exceed 50 characters.", nameof(packageTypeName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            PackageTypeName = packageTypeName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the package type name
        /// </summary>
        /// <param name="newPackageTypeName">The new package type name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when package type name is invalid</exception>
        public void UpdatePackageTypeName(string newPackageTypeName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newPackageTypeName))
                throw new ArgumentException("Package type name cannot be null or empty.", nameof(newPackageTypeName));
            
            if (newPackageTypeName.Length > 50)
                throw new ArgumentException("Package type name cannot exceed 50 characters.", nameof(newPackageTypeName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            PackageTypeName = newPackageTypeName.Trim();
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
            if (PackageTypeId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            PackageTypeId = id;
        }

        public override string ToString()
        {
            return $"PackageType: {PackageTypeName} (ID: {PackageTypeId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is PackageType other && PackageTypeId == other.PackageTypeId;
        }

        public override int GetHashCode()
        {
            return PackageTypeId.GetHashCode();
        }
    }
} 