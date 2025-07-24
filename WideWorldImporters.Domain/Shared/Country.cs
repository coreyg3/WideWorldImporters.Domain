using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents a country that contains states or provinces (including geographic boundaries)
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Numeric ID used for reference to a country within the database
        /// </summary>
        public int CountryId { get; private set; }

        /// <summary>
        /// Name of the country
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string CountryName { get; private set; } = string.Empty;

        /// <summary>
        /// Full formal name of the country as agreed by United Nations
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string FormalName { get; private set; } = string.Empty;

        /// <summary>
        /// 3 letter alphabetic code assigned to the country by ISO
        /// </summary>
        [MaxLength(3)]
        public string? IsoAlpha3Code { get; private set; }

        /// <summary>
        /// Numeric code assigned to the country by ISO
        /// </summary>
        public int? IsoNumericCode { get; private set; }

        /// <summary>
        /// Type of country or administrative region
        /// </summary>
        [MaxLength(20)]
        public string? CountryType { get; private set; }

        /// <summary>
        /// Latest available population for the country
        /// </summary>
        public long? LatestRecordedPopulation { get; private set; }

        /// <summary>
        /// Name of the continent
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Continent { get; private set; } = string.Empty;

        /// <summary>
        /// Name of the region
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Region { get; private set; } = string.Empty;

        /// <summary>
        /// Name of the subregion
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Subregion { get; private set; } = string.Empty;

        /// <summary>
        /// Geographic border of the country as described by the United Nations
        /// </summary>
        public byte[]? Border { get; private set; }

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
        private Country() { }

        /// <summary>
        /// Creates a new country
        /// </summary>
        /// <param name="countryName">Name of the country (required, max 60 characters)</param>
        /// <param name="formalName">Full formal name as agreed by UN (required, max 60 characters)</param>
        /// <param name="continent">Name of the continent (required, max 30 characters)</param>
        /// <param name="region">Name of the region (required, max 30 characters)</param>
        /// <param name="subregion">Name of the subregion (required, max 30 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this country</param>
        /// <param name="isoAlpha3Code">3 letter ISO alphabetic code (optional, max 3 characters)</param>
        /// <param name="isoNumericCode">ISO numeric code (optional)</param>
        /// <param name="countryType">Type of country or administrative region (optional, max 20 characters)</param>
        /// <param name="latestRecordedPopulation">Latest population data (optional)</param>
        /// <param name="border">Geographic border data (optional)</param>
        public Country(
            string countryName,
            string formalName,
            string continent,
            string region,
            string subregion,
            int lastEditedBy,
            string? isoAlpha3Code = null,
            int? isoNumericCode = null,
            string? countryType = null,
            long? latestRecordedPopulation = null,
            byte[]? border = null)
        {
            ValidateCountryName(countryName);
            ValidateFormalName(formalName);
            ValidateContinent(continent);
            ValidateRegion(region);
            ValidateSubregion(subregion);
            ValidateIsoAlpha3Code(isoAlpha3Code);
            ValidateIsoNumericCode(isoNumericCode);
            ValidateCountryType(countryType);
            ValidatePopulation(latestRecordedPopulation);
            ValidateEditor(lastEditedBy);

            CountryName = countryName.Trim();
            FormalName = formalName.Trim();
            Continent = continent.Trim();
            Region = region.Trim();
            Subregion = subregion.Trim();
            IsoAlpha3Code = string.IsNullOrWhiteSpace(isoAlpha3Code) ? null : isoAlpha3Code.Trim().ToUpperInvariant();
            IsoNumericCode = isoNumericCode;
            CountryType = string.IsNullOrWhiteSpace(countryType) ? null : countryType.Trim();
            LatestRecordedPopulation = latestRecordedPopulation;
            Border = border;
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the country names
        /// </summary>
        /// <param name="newCountryName">New country name</param>
        /// <param name="newFormalName">New formal name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCountryNames(string newCountryName, string newFormalName, int editedBy)
        {
            ValidateCountryName(newCountryName);
            ValidateFormalName(newFormalName);
            ValidateEditor(editedBy);

            CountryName = newCountryName.Trim();
            FormalName = newFormalName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the geographic classification
        /// </summary>
        /// <param name="newContinent">New continent name</param>
        /// <param name="newRegion">New region name</param>
        /// <param name="newSubregion">New subregion name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateGeographicClassification(string newContinent, string newRegion, string newSubregion, int editedBy)
        {
            ValidateContinent(newContinent);
            ValidateRegion(newRegion);
            ValidateSubregion(newSubregion);
            ValidateEditor(editedBy);

            Continent = newContinent.Trim();
            Region = newRegion.Trim();
            Subregion = newSubregion.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the ISO codes
        /// </summary>
        /// <param name="newIsoAlpha3Code">New 3 letter ISO code</param>
        /// <param name="newIsoNumericCode">New ISO numeric code</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateIsoCodes(string? newIsoAlpha3Code, int? newIsoNumericCode, int editedBy)
        {
            ValidateIsoAlpha3Code(newIsoAlpha3Code);
            ValidateIsoNumericCode(newIsoNumericCode);
            ValidateEditor(editedBy);

            IsoAlpha3Code = string.IsNullOrWhiteSpace(newIsoAlpha3Code) ? null : newIsoAlpha3Code.Trim().ToUpperInvariant();
            IsoNumericCode = newIsoNumericCode;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the country type
        /// </summary>
        /// <param name="newCountryType">New country type</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCountryType(string? newCountryType, int editedBy)
        {
            ValidateCountryType(newCountryType);
            ValidateEditor(editedBy);

            CountryType = string.IsNullOrWhiteSpace(newCountryType) ? null : newCountryType.Trim();
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
            if (CountryId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            CountryId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        // Validation methods
        private static void ValidateCountryName(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
                throw new ArgumentException("Country name cannot be null or empty.", nameof(countryName));
            
            if (countryName.Length > 60)
                throw new ArgumentException("Country name cannot exceed 60 characters.", nameof(countryName));
        }

        private static void ValidateFormalName(string formalName)
        {
            if (string.IsNullOrWhiteSpace(formalName))
                throw new ArgumentException("Formal name cannot be null or empty.", nameof(formalName));
            
            if (formalName.Length > 60)
                throw new ArgumentException("Formal name cannot exceed 60 characters.", nameof(formalName));
        }

        private static void ValidateContinent(string continent)
        {
            if (string.IsNullOrWhiteSpace(continent))
                throw new ArgumentException("Continent cannot be null or empty.", nameof(continent));
            
            if (continent.Length > 30)
                throw new ArgumentException("Continent cannot exceed 30 characters.", nameof(continent));
        }

        private static void ValidateRegion(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException("Region cannot be null or empty.", nameof(region));
            
            if (region.Length > 30)
                throw new ArgumentException("Region cannot exceed 30 characters.", nameof(region));
        }

        private static void ValidateSubregion(string subregion)
        {
            if (string.IsNullOrWhiteSpace(subregion))
                throw new ArgumentException("Subregion cannot be null or empty.", nameof(subregion));
            
            if (subregion.Length > 30)
                throw new ArgumentException("Subregion cannot exceed 30 characters.", nameof(subregion));
        }

        private static void ValidateIsoAlpha3Code(string? isoAlpha3Code)
        {
            if (!string.IsNullOrEmpty(isoAlpha3Code))
            {
                if (isoAlpha3Code.Length != 3)
                    throw new ArgumentException("ISO Alpha-3 code must be exactly 3 characters.", nameof(isoAlpha3Code));
                
                if (!isoAlpha3Code.All(char.IsLetter))
                    throw new ArgumentException("ISO Alpha-3 code must contain only letters.", nameof(isoAlpha3Code));
            }
        }

        private static void ValidateIsoNumericCode(int? isoNumericCode)
        {
            if (isoNumericCode.HasValue && (isoNumericCode.Value < 1 || isoNumericCode.Value > 999))
                throw new ArgumentException("ISO numeric code must be between 1 and 999.", nameof(isoNumericCode));
        }

        private static void ValidateCountryType(string? countryType)
        {
            if (!string.IsNullOrEmpty(countryType) && countryType.Length > 20)
                throw new ArgumentException("Country type cannot exceed 20 characters.", nameof(countryType));
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
            return $"Country: {CountryName} ({IsoAlpha3Code}) - ID: {CountryId}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Country other && CountryId == other.CountryId;
        }

        public override int GetHashCode()
        {
            return CountryId.GetHashCode();
        }
    }
} 