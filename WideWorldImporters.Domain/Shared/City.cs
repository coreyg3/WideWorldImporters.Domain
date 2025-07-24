using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents a city that is part of any address (including geographic location)
    /// </summary>
    public class City
    {
        /// <summary>
        /// Numeric ID used for reference to a city within the database
        /// </summary>
        public int CityId { get; private set; }

        /// <summary>
        /// Formal name of the city
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CityName { get; private set; } = string.Empty;

        /// <summary>
        /// State or province for this city
        /// </summary>
        public int StateProvinceId { get; private set; }

        /// <summary>
        /// Geographic location of the city
        /// </summary>
        public byte[]? Location { get; private set; }

        /// <summary>
        /// Latest available population for the City
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
        private City() { }

        /// <summary>
        /// Creates a new city
        /// </summary>
        /// <param name="cityName">Formal name of the city (required, max 50 characters)</param>
        /// <param name="stateProvinceId">State or province ID reference</param>
        /// <param name="lastEditedBy">ID of the person creating this city</param>
        /// <param name="location">Geographic location data (optional)</param>
        /// <param name="latestRecordedPopulation">Latest population data (optional)</param>
        public City(
            string cityName,
            int stateProvinceId,
            int lastEditedBy,
            byte[]? location = null,
            long? latestRecordedPopulation = null)
        {
            ValidateCityName(cityName);
            ValidateStateProvinceId(stateProvinceId);
            ValidatePopulation(latestRecordedPopulation);
            ValidateEditor(lastEditedBy);

            CityName = cityName.Trim();
            StateProvinceId = stateProvinceId;
            Location = location;
            LatestRecordedPopulation = latestRecordedPopulation;
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the city name
        /// </summary>
        /// <param name="newCityName">New city name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCityName(string newCityName, int editedBy)
        {
            ValidateCityName(newCityName);
            ValidateEditor(editedBy);

            CityName = newCityName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the geographic location
        /// </summary>
        /// <param name="newLocation">New geographic location data</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateLocation(byte[]? newLocation, int editedBy)
        {
            ValidateEditor(editedBy);

            Location = newLocation;
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

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (CityId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            CityId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        // Validation methods
        private static void ValidateCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("City name cannot be null or empty.", nameof(cityName));
            
            if (cityName.Length > 50)
                throw new ArgumentException("City name cannot exceed 50 characters.", nameof(cityName));
        }

        private static void ValidateStateProvinceId(int stateProvinceId)
        {
            if (stateProvinceId <= 0)
                throw new ArgumentException("State province ID must be a valid reference.", nameof(stateProvinceId));
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
            return $"City: {CityName} - ID: {CityId}";
        }

        public override bool Equals(object? obj)
        {
            return obj is City other && CityId == other.CityId;
        }

        public override int GetHashCode()
        {
            return CityId.GetHashCode();
        }
    }
} 