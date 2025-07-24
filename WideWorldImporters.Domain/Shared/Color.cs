using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents colors that stock items can (optionally) have
    /// </summary>
    public class Color
    {
        /// <summary>
        /// Numeric ID used for reference to a color within the database
        /// </summary>
        public int ColorId { get; private set; }

        /// <summary>
        /// Full name of a color that can be used to describe stock items
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ColorName { get; private set; } = string.Empty;

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
        private Color() { }

        /// <summary>
        /// Creates a new color
        /// </summary>
        /// <param name="colorName">The color name (required, max 20 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this color</param>
        /// <exception cref="ArgumentException">Thrown when color name is invalid</exception>
        public Color(string colorName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(colorName))
                throw new ArgumentException("Color name cannot be null or empty.", nameof(colorName));
            
            if (colorName.Length > 20)
                throw new ArgumentException("Color name cannot exceed 20 characters.", nameof(colorName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            ColorName = colorName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the color name
        /// </summary>
        /// <param name="newColorName">The new color name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when color name is invalid</exception>
        public void UpdateColorName(string newColorName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newColorName))
                throw new ArgumentException("Color name cannot be null or empty.", nameof(newColorName));
            
            if (newColorName.Length > 20)
                throw new ArgumentException("Color name cannot exceed 20 characters.", nameof(newColorName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            ColorName = newColorName.Trim();
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
            if (ColorId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            ColorId = id;
        }

        public override string ToString()
        {
            return $"Color: {ColorName} (ID: {ColorId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Color other && ColorId == other.ColorId;
        }

        public override int GetHashCode()
        {
            return ColorId.GetHashCode();
        }
    }
} 