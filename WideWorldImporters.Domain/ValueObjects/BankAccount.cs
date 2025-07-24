using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing bank account information (contains sensitive financial data)
    /// </summary>
    public class BankAccount : IEquatable<BankAccount>
    {
        /// <summary>
        /// Bank account name (i.e name on the account) - max 50 characters
        /// </summary>
        [MaxLength(50)]
        public string? BankAccountName { get; }

        /// <summary>
        /// Bank branch - max 50 characters
        /// </summary>
        [MaxLength(50)]
        public string? BankAccountBranch { get; }

        /// <summary>
        /// Bank account code (usually a numeric reference for the bank branch) - max 20 characters
        /// </summary>
        [MaxLength(20)]
        public string? BankAccountCode { get; }

        /// <summary>
        /// Bank account number - max 20 characters
        /// </summary>
        [MaxLength(20)]
        public string? BankAccountNumber { get; }

        /// <summary>
        /// Bank's international code (such as a SWIFT code) - max 20 characters
        /// </summary>
        [MaxLength(20)]
        public string? BankInternationalCode { get; }

        /// <summary>
        /// Creates new bank account information
        /// </summary>
        /// <param name="bankAccountName">Name on the account (optional)</param>
        /// <param name="bankAccountBranch">Bank branch (optional)</param>
        /// <param name="bankAccountCode">Bank account code (optional)</param>
        /// <param name="bankAccountNumber">Account number (optional)</param>
        /// <param name="bankInternationalCode">International code like SWIFT (optional)</param>
        public BankAccount(
            string? bankAccountName = null,
            string? bankAccountBranch = null,
            string? bankAccountCode = null,
            string? bankAccountNumber = null,
            string? bankInternationalCode = null)
        {
            if (!string.IsNullOrEmpty(bankAccountName) && bankAccountName.Length > 50)
                throw new ArgumentException("Bank account name cannot exceed 50 characters.", nameof(bankAccountName));

            if (!string.IsNullOrEmpty(bankAccountBranch) && bankAccountBranch.Length > 50)
                throw new ArgumentException("Bank account branch cannot exceed 50 characters.", nameof(bankAccountBranch));

            if (!string.IsNullOrEmpty(bankAccountCode) && bankAccountCode.Length > 20)
                throw new ArgumentException("Bank account code cannot exceed 20 characters.", nameof(bankAccountCode));

            if (!string.IsNullOrEmpty(bankAccountNumber) && bankAccountNumber.Length > 20)
                throw new ArgumentException("Bank account number cannot exceed 20 characters.", nameof(bankAccountNumber));

            if (!string.IsNullOrEmpty(bankInternationalCode) && bankInternationalCode.Length > 20)
                throw new ArgumentException("Bank international code cannot exceed 20 characters.", nameof(bankInternationalCode));

            BankAccountName = string.IsNullOrWhiteSpace(bankAccountName) ? null : bankAccountName.Trim();
            BankAccountBranch = string.IsNullOrWhiteSpace(bankAccountBranch) ? null : bankAccountBranch.Trim();
            BankAccountCode = string.IsNullOrWhiteSpace(bankAccountCode) ? null : bankAccountCode.Trim();
            BankAccountNumber = string.IsNullOrWhiteSpace(bankAccountNumber) ? null : bankAccountNumber.Trim();
            BankInternationalCode = string.IsNullOrWhiteSpace(bankInternationalCode) ? null : bankInternationalCode.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Gets whether this bank account has any information
        /// </summary>
        public bool HasAnyInformation =>
            !string.IsNullOrEmpty(BankAccountName) ||
            !string.IsNullOrEmpty(BankAccountBranch) ||
            !string.IsNullOrEmpty(BankAccountCode) ||
            !string.IsNullOrEmpty(BankAccountNumber) ||
            !string.IsNullOrEmpty(BankInternationalCode);

        public bool Equals(BankAccount? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return BankAccountName == other.BankAccountName &&
                   BankAccountBranch == other.BankAccountBranch &&
                   BankAccountCode == other.BankAccountCode &&
                   BankAccountNumber == other.BankAccountNumber &&
                   BankInternationalCode == other.BankInternationalCode;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as BankAccount);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BankAccountName, BankAccountBranch, BankAccountCode, BankAccountNumber, BankInternationalCode);
        }

        public override string ToString()
        {
            if (!HasAnyInformation)
                return "No bank account information";

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(BankAccountName)) parts.Add($"Name: {BankAccountName}");
            if (!string.IsNullOrEmpty(BankAccountBranch)) parts.Add($"Branch: {BankAccountBranch}");
            if (!string.IsNullOrEmpty(BankAccountCode)) parts.Add($"Code: {BankAccountCode}");
            if (!string.IsNullOrEmpty(BankInternationalCode)) parts.Add($"International: {BankInternationalCode}");
            
            // Don't show account number in ToString for security
            if (!string.IsNullOrEmpty(BankAccountNumber)) parts.Add("Account: [MASKED]");
            
            return string.Join(", ", parts);
        }

        public static bool operator ==(BankAccount? left, BankAccount? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BankAccount? left, BankAccount? right)
        {
            return !Equals(left, right);
        }
    }
} 