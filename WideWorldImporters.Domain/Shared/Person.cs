using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents people known to the application (staff, customer contacts, supplier contacts)
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Numeric ID used for reference to a person within the database
        /// </summary>
        public int PersonId { get; private set; }

        /// <summary>
        /// Full name for this person
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FullName { get; private set; } = string.Empty;

        /// <summary>
        /// Name that this person prefers to be called
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PreferredName { get; private set; } = string.Empty;

        /// <summary>
        /// Name to build full text search on (computed column: PreferredName + ' ' + FullName)
        /// </summary>
        public string SearchName { get; private set; } = string.Empty;

        /// <summary>
        /// Is this person permitted to log on?
        /// </summary>
        public bool IsPermittedToLogon { get; private set; }

        /// <summary>
        /// Person's system logon name
        /// </summary>
        [MaxLength(50)]
        public string? LogonName { get; private set; }

        /// <summary>
        /// Is logon token provided by an external system?
        /// </summary>
        public bool IsExternalLogonProvider { get; private set; }

        /// <summary>
        /// Hash of password for users without external logon tokens
        /// </summary>
        public byte[]? HashedPassword { get; private set; }

        /// <summary>
        /// Is the currently permitted to make online access?
        /// </summary>
        public bool IsSystemUser { get; private set; }

        /// <summary>
        /// Is this person an employee?
        /// </summary>
        public bool IsEmployee { get; private set; }

        /// <summary>
        /// Is this person a staff salesperson?
        /// </summary>
        public bool IsSalesperson { get; private set; }

        /// <summary>
        /// User preferences related to the website (holds JSON data)
        /// </summary>
        public string? UserPreferences { get; private set; }

        /// <summary>
        /// Contact information (phone, fax, email)
        /// </summary>
        public PersonContactInformation ContactInformation { get; private set; }

        /// <summary>
        /// Photo of this person
        /// </summary>
        public byte[]? Photo { get; private set; }

        /// <summary>
        /// Custom fields for employees and salespeople
        /// </summary>
        public string? CustomFields { get; private set; }

        /// <summary>
        /// Other languages spoken (computed column from custom fields)
        /// </summary>
        public string? OtherLanguages { get; private set; }

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
        private Person() 
        {
            ContactInformation = null!;
        }

        /// <summary>
        /// Creates a new person
        /// </summary>
        /// <param name="fullName">Full name (required, max 50 characters)</param>
        /// <param name="preferredName">Preferred name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this record</param>
        /// <param name="contactInformation">Contact information (optional)</param>
        /// <param name="isPermittedToLogon">Can this person log on?</param>
        /// <param name="logonName">System logon name (optional)</param>
        /// <param name="isExternalLogonProvider">Uses external authentication?</param>
        /// <param name="isSystemUser">Has online access?</param>
        /// <param name="isEmployee">Is an employee?</param>
        /// <param name="isSalesperson">Is a salesperson?</param>
        /// <param name="userPreferences">User preferences JSON (optional)</param>
        /// <param name="photo">Photo data (optional)</param>
        /// <param name="customFields">Custom fields JSON (optional)</param>
        public Person(
            string fullName,
            string preferredName,
            int lastEditedBy,
            PersonContactInformation? contactInformation = null,
            bool isPermittedToLogon = false,
            string? logonName = null,
            bool isExternalLogonProvider = false,
            bool isSystemUser = false,
            bool isEmployee = false,
            bool isSalesperson = false,
            string? userPreferences = null,
            byte[]? photo = null,
            string? customFields = null)
        {
            ValidateFullName(fullName);
            ValidatePreferredName(preferredName);
            ValidateLogonName(logonName);
            ValidateLogonConfiguration(isPermittedToLogon, logonName, isExternalLogonProvider);
            ValidateEditor(lastEditedBy);

            FullName = fullName.Trim();
            PreferredName = preferredName.Trim();
            ContactInformation = contactInformation ?? new PersonContactInformation();
            IsPermittedToLogon = isPermittedToLogon;
            LogonName = string.IsNullOrWhiteSpace(logonName) ? null : logonName.Trim();
            IsExternalLogonProvider = isExternalLogonProvider;
            IsSystemUser = isSystemUser;
            IsEmployee = isEmployee;
            IsSalesperson = isSalesperson;
            UserPreferences = string.IsNullOrWhiteSpace(userPreferences) ? null : userPreferences.Trim();
            Photo = photo;
            CustomFields = string.IsNullOrWhiteSpace(customFields) ? null : customFields.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the person's names
        /// </summary>
        /// <param name="newFullName">New full name</param>
        /// <param name="newPreferredName">New preferred name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateNames(string newFullName, string newPreferredName, int editedBy)
        {
            ValidateFullName(newFullName);
            ValidatePreferredName(newPreferredName);
            ValidateEditor(editedBy);

            FullName = newFullName.Trim();
            PreferredName = newPreferredName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates contact information
        /// </summary>
        /// <param name="newContactInformation">New contact information</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateContactInformation(PersonContactInformation newContactInformation, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newContactInformation);
            ValidateEditor(editedBy);

            ContactInformation = newContactInformation;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates logon configuration
        /// </summary>
        /// <param name="isPermittedToLogon">Can log on?</param>
        /// <param name="logonName">System logon name</param>
        /// <param name="isExternalLogonProvider">Uses external authentication?</param>
        /// <param name="isSystemUser">Has online access?</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateLogonConfiguration(bool isPermittedToLogon, string? logonName, bool isExternalLogonProvider, bool isSystemUser, int editedBy)
        {
            ValidateLogonName(logonName);
            ValidateLogonConfiguration(isPermittedToLogon, logonName, isExternalLogonProvider);
            ValidateEditor(editedBy);

            IsPermittedToLogon = isPermittedToLogon;
            LogonName = string.IsNullOrWhiteSpace(logonName) ? null : logonName.Trim();
            IsExternalLogonProvider = isExternalLogonProvider;
            IsSystemUser = isSystemUser;
            
            // Clear password hash if switching to external provider
            if (isExternalLogonProvider)
            {
                HashedPassword = null;
            }
            
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Sets the password hash (for internal authentication)
        /// </summary>
        /// <param name="hashedPassword">Hashed password</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void SetPasswordHash(byte[] hashedPassword, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(hashedPassword);
            
            if (IsExternalLogonProvider)
                throw new InvalidOperationException("Cannot set password hash for external logon providers.");
                
            ValidateEditor(editedBy);

            HashedPassword = hashedPassword;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Clears the password hash
        /// </summary>
        /// <param name="editedBy">ID of the person making the change</param>
        public void ClearPasswordHash(int editedBy)
        {
            ValidateEditor(editedBy);

            HashedPassword = null;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates role assignments
        /// </summary>
        /// <param name="isEmployee">Is an employee?</param>
        /// <param name="isSalesperson">Is a salesperson?</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateRoles(bool isEmployee, bool isSalesperson, int editedBy)
        {
            ValidateEditor(editedBy);

            IsEmployee = isEmployee;
            IsSalesperson = isSalesperson;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates user preferences
        /// </summary>
        /// <param name="newUserPreferences">New user preferences JSON</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateUserPreferences(string? newUserPreferences, int editedBy)
        {
            ValidateEditor(editedBy);

            UserPreferences = string.IsNullOrWhiteSpace(newUserPreferences) ? null : newUserPreferences.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the person's photo
        /// </summary>
        /// <param name="newPhoto">New photo data</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePhoto(byte[]? newPhoto, int editedBy)
        {
            ValidateEditor(editedBy);

            Photo = newPhoto;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates custom fields
        /// </summary>
        /// <param name="newCustomFields">New custom fields JSON</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCustomFields(string? newCustomFields, int editedBy)
        {
            ValidateEditor(editedBy);

            CustomFields = string.IsNullOrWhiteSpace(newCustomFields) ? null : newCustomFields.Trim();
            LastEditedBy = editedBy;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (PersonId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            PersonId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        internal void SetComputedColumns(string searchName, string? otherLanguages)
        {
            SearchName = searchName;
            OtherLanguages = otherLanguages;
        }

        // Validation methods
        private static void ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be null or empty.", nameof(fullName));
            
            if (fullName.Length > 50)
                throw new ArgumentException("Full name cannot exceed 50 characters.", nameof(fullName));
        }

        private static void ValidatePreferredName(string preferredName)
        {
            if (string.IsNullOrWhiteSpace(preferredName))
                throw new ArgumentException("Preferred name cannot be null or empty.", nameof(preferredName));
            
            if (preferredName.Length > 50)
                throw new ArgumentException("Preferred name cannot exceed 50 characters.", nameof(preferredName));
        }

        private static void ValidateLogonName(string? logonName)
        {
            if (!string.IsNullOrEmpty(logonName) && logonName.Length > 50)
                throw new ArgumentException("Logon name cannot exceed 50 characters.", nameof(logonName));
        }

        private static void ValidateLogonConfiguration(bool isPermittedToLogon, string? logonName, bool isExternalLogonProvider)
        {
            if (isPermittedToLogon && string.IsNullOrWhiteSpace(logonName))
                throw new ArgumentException("Logon name is required when person is permitted to logon.");
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"Person: {PreferredName} ({FullName}) - ID: {PersonId}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Person other && PersonId == other.PersonId;
        }

        public override int GetHashCode()
        {
            return PersonId.GetHashCode();
        }
    }
} 