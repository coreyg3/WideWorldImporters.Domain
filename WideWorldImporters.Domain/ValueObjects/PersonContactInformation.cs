using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing contact information for a person
    /// </summary>
    public class PersonContactInformation : IEquatable<PersonContactInformation>
    {
        /// <summary>
        /// Phone number (optional, max 20 characters)
        /// </summary>
        [MaxLength(20)]
        public string? PhoneNumber { get; }

        /// <summary>
        /// Fax number (optional, max 20 characters)
        /// </summary>
        [MaxLength(20)]
        public string? FaxNumber { get; }

        /// <summary>
        /// Email address for this person (optional, max 256 characters)
        /// </summary>
        [MaxLength(256)]
        public string? EmailAddress { get; }

        /// <summary>
        /// Creates new person contact information
        /// </summary>
        /// <param name="phoneNumber">Phone number (optional)</param>
        /// <param name="faxNumber">Fax number (optional)</param>
        /// <param name="emailAddress">Email address (optional)</param>
        public PersonContactInformation(string? phoneNumber = null, string? faxNumber = null, string? emailAddress = null)
        {
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length > 20)
                throw new ArgumentException("Phone number cannot exceed 20 characters.", nameof(phoneNumber));

            if (!string.IsNullOrEmpty(faxNumber) && faxNumber.Length > 20)
                throw new ArgumentException("Fax number cannot exceed 20 characters.", nameof(faxNumber));

            if (!string.IsNullOrEmpty(emailAddress))
            {
                if (emailAddress.Length > 256)
                    throw new ArgumentException("Email address cannot exceed 256 characters.", nameof(emailAddress));

                // Basic email validation
                if (!emailAddress.Contains('@') || !emailAddress.Contains('.'))
                    throw new ArgumentException("Email address must be in a valid format.", nameof(emailAddress));
            }

            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
            FaxNumber = string.IsNullOrWhiteSpace(faxNumber) ? null : faxNumber.Trim();
            EmailAddress = string.IsNullOrWhiteSpace(emailAddress) ? null : emailAddress.Trim().ToLowerInvariant();
        }

        public bool Equals(PersonContactInformation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return PhoneNumber == other.PhoneNumber &&
                   FaxNumber == other.FaxNumber &&
                   EmailAddress == other.EmailAddress;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PersonContactInformation);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PhoneNumber, FaxNumber, EmailAddress);
        }

        public override string ToString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(PhoneNumber)) parts.Add($"Phone: {PhoneNumber}");
            if (!string.IsNullOrEmpty(FaxNumber)) parts.Add($"Fax: {FaxNumber}");
            if (!string.IsNullOrEmpty(EmailAddress)) parts.Add($"Email: {EmailAddress}");
            
            return parts.Any() ? string.Join(", ", parts) : "No contact information";
        }

        public static bool operator ==(PersonContactInformation? left, PersonContactInformation? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PersonContactInformation? left, PersonContactInformation? right)
        {
            return !Equals(left, right);
        }
    }
} 