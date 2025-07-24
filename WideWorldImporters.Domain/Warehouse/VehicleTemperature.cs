using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Represents a temperature sensor reading from a delivery vehicle's chiller system
    /// This entity handles high-volume IoT data for cold chain compliance and delivery logistics
    /// </summary>
    public class VehicleTemperature
    {
        /// <summary>
        /// Unique identifier for this temperature reading
        /// </summary>
        public long VehicleTemperatureId { get; private set; }

        /// <summary>
        /// Vehicle registration number/license plate
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string VehicleRegistration { get; private set; } = string.Empty;

        /// <summary>
        /// Identifier for the specific chiller sensor in the vehicle
        /// </summary>
        public int ChillerSensorNumber { get; private set; }

        /// <summary>
        /// Timestamp when this temperature reading was recorded
        /// </summary>
        public DateTime RecordedWhen { get; private set; }

        /// <summary>
        /// Temperature reading in degrees Celsius
        /// </summary>
        public decimal Temperature { get; private set; }

        /// <summary>
        /// Complete sensor data in text format (optional, max 1000 characters)
        /// </summary>
        [MaxLength(1000)]
        public string? FullSensorData { get; private set; }

        /// <summary>
        /// Indicates if the sensor data has been compressed to save storage
        /// </summary>
        public bool IsCompressed { get; private set; }

        /// <summary>
        /// Compressed sensor data in binary format (optional)
        /// </summary>
        public byte[]? CompressedSensorData { get; private set; }

        // Private parameterless constructor for EF Core
        private VehicleTemperature() { }

        /// <summary>
        /// Creates a new vehicle temperature reading
        /// </summary>
        /// <param name="vehicleRegistration">Vehicle registration/license plate</param>
        /// <param name="chillerSensorNumber">Chiller sensor identifier</param>
        /// <param name="recordedWhen">When the reading was taken</param>
        /// <param name="temperature">Temperature reading in Celsius</param>
        /// <param name="fullSensorData">Complete sensor data (optional)</param>
        /// <param name="isCompressed">Whether data is compressed</param>
        /// <param name="compressedSensorData">Compressed binary data (optional)</param>
        public VehicleTemperature(
            string vehicleRegistration,
            int chillerSensorNumber,
            DateTime recordedWhen,
            decimal temperature,
            string? fullSensorData = null,
            bool isCompressed = false,
            byte[]? compressedSensorData = null)
        {
            ValidateVehicleRegistration(vehicleRegistration);
            ValidateChillerSensorNumber(chillerSensorNumber);
            ValidateRecordedWhen(recordedWhen);
            ValidateTemperature(temperature);
            ValidateFullSensorData(fullSensorData);
            ValidateCompressionConsistency(isCompressed, fullSensorData, compressedSensorData);

            VehicleRegistration = vehicleRegistration.Trim().ToUpperInvariant();
            ChillerSensorNumber = chillerSensorNumber;
            RecordedWhen = recordedWhen;
            Temperature = temperature;
            FullSensorData = string.IsNullOrWhiteSpace(fullSensorData) ? null : fullSensorData.Trim();
            IsCompressed = isCompressed;
            CompressedSensorData = compressedSensorData;
        }

        /// <summary>
        /// Creates a current temperature reading (uses current UTC time)
        /// </summary>
        /// <param name="vehicleRegistration">Vehicle registration</param>
        /// <param name="chillerSensorNumber">Sensor number</param>
        /// <param name="temperature">Temperature reading</param>
        /// <param name="fullSensorData">Complete sensor data (optional)</param>
        /// <param name="isCompressed">Whether data is compressed</param>
        /// <param name="compressedSensorData">Compressed data (optional)</param>
        /// <returns>New temperature reading with current timestamp</returns>
        public static VehicleTemperature CreateCurrentReading(
            string vehicleRegistration,
            int chillerSensorNumber,
            decimal temperature,
            string? fullSensorData = null,
            bool isCompressed = false,
            byte[]? compressedSensorData = null)
        {
            return new VehicleTemperature(
                vehicleRegistration,
                chillerSensorNumber,
                DateTime.UtcNow,
                temperature,
                fullSensorData,
                isCompressed,
                compressedSensorData);
        }

        /// <summary>
        /// Creates a temperature reading with compressed sensor data
        /// </summary>
        /// <param name="vehicleRegistration">Vehicle registration</param>
        /// <param name="chillerSensorNumber">Sensor number</param>
        /// <param name="recordedWhen">Recording timestamp</param>
        /// <param name="temperature">Temperature reading</param>
        /// <param name="compressedData">Compressed sensor data</param>
        /// <returns>New temperature reading with compressed data</returns>
        public static VehicleTemperature CreateWithCompressedData(
            string vehicleRegistration,
            int chillerSensorNumber,
            DateTime recordedWhen,
            decimal temperature,
            byte[] compressedData)
        {
            return new VehicleTemperature(
                vehicleRegistration,
                chillerSensorNumber,
                recordedWhen,
                temperature,
                fullSensorData: null,
                isCompressed: true,
                compressedSensorData: compressedData);
        }

        /// <summary>
        /// Indicates if this temperature reading represents a cold chain compliance breach
        /// (outside safe temperature range for food transport)
        /// </summary>
        public bool IsComplianceBreach => Temperature < -30m || Temperature > 8m;

        /// <summary>
        /// Indicates if this is a critical temperature alert (extreme readings)
        /// </summary>
        public bool IsCriticalAlert => Temperature < -40m || Temperature > 20m;

        /// <summary>
        /// Indicates if this reading includes additional sensor data beyond temperature
        /// </summary>
        public bool HasExtendedSensorData => !string.IsNullOrEmpty(FullSensorData) || CompressedSensorData?.Length > 0;

        /// <summary>
        /// Indicates if this reading uses data compression for storage efficiency
        /// </summary>
        public bool UsesCompression => IsCompressed && CompressedSensorData?.Length > 0;

        /// <summary>
        /// Gets the age of this reading in minutes
        /// </summary>
        public double AgeInMinutes => (DateTime.UtcNow - RecordedWhen).TotalMinutes;

        /// <summary>
        /// Gets the age of this reading in hours
        /// </summary>
        public double AgeInHours => (DateTime.UtcNow - RecordedWhen).TotalHours;

        /// <summary>
        /// Indicates if this is a recent reading (within last 30 minutes)
        /// </summary>
        public bool IsRecentReading => AgeInMinutes <= 30;

        /// <summary>
        /// Gets the temperature category for compliance reporting
        /// </summary>
        public string TemperatureCategory
        {
            get
            {
                return Temperature switch
                {
                    < -30m => "FROZEN (Critical Low)",
                    >= -30m and <= -18m => "Frozen Storage",
                    > -18m and <= 0m => "Chilled",
                    > 0m and <= 8m => "Cold Chain Safe",
                    > 8m and <= 20m => "WARM (Breach)",
                    _ => "HOT (Critical High)"
                };
            }
        }

        /// <summary>
        /// Gets the compliance status for regulatory reporting
        /// </summary>
        public string ComplianceStatus
        {
            get
            {
                if (IsCriticalAlert) return "CRITICAL ALERT";
                if (IsComplianceBreach) return "COMPLIANCE BREACH";
                return "COMPLIANT";
            }
        }

        /// <summary>
        /// Gets the data storage efficiency summary
        /// </summary>
        public string DataStorageSummary
        {
            get
            {
                if (UsesCompression)
                {
                    var ratio = FullSensorData?.Length > 0 ? 
                        (double)(CompressedSensorData?.Length ?? 0) / FullSensorData.Length : 0;
                    return $"Compressed ({ratio:P0} of original)";
                }
                
                if (HasExtendedSensorData)
                    return $"Uncompressed ({FullSensorData?.Length ?? 0} chars)";
                    
                return "Temperature only";
            }
        }

        /// <summary>
        /// Gets a comprehensive reading summary for monitoring dashboards
        /// </summary>
        public string ReadingSummary
        {
            get
            {
                var parts = new List<string>
                {
                    $"{VehicleRegistration}",
                    $"Sensor {ChillerSensorNumber}",
                    $"{Temperature:F1}°C",
                    TemperatureCategory
                };

                if (IsComplianceBreach)
                    parts.Add("⚠️ BREACH");

                if (HasExtendedSensorData)
                    parts.Add("📊 Extended Data");

                return string.Join(" | ", parts);
            }
        }

        /// <summary>
        /// Indicates if this reading requires immediate attention
        /// </summary>
        public bool RequiresAttention => IsComplianceBreach || IsCriticalAlert;

        /// <summary>
        /// Gets the vehicle identifier (normalized registration)
        /// </summary>
        public string VehicleId => VehicleRegistration;

        /// <summary>
        /// Gets the sensor identifier combining vehicle and sensor number
        /// </summary>
        public string SensorId => $"{VehicleRegistration}-{ChillerSensorNumber:D2}";

        // Validation methods
        private static void ValidateVehicleRegistration(string vehicleRegistration)
        {
            if (string.IsNullOrWhiteSpace(vehicleRegistration))
                throw new ArgumentException("Vehicle registration cannot be null or empty.", nameof(vehicleRegistration));
            
            if (vehicleRegistration.Length > 20)
                throw new ArgumentException("Vehicle registration cannot exceed 20 characters.", nameof(vehicleRegistration));
        }

        private static void ValidateChillerSensorNumber(int chillerSensorNumber)
        {
            if (chillerSensorNumber <= 0)
                throw new ArgumentException("Chiller sensor number must be positive.", nameof(chillerSensorNumber));
        }

        private static void ValidateRecordedWhen(DateTime recordedWhen)
        {
            if (recordedWhen == default)
                throw new ArgumentException("Recorded when must be a valid date and time.", nameof(recordedWhen));

            // Allow some tolerance for future timestamps (5 minutes) to handle clock skew
            if (recordedWhen > DateTime.UtcNow.AddMinutes(5))
                throw new ArgumentException("Recorded when cannot be significantly in the future.", nameof(recordedWhen));
        }

        private static void ValidateTemperature(decimal temperature)
        {
            // Reasonable range for vehicle chiller systems
            if (temperature < -60m || temperature > 60m)
                throw new ArgumentException("Temperature must be within a reasonable range (-60°C to +60°C).", nameof(temperature));
        }

        private static void ValidateFullSensorData(string? fullSensorData)
        {
            if (!string.IsNullOrEmpty(fullSensorData) && fullSensorData.Length > 1000)
                throw new ArgumentException("Full sensor data cannot exceed 1000 characters.", nameof(fullSensorData));
        }

        private static void ValidateCompressionConsistency(bool isCompressed, string? fullSensorData, byte[]? compressedSensorData)
        {
            if (isCompressed && compressedSensorData == null)
                throw new ArgumentException("Compressed data must be provided when IsCompressed is true.", nameof(compressedSensorData));

            if (!isCompressed && compressedSensorData != null)
                throw new ArgumentException("Compressed data should not be provided when IsCompressed is false.", nameof(compressedSensorData));
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(long id)
        {
            if (VehicleTemperatureId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            VehicleTemperatureId = id;
        }

        public override string ToString()
        {
            return $"VehicleTemp {VehicleTemperatureId}: {ReadingSummary}";
        }

        public override bool Equals(object? obj)
        {
            return obj is VehicleTemperature other && VehicleTemperatureId == other.VehicleTemperatureId;
        }

        public override int GetHashCode()
        {
            return VehicleTemperatureId.GetHashCode();
        }
    }
} 