using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents a state or province that contains cities (including geographic location)
    /// </summary>
    public class StateProvince
    {
        /// <summary>
        /// Numeric ID used for reference to a state or province within the database
        /// </summary>
        public int StateProvinceId { get; private set; }

        /// <summary>
        /// Common code for this state or province (such as WA - Washington for the USA)
        /// </summary>
        [Required]
        [MaxLength(5)]
        public string StateProvinceCode { get; private set; } = string.Empty;

        /// <summary>
        /// Formal name of the state or province
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string StateProvinceName { get; private set; } = string.Empty;

        /// <summary>
        /// Country for this StateProvince
        /// </summary>
        public int CountryId { get; private set; }

        /// <summary>
        /// Sales territory for this StateProvince
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SalesTerritory { get; private set; } = string.Empty;

        /// <summary>
        /// Geographic boundary of the state or province
        /// </summary>
        public byte[]? Border { get; private set; }

        /// <summary>
        /// Latest available population for the StateProvince
        /// </summary>
        public long? LatestRecordedPopulation { get; private set; }

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
        private StateProvince() { }

        /// <summary>
        /// Creates a new state or province
        /// </summary>
        /// <param name="stateProvinceCode">Common code (required, max 5 characters)</param>
        /// <param name="stateProvinceName">Formal name (required, max 50 characters)</param>
        /// <param name="countryId">Country ID reference</param>
        /// <param name="salesTerritory">Sales territory (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this state/province</param>
        /// <param name="border">Geographic border data (optional)</param>
        /// <param name="latestRecordedPopulation">Latest population data (optional)</param>
        public StateProvince(
            string stateProvinceCode,
            string stateProvinceName,
            int countryId,
            string salesTerritory,
            int lastEditedBy,
            byte[]? border = null,
            long? latestRecordedPopulation = null)
        {
            ValidateStateProvinceCode(stateProvinceCode);
            ValidateStateProvinceName(stateProvinceName);
            ValidateCountryId(countryId);
            ValidateSalesTerritory(salesTerritory);
            ValidatePopulation(latestRecordedPopulation);
            ValidateEditor(lastEditedBy);

            StateProvinceCode = stateProvinceCode.Trim().ToUpperInvariant();
            StateProvinceName = stateProvinceName.Trim();
            CountryId = countryId;
            SalesTerritory = salesTerritory.Trim();
            Border = border;
            LatestRecordedPopulation = latestRecordedPopulation;
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the state/province identification
        /// </summary>
        /// <param name="newStateProvinceCode">New state/province code</param>
        /// <param name="newStateProvinceName">New state/province name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateStateProvinceIdentification(string newStateProvinceCode, string newStateProvinceName, int editedBy)
        {
            ValidateStateProvinceCode(newStateProvinceCode);
            ValidateStateProvinceName(newStateProvinceName);
            ValidateEditor(editedBy);

            StateProvinceCode = newStateProvinceCode.Trim().ToUpperInvariant();
            StateProvinceName = newStateProvinceName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the sales territory
        /// </summary>
        /// <param name="newSalesTerritory">New sales territory</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateSalesTerritory(string newSalesTerritory, int editedBy)
        {
            ValidateSalesTerritory(newSalesTerritory);
            ValidateEditor(editedBy);

            SalesTerritory = newSalesTerritory.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the population data
        /// </summary>
        /// <param name="newPopulation">New population figure</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePopulation(long? newPopulation, int editedBy)
        {
            ValidatePopulation(newPopulation);
            ValidateEditor(editedBy);

            LatestRecordedPopulation = newPopulation;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the geographic border data
        /// </summary>
        /// <param name="newBorder">New border geographic data</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateBorder(byte[]? newBorder, int editedBy)
        {
            ValidateEditor(editedBy);

            Border = newBorder;
            LastEditedBy = editedBy;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (StateProvinceId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            StateProvinceId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        // Validation methods
        private static void ValidateStateProvinceCode(string stateProvinceCode)
        {
            if (string.IsNullOrWhiteSpace(stateProvinceCode))
                throw new ArgumentException("State province code cannot be null or empty.", nameof(stateProvinceCode));
            
            if (stateProvinceCode.Length > 5)
                throw new ArgumentException("State province code cannot exceed 5 characters.", nameof(stateProvinceCode));
        }

        private static void ValidateStateProvinceName(string stateProvinceName)
        {
            if (string.IsNullOrWhiteSpace(stateProvinceName))
                throw new ArgumentException("State province name cannot be null or empty.", nameof(stateProvinceName));
            
            if (stateProvinceName.Length > 50)
                throw new ArgumentException("State province name cannot exceed 50 characters.", nameof(stateProvinceName));
        }

        private static void ValidateCountryId(int countryId)
        {
            if (countryId <= 0)
                throw new ArgumentException("Country ID must be a valid reference.", nameof(countryId));
        }

        private static void ValidateSalesTerritory(string salesTerritory)
        {
            if (string.IsNullOrWhiteSpace(salesTerritory))
                throw new ArgumentException("Sales territory cannot be null or empty.", nameof(salesTerritory));
            
            if (salesTerritory.Length > 50)
                throw new ArgumentException("Sales territory cannot exceed 50 characters.", nameof(salesTerritory));
        }

        private static void ValidatePopulation(long? population)
        {
            if (population.HasValue && population.Value < 0)
                throw new ArgumentException("Population cannot be negative.", nameof(population));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"StateProvince: {StateProvinceName} ({StateProvinceCode}) - ID: {StateProvinceId}";
        }

        public override bool Equals(object? obj)
        {
            return obj is StateProvince other && StateProvinceId == other.StateProvinceId;
        }

        public override int GetHashCode()
        {
            return StateProvinceId.GetHashCode();
        }
    }
} 