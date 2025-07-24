using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Domain entity representing a temperature sensor reading from a cold room/warehouse
    /// This is an immutable time-series record for cold storage compliance tracking
    /// </summary>
    public class ColdRoomTemperature
    {
        /// <summary>
        /// Unique identifier for this temperature reading
        /// </summary>
        public long ColdRoomTemperatureId { get; private set; }

        /// <summary>
        /// Identifier for the specific cold room sensor
        /// </summary>
        public int ColdRoomSensorNumber { get; private set; }

        /// <summary>
        /// Timestamp when this temperature reading was recorded
        /// </summary>
        public DateTime RecordedWhen { get; private set; }

        /// <summary>
        /// Temperature reading in degrees Celsius
        /// </summary>
        public decimal Temperature { get; private set; }

        /// <summary>
        /// System-versioned temporal table start date
        /// </summary>
        public DateTime ValidFrom { get; private set; }

        /// <summary>
        /// System-versioned temporal table end date
        /// </summary>
        public DateTime ValidTo { get; private set; }

        // Private parameterless constructor for EF Core
        private ColdRoomTemperature() { }

        /// <summary>
        /// Creates a new cold room temperature reading
        /// This represents an immutable point-in-time measurement from a warehouse cold room
        /// </summary>
        /// <param name="coldRoomSensorNumber">Cold room sensor identifier</param>
        /// <param name="recordedWhen">When the reading was taken</param>
        /// <param name="temperature">Temperature reading in Celsius</param>
        public ColdRoomTemperature(
            int coldRoomSensorNumber,
            DateTime recordedWhen,
            decimal temperature)
        {
            ValidateColdRoomSensorNumber(coldRoomSensorNumber);
            ValidateRecordedWhen(recordedWhen);
            ValidateTemperature(temperature);

            ColdRoomSensorNumber = coldRoomSensorNumber;
            RecordedWhen = recordedWhen;
            Temperature = temperature;
        }

        /// <summary>
        /// Creates a temperature reading for the current time
        /// </summary>
        /// <param name="coldRoomSensorNumber">Cold room sensor identifier</param>
        /// <param name="temperature">Temperature reading in Celsius</param>
        /// <returns>New temperature reading with current timestamp</returns>
        public static ColdRoomTemperature CreateCurrentReading(int coldRoomSensorNumber, decimal temperature)
        {
            return new ColdRoomTemperature(coldRoomSensorNumber, DateTime.UtcNow, temperature);
        }

        /// <summary>
        /// Indicates if this reading represents a cold storage compliance breach
        /// Typical cold storage range is -30°C to 8°C for different product types
        /// </summary>
        public bool IsComplianceBreach => Temperature < -30 || Temperature > 8;

        /// <summary>
        /// Indicates if this reading suggests potential freezing conditions
        /// </summary>
        public bool IsFreezingCondition => Temperature <= 0;

        /// <summary>
        /// Gets the age of this reading
        /// </summary>
        public TimeSpan Age => DateTime.UtcNow - RecordedWhen;

        /// <summary>
        /// Indicates if this is a recent reading (within last hour)
        /// </summary>
        public bool IsRecentReading => Age.TotalHours <= 1;

        /// <summary>
        /// Categorizes the temperature reading for cold chain management
        /// </summary>
        public string TemperatureCategory
        {
            get
            {
                return Temperature switch
                {
                    <= -18 => "Frozen Storage",      // Deep freeze
                    <= 0 => "Freezer",              // Standard freezer
                    <= 4 => "Refrigerated",         // Standard refrigeration
                    <= 8 => "Cool Storage",         // Cool but not refrigerated
                    _ => "Temperature Breach"       // Outside acceptable range
                };
            }
        }

        // Internal methods for infrastructure layer
        internal void SetId(long id)
        {
            if (ColdRoomTemperatureId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            ColdRoomTemperatureId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        // Validation methods
        private static void ValidateColdRoomSensorNumber(int coldRoomSensorNumber)
        {
            if (coldRoomSensorNumber <= 0)
                throw new ArgumentException("Cold room sensor number must be positive.", nameof(coldRoomSensorNumber));
            
            if (coldRoomSensorNumber > 9999) // Reasonable upper bound for warehouse sensors
                throw new ArgumentException("Cold room sensor number exceeds reasonable range (1-9999).", nameof(coldRoomSensorNumber));
        }

        private static void ValidateRecordedWhen(DateTime recordedWhen)
        {
            if (recordedWhen == default)
                throw new ArgumentException("Recorded timestamp cannot be default value.", nameof(recordedWhen));
            
            if (recordedWhen > DateTime.UtcNow.AddMinutes(5)) // Allow small clock skew
                throw new ArgumentException("Recorded timestamp cannot be in the future.", nameof(recordedWhen));
            
            if (recordedWhen < DateTime.UtcNow.AddYears(-10)) // Reasonable historical limit
                throw new ArgumentException("Recorded timestamp is too far in the past.", nameof(recordedWhen));
        }

        private static void ValidateTemperature(decimal temperature)
        {
            if (temperature < -273.15m) // Absolute zero
                throw new ArgumentException("Temperature cannot be below absolute zero (-273.15°C).", nameof(temperature));
            
            if (temperature > 100m) // Reasonable upper bound for cold storage sensors
                throw new ArgumentException("Temperature reading appears unrealistic for cold storage (above 100°C).", nameof(temperature));
        }

        public override string ToString()
        {
            var breach = IsComplianceBreach ? " ⚠️ BREACH" : "";
            var category = $" ({TemperatureCategory})";
            return $"Cold Room Sensor {ColdRoomSensorNumber}: {Temperature:F2}°C{category}{breach} @ {RecordedWhen:yyyy-MM-dd HH:mm:ss}";
        }

        public override bool Equals(object? obj)
        {
            return obj is ColdRoomTemperature other && ColdRoomTemperatureId != 0 && ColdRoomTemperatureId == other.ColdRoomTemperatureId;
        }

        public override int GetHashCode()
        {
            return ColdRoomTemperatureId != 0 ? ColdRoomTemperatureId.GetHashCode() : base.GetHashCode();
        }
    }
} 