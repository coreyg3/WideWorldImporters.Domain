using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing an address
    /// </summary>
    public class Address : IEquatable<Address>
    {
        /// <summary>
        /// First address line (required, max 60 characters)
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string AddressLine1 { get; }

        /// <summary>
        /// Second address line (optional, max 60 characters)
        /// </summary>
        [MaxLength(60)]
        public string? AddressLine2 { get; }

        /// <summary>
        /// Postal code (required, max 10 characters)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string PostalCode { get; }

        /// <summary>
        /// City ID reference
        /// </summary>
        public int CityId { get; }

        /// <summary>
        /// Creates a new address
        /// </summary>
        /// <param name="addressLine1">First address line (required)</param>
        /// <param name="postalCode">Postal code (required)</param>
        /// <param name="cityId">City ID reference</param>
        /// <param name="addressLine2">Second address line (optional)</param>
        public Address(string addressLine1, string postalCode, int cityId, string? addressLine2 = null)
        {
            if (string.IsNullOrWhiteSpace(addressLine1))
                throw new ArgumentException("Address line 1 cannot be null or empty.", nameof(addressLine1));
            
            if (addressLine1.Length > 60)
                throw new ArgumentException("Address line 1 cannot exceed 60 characters.", nameof(addressLine1));

            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("Postal code cannot be null or empty.", nameof(postalCode));
            
            if (postalCode.Length > 10)
                throw new ArgumentException("Postal code cannot exceed 10 characters.", nameof(postalCode));

            if (!string.IsNullOrEmpty(addressLine2) && addressLine2.Length > 60)
                throw new ArgumentException("Address line 2 cannot exceed 60 characters.", nameof(addressLine2));

            if (cityId <= 0)
                throw new ArgumentException("City ID must be a valid reference.", nameof(cityId));

            AddressLine1 = addressLine1.Trim();
            AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
            PostalCode = postalCode.Trim();
            CityId = cityId;
        }

        public bool Equals(Address? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return AddressLine1 == other.AddressLine1 &&
                   AddressLine2 == other.AddressLine2 &&
                   PostalCode == other.PostalCode &&
                   CityId == other.CityId;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Address);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AddressLine1, AddressLine2, PostalCode, CityId);
        }

        public override string ToString()
        {
            var address = AddressLine1;
            if (!string.IsNullOrEmpty(AddressLine2))
                address += $", {AddressLine2}";
            address += $", {PostalCode}";
            return address;
        }

        public static bool operator ==(Address? left, Address? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address? left, Address? right)
        {
            return !Equals(left, right);
        }
    }
} 