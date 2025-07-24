using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents any configurable parameters for the whole system (singleton entity)
    /// </summary>
    public class SystemParameters
    {
        /// <summary>
        /// Numeric ID used for row holding system parameters
        /// </summary>
        public int SystemParameterId { get; private set; }

        /// <summary>
        /// Company delivery address
        /// </summary>
        public Address DeliveryAddress { get; private set; }

        /// <summary>
        /// Geographic location for the company office
        /// </summary>
        public byte[] DeliveryLocation { get; private set; }

        /// <summary>
        /// Company postal address
        /// </summary>
        public Address PostalAddress { get; private set; }

        /// <summary>
        /// JSON-structured application settings
        /// </summary>
        [Required]
        public string ApplicationSettings { get; private set; } = string.Empty;

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private SystemParameters() 
        {
            DeliveryAddress = null!;
            PostalAddress = null!;
            DeliveryLocation = null!;
        }

        /// <summary>
        /// Creates new system parameters (typically done only once during system initialization)
        /// </summary>
        /// <param name="deliveryAddress">Company delivery address</param>
        /// <param name="deliveryLocation">Geographic location for company office</param>
        /// <param name="postalAddress">Company postal address</param>
        /// <param name="applicationSettings">JSON-structured application settings</param>
        /// <param name="lastEditedBy">ID of the person creating these parameters</param>
        public SystemParameters(
            Address deliveryAddress,
            byte[] deliveryLocation,
            Address postalAddress,
            string applicationSettings,
            int lastEditedBy)
        {
            ArgumentNullException.ThrowIfNull(deliveryAddress);
            ArgumentNullException.ThrowIfNull(deliveryLocation);
            ArgumentNullException.ThrowIfNull(postalAddress);
            ValidateApplicationSettings(applicationSettings);
            ValidateEditor(lastEditedBy);

            DeliveryAddress = deliveryAddress;
            DeliveryLocation = deliveryLocation;
            PostalAddress = postalAddress;
            ApplicationSettings = applicationSettings.Trim();
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the company delivery address and location
        /// </summary>
        /// <param name="newDeliveryAddress">New delivery address</param>
        /// <param name="newDeliveryLocation">New geographic location</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDeliveryAddress(Address newDeliveryAddress, byte[] newDeliveryLocation, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newDeliveryAddress);
            ArgumentNullException.ThrowIfNull(newDeliveryLocation);
            ValidateEditor(editedBy);

            DeliveryAddress = newDeliveryAddress;
            DeliveryLocation = newDeliveryLocation;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the company postal address
        /// </summary>
        /// <param name="newPostalAddress">New postal address</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePostalAddress(Address newPostalAddress, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newPostalAddress);
            ValidateEditor(editedBy);

            PostalAddress = newPostalAddress;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the application settings
        /// </summary>
        /// <param name="newApplicationSettings">New application settings JSON</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateApplicationSettings(string newApplicationSettings, int editedBy)
        {
            ValidateApplicationSettings(newApplicationSettings);
            ValidateEditor(editedBy);

            ApplicationSettings = newApplicationSettings.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates both company addresses simultaneously
        /// </summary>
        /// <param name="newDeliveryAddress">New delivery address</param>
        /// <param name="newDeliveryLocation">New geographic location</param>
        /// <param name="newPostalAddress">New postal address</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCompanyAddresses(Address newDeliveryAddress, byte[] newDeliveryLocation, Address newPostalAddress, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newDeliveryAddress);
            ArgumentNullException.ThrowIfNull(newDeliveryLocation);
            ArgumentNullException.ThrowIfNull(newPostalAddress);
            ValidateEditor(editedBy);

            DeliveryAddress = newDeliveryAddress;
            DeliveryLocation = newDeliveryLocation;
            PostalAddress = newPostalAddress;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        // Internal method for infrastructure layer
        internal void SetId(int id)
        {
            if (SystemParameterId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            SystemParameterId = id;
        }

        internal void SetLastEditedWhen(DateTime lastEditedWhen)
        {
            LastEditedWhen = lastEditedWhen;
        }

        // Validation methods
        private static void ValidateApplicationSettings(string applicationSettings)
        {
            if (string.IsNullOrWhiteSpace(applicationSettings))
                throw new ArgumentException("Application settings cannot be null or empty.", nameof(applicationSettings));

            // Basic JSON validation - check if it starts and ends with braces
            var trimmed = applicationSettings.Trim();
            if (!trimmed.StartsWith('{') || !trimmed.EndsWith('}'))
                throw new ArgumentException("Application settings must be valid JSON format.", nameof(applicationSettings));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"SystemParameters (ID: {SystemParameterId}) - Last edited: {LastEditedWhen:yyyy-MM-dd HH:mm:ss}";
        }

        public override bool Equals(object? obj)
        {
            return obj is SystemParameters other && SystemParameterId == other.SystemParameterId;
        }

        public override int GetHashCode()
        {
            return SystemParameterId.GetHashCode();
        }
    }
} 