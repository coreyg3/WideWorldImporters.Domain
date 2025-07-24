using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing contact information
    /// </summary>
    public class ContactInformation : IEquatable<ContactInformation>
    {
        /// <summary>
        /// Phone number (required, max 20 characters)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; }

        /// <summary>
        /// Fax number (required, max 20 characters)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string FaxNumber { get; }

        /// <summary>
        /// Website URL (required, max 256 characters)
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string WebsiteUrl { get; }

        /// <summary>
        /// Creates new contact information
        /// </summary>
        /// <param name="phoneNumber">Phone number (required)</param>
        /// <param name="faxNumber">Fax number (required)</param>
        /// <param name="websiteUrl">Website URL (required)</param>
        public ContactInformation(string phoneNumber, string faxNumber, string websiteUrl)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            
            if (phoneNumber.Length > 20)
                throw new ArgumentException("Phone number cannot exceed 20 characters.", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(faxNumber))
                throw new ArgumentException("Fax number cannot be null or empty.", nameof(faxNumber));
            
            if (faxNumber.Length > 20)
                throw new ArgumentException("Fax number cannot exceed 20 characters.", nameof(faxNumber));

            if (string.IsNullOrWhiteSpace(websiteUrl))
                throw new ArgumentException("Website URL cannot be null or empty.", nameof(websiteUrl));
            
            if (websiteUrl.Length > 256)
                throw new ArgumentException("Website URL cannot exceed 256 characters.", nameof(websiteUrl));

            // Basic URL validation
            if (!Uri.TryCreate(websiteUrl, UriKind.Absolute, out var uri) || 
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Website URL must be a valid HTTP or HTTPS URL.", nameof(websiteUrl));
            }

            PhoneNumber = phoneNumber.Trim();
            FaxNumber = faxNumber.Trim();
            WebsiteUrl = websiteUrl.Trim();
        }

        public bool Equals(ContactInformation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return PhoneNumber == other.PhoneNumber &&
                   FaxNumber == other.FaxNumber &&
                   WebsiteUrl == other.WebsiteUrl;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ContactInformation);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PhoneNumber, FaxNumber, WebsiteUrl);
        }

        public override string ToString()
        {
            return $"Phone: {PhoneNumber}, Fax: {FaxNumber}, Website: {WebsiteUrl}";
        }

        public static bool operator ==(ContactInformation? left, ContactInformation? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContactInformation? left, ContactInformation? right)
        {
            return !Equals(left, right);
        }
    }
} 