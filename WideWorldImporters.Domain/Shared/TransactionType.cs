using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Shared
{
    /// <summary>
    /// Represents types of customer, supplier, or stock transactions (i.e: invoice, credit note, etc.)
    /// </summary>
    public class TransactionType
    {
        /// <summary>
        /// Numeric ID used for reference to a transaction type within the database
        /// </summary>
        public int TransactionTypeId { get; private set; }

        /// <summary>
        /// Full name of the transaction type
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TransactionTypeName { get; private set; } = string.Empty;

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
        private TransactionType() { }

        /// <summary>
        /// Creates a new transaction type
        /// </summary>
        /// <param name="transactionTypeName">The transaction type name (required, max 50 characters)</param>
        /// <param name="lastEditedBy">ID of the person creating this transaction type</param>
        /// <exception cref="ArgumentException">Thrown when transaction type name is invalid</exception>
        public TransactionType(string transactionTypeName, int lastEditedBy)
        {
            if (string.IsNullOrWhiteSpace(transactionTypeName))
                throw new ArgumentException("Transaction type name cannot be null or empty.", nameof(transactionTypeName));
            
            if (transactionTypeName.Length > 50)
                throw new ArgumentException("Transaction type name cannot exceed 50 characters.", nameof(transactionTypeName));

            if (lastEditedBy <= 0)
                throw new ArgumentException("LastEditedBy must be a valid person ID.", nameof(lastEditedBy));

            TransactionTypeName = transactionTypeName.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the transaction type name
        /// </summary>
        /// <param name="newTransactionTypeName">The new transaction type name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        /// <exception cref="ArgumentException">Thrown when transaction type name is invalid</exception>
        public void UpdateTransactionTypeName(string newTransactionTypeName, int editedBy)
        {
            if (string.IsNullOrWhiteSpace(newTransactionTypeName))
                throw new ArgumentException("Transaction type name cannot be null or empty.", nameof(newTransactionTypeName));
            
            if (newTransactionTypeName.Length > 50)
                throw new ArgumentException("Transaction type name cannot exceed 50 characters.", nameof(newTransactionTypeName));

            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));

            TransactionTypeName = newTransactionTypeName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Sets the temporal table properties (typically called by infrastructure layer)
        /// </summary>
        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (TransactionTypeId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            TransactionTypeId = id;
        }

        public override string ToString()
        {
            return $"TransactionType: {TransactionTypeName} (ID: {TransactionTypeId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is TransactionType other && TransactionTypeId == other.TransactionTypeId;
        }

        public override int GetHashCode()
        {
            return TransactionTypeId.GetHashCode();
        }
    }
} 