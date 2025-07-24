using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Purchasing
{
    /// <summary>
    /// Represents a supplier financial transaction in the accounts payable ledger
    /// This entity tracks all supplier-related financial activities including invoices, payments, and credits
    /// </summary>
    public class SupplierTransaction
    {
        /// <summary>
        /// Numeric ID used to refer to a supplier transaction within the database
        /// </summary>
        public int SupplierTransactionId { get; private set; }

        /// <summary>
        /// Supplier for this transaction
        /// </summary>
        public int SupplierId { get; private set; }

        /// <summary>
        /// Type of transaction (invoice, payment, credit note, etc.)
        /// </summary>
        public int TransactionTypeId { get; private set; }

        /// <summary>
        /// ID of a purchase order (for transactions associated with a purchase order)
        /// </summary>
        public int? PurchaseOrderId { get; private set; }

        /// <summary>
        /// ID of a payment method (for transactions involving payments)
        /// </summary>
        public int? PaymentMethodId { get; private set; }

        /// <summary>
        /// Invoice number for an invoice received from the supplier
        /// </summary>
        [MaxLength(20)]
        public string? SupplierInvoiceNumber { get; private set; }

        /// <summary>
        /// Date for the transaction
        /// </summary>
        public DateOnly TransactionDate { get; private set; }

        /// <summary>
        /// Financial amounts for this transaction (amounts, tax, outstanding balance)
        /// </summary>
        public TransactionFinancials Financials { get; private set; }

        /// <summary>
        /// Date that this transaction was finalized (if it has been)
        /// </summary>
        public DateOnly? FinalizationDate { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private SupplierTransaction() { Financials = null!; }

        /// <summary>
        /// Creates a new supplier transaction
        /// </summary>
        /// <param name="supplierId">Supplier ID reference</param>
        /// <param name="transactionTypeId">Transaction type ID reference</param>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <param name="financials">Transaction financial amounts</param>
        /// <param name="lastEditedBy">ID of the person creating this transaction</param>
        /// <param name="purchaseOrderId">Optional purchase order ID for purchase order-related transactions</param>
        /// <param name="paymentMethodId">Optional payment method ID for payment transactions</param>
        /// <param name="supplierInvoiceNumber">Optional supplier invoice number</param>
        public SupplierTransaction(
            int supplierId,
            int transactionTypeId,
            DateOnly transactionDate,
            TransactionFinancials financials,
            int lastEditedBy,
            int? purchaseOrderId = null,
            int? paymentMethodId = null,
            string? supplierInvoiceNumber = null)
        {
            ValidateSupplierId(supplierId);
            ValidateTransactionTypeId(transactionTypeId);
            ValidateTransactionDate(transactionDate);
            ValidateEditor(lastEditedBy);
            ValidateSupplierInvoiceNumber(supplierInvoiceNumber);
            ValidateTransactionConsistency(purchaseOrderId, paymentMethodId, financials);

            SupplierId = supplierId;
            TransactionTypeId = transactionTypeId;
            TransactionDate = transactionDate;
            Financials = financials ?? throw new ArgumentNullException(nameof(financials));
            PurchaseOrderId = purchaseOrderId;
            PaymentMethodId = paymentMethodId;
            SupplierInvoiceNumber = string.IsNullOrWhiteSpace(supplierInvoiceNumber) ? null : supplierInvoiceNumber.Trim();
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an invoice transaction from a supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be invoice type)</param>
        /// <param name="supplierInvoiceNumber">Supplier's invoice number</param>
        /// <param name="transactionDate">Transaction date</param>
        /// <param name="amountExcludingTax">Amount before tax</param>
        /// <param name="taxAmount">Tax amount</param>
        /// <param name="lastEditedBy">Person creating the transaction</param>
        /// <param name="purchaseOrderId">Optional purchase order ID this invoice relates to</param>
        /// <returns>New supplier invoice transaction</returns>
        public static SupplierTransaction CreateInvoiceTransaction(
            int supplierId,
            int transactionTypeId,
            string supplierInvoiceNumber,
            DateOnly transactionDate,
            decimal amountExcludingTax,
            decimal taxAmount,
            int lastEditedBy,
            int? purchaseOrderId = null)
        {
            if (string.IsNullOrWhiteSpace(supplierInvoiceNumber))
                throw new ArgumentException("Supplier invoice number is required for invoice transactions.", nameof(supplierInvoiceNumber));

            var transactionAmount = amountExcludingTax + taxAmount;
            var financials = new TransactionFinancials(amountExcludingTax, taxAmount, transactionAmount, transactionAmount);

            return new SupplierTransaction(
                supplierId,
                transactionTypeId,
                transactionDate,
                financials,
                lastEditedBy,
                purchaseOrderId: purchaseOrderId,
                supplierInvoiceNumber: supplierInvoiceNumber);
        }

        /// <summary>
        /// Creates a payment transaction to a supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be payment type)</param>
        /// <param name="paymentMethodId">Payment method ID</param>
        /// <param name="transactionDate">Payment date</param>
        /// <param name="paymentAmount">Payment amount</param>
        /// <param name="lastEditedBy">Person recording the payment</param>
        /// <param name="purchaseOrderId">Optional purchase order ID this payment is for</param>
        /// <param name="supplierInvoiceNumber">Optional supplier invoice number this payment is for</param>
        /// <returns>New payment transaction</returns>
        public static SupplierTransaction CreatePaymentTransaction(
            int supplierId,
            int transactionTypeId,
            int paymentMethodId,
            DateOnly transactionDate,
            decimal paymentAmount,
            int lastEditedBy,
            int? purchaseOrderId = null,
            string? supplierInvoiceNumber = null)
        {
            var financials = TransactionFinancials.CreatePayment(paymentAmount);

            return new SupplierTransaction(
                supplierId,
                transactionTypeId,
                transactionDate,
                financials,
                lastEditedBy,
                purchaseOrderId: purchaseOrderId,
                paymentMethodId: paymentMethodId,
                supplierInvoiceNumber: supplierInvoiceNumber);
        }

        /// <summary>
        /// Creates a credit note transaction from a supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be credit type)</param>
        /// <param name="transactionDate">Credit date</param>
        /// <param name="creditAmount">Credit amount (before tax)</param>
        /// <param name="taxRate">Tax rate for the credit</param>
        /// <param name="lastEditedBy">Person creating the credit</param>
        /// <param name="supplierInvoiceNumber">Optional supplier credit note number</param>
        /// <param name="purchaseOrderId">Optional purchase order ID this credit relates to</param>
        /// <returns>New credit transaction</returns>
        public static SupplierTransaction CreateCreditTransaction(
            int supplierId,
            int transactionTypeId,
            DateOnly transactionDate,
            decimal creditAmount,
            decimal taxRate,
            int lastEditedBy,
            string? supplierInvoiceNumber = null,
            int? purchaseOrderId = null)
        {
            var financials = TransactionFinancials.CreateCredit(creditAmount, taxRate);

            return new SupplierTransaction(
                supplierId,
                transactionTypeId,
                transactionDate,
                financials,
                lastEditedBy,
                purchaseOrderId: purchaseOrderId,
                supplierInvoiceNumber: supplierInvoiceNumber);
        }

        /// <summary>
        /// Updates the outstanding balance for this transaction
        /// </summary>
        /// <param name="newOutstandingBalance">New outstanding balance</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateOutstandingBalance(decimal newOutstandingBalance, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot update outstanding balance for a finalized transaction.");

            ValidateEditor(editedBy);

            Financials = Financials.WithOutstandingBalance(newOutstandingBalance);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Applies a payment to this transaction, reducing the outstanding balance
        /// </summary>
        /// <param name="paymentAmount">Payment amount to apply</param>
        /// <param name="editedBy">Person applying the payment</param>
        public void ApplyPayment(decimal paymentAmount, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot apply payment to a finalized transaction.");

            if (!Financials.HasOutstandingBalance)
                throw new InvalidOperationException("Transaction has no outstanding balance to apply payment to.");

            ValidateEditor(editedBy);

            Financials = Financials.ApplyPayment(paymentAmount);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Finalizes this transaction (invoices, credits and payments have been matched)
        /// </summary>
        /// <param name="finalizationDate">Date of finalization</param>
        /// <param name="editedBy">Person finalizing the transaction</param>
        public void FinalizeTransaction(DateOnly finalizationDate, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Transaction is already finalized.");

            if (finalizationDate < TransactionDate)
                throw new ArgumentException("Finalization date cannot be before transaction date.", nameof(finalizationDate));

            ValidateEditor(editedBy);

            FinalizationDate = finalizationDate;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Unfinalizes this transaction (allows further modifications)
        /// </summary>
        /// <param name="editedBy">Person unfinalizing the transaction</param>
        public void UnfinalizeTransaction(int editedBy)
        {
            if (!IsFinalized)
                throw new InvalidOperationException("Transaction is not finalized.");

            ValidateEditor(editedBy);

            FinalizationDate = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the payment method for this transaction
        /// </summary>
        /// <param name="newPaymentMethodId">New payment method ID</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdatePaymentMethod(int? newPaymentMethodId, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot update payment method for a finalized transaction.");

            ValidateEditor(editedBy);

            PaymentMethodId = newPaymentMethodId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Links this transaction to a purchase order
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID to link to</param>
        /// <param name="editedBy">Person making the link</param>
        public void LinkToPurchaseOrder(int purchaseOrderId, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot link purchase order for a finalized transaction.");

            if (purchaseOrderId <= 0)
                throw new ArgumentException("Purchase order ID must be a valid reference.", nameof(purchaseOrderId));

            ValidateEditor(editedBy);

            PurchaseOrderId = purchaseOrderId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the purchase order link from this transaction
        /// </summary>
        /// <param name="editedBy">Person removing the link</param>
        public void UnlinkFromPurchaseOrder(int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot unlink purchase order for a finalized transaction.");

            ValidateEditor(editedBy);

            PurchaseOrderId = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the supplier invoice number
        /// </summary>
        /// <param name="newSupplierInvoiceNumber">New supplier invoice number</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateSupplierInvoiceNumber(string? newSupplierInvoiceNumber, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot update supplier invoice number for a finalized transaction.");

            ValidateSupplierInvoiceNumber(newSupplierInvoiceNumber);
            ValidateEditor(editedBy);

            SupplierInvoiceNumber = string.IsNullOrWhiteSpace(newSupplierInvoiceNumber) ? null : newSupplierInvoiceNumber.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Is this transaction finalized (invoices, credits and payments have been matched)
        /// </summary>
        public bool IsFinalized => FinalizationDate.HasValue;

        /// <summary>
        /// Transaction amount (excluding tax)
        /// </summary>
        public decimal AmountExcludingTax => Financials.AmountExcludingTax;

        /// <summary>
        /// Tax amount calculated
        /// </summary>
        public decimal TaxAmount => Financials.TaxAmount;

        /// <summary>
        /// Transaction amount (including tax)
        /// </summary>
        public decimal TransactionAmount => Financials.TransactionAmount;

        /// <summary>
        /// Amount still outstanding for this transaction
        /// </summary>
        public decimal OutstandingBalance => Financials.OutstandingBalance;

        /// <summary>
        /// Indicates if this transaction is fully paid
        /// </summary>
        public bool IsFullyPaid => Financials.IsFullyPaid;

        /// <summary>
        /// Indicates if this transaction has any outstanding balance
        /// </summary>
        public bool HasOutstandingBalance => Financials.HasOutstandingBalance;

        /// <summary>
        /// Indicates if this is a payment transaction
        /// </summary>
        public bool IsPayment => Financials.IsPayment;

        /// <summary>
        /// Indicates if this is a credit transaction
        /// </summary>
        public bool IsCredit => Financials.IsCredit;

        /// <summary>
        /// Indicates if this is an invoice transaction
        /// </summary>
        public bool IsInvoice => Financials.IsInvoice;

        /// <summary>
        /// Gets the absolute transaction amount (always positive)
        /// </summary>
        public decimal AbsoluteTransactionAmount => Financials.AbsoluteTransactionAmount;

        /// <summary>
        /// Age of this transaction in days from transaction date
        /// </summary>
        public int AgeDays => DateOnly.FromDateTime(DateTime.Today).DayNumber - TransactionDate.DayNumber;

        /// <summary>
        /// Indicates if this transaction is overdue (over 30 days old with outstanding balance)
        /// </summary>
        public bool IsOverdue => HasOutstandingBalance && AgeDays > 30;

        /// <summary>
        /// Indicates if this transaction is linked to a purchase order
        /// </summary>
        public bool IsLinkedToPurchaseOrder => PurchaseOrderId.HasValue;

        /// <summary>
        /// Indicates if this transaction has a payment method specified
        /// </summary>
        public bool HasPaymentMethod => PaymentMethodId.HasValue;

        /// <summary>
        /// Indicates if this transaction has a supplier invoice number
        /// </summary>
        public bool HasSupplierInvoiceNumber => !string.IsNullOrWhiteSpace(SupplierInvoiceNumber);

        /// <summary>
        /// Gets the transaction category for reporting
        /// </summary>
        public string TransactionCategory => Financials.TransactionCategory;

        /// <summary>
        /// Gets the payment status description
        /// </summary>
        public string PaymentStatus => Financials.PaymentStatus;

        /// <summary>
        /// Gets the finalization status description
        /// </summary>
        public string FinalizationStatus => IsFinalized ? $"Finalized on {FinalizationDate}" : "Open";

        /// <summary>
        /// Gets a comprehensive status summary
        /// </summary>
        public string StatusSummary
        {
            get
            {
                var parts = new List<string> { TransactionCategory };
                
                if (PaymentStatus != "N/A")
                    parts.Add(PaymentStatus);
                    
                parts.Add(FinalizationStatus);
                
                if (IsOverdue)
                    parts.Add($"OVERDUE ({AgeDays} days)");
                    
                return string.Join(" | ", parts);
            }
        }

        // Validation methods
        private static void ValidateSupplierId(int supplierId)
        {
            if (supplierId <= 0)
                throw new ArgumentException("Supplier ID must be a valid supplier reference.", nameof(supplierId));
        }

        private static void ValidateTransactionTypeId(int transactionTypeId)
        {
            if (transactionTypeId <= 0)
                throw new ArgumentException("Transaction type ID must be a valid reference.", nameof(transactionTypeId));
        }

        private static void ValidateTransactionDate(DateOnly transactionDate)
        {
            if (transactionDate == default)
                throw new ArgumentException("Transaction date must be a valid date.", nameof(transactionDate));

            var today = DateOnly.FromDateTime(DateTime.Today);
            if (transactionDate > today.AddDays(1)) // Allow for time zone differences
                throw new ArgumentException("Transaction date cannot be in the future.", nameof(transactionDate));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("Editor must be a valid person ID.", nameof(editedBy));
        }

        private static void ValidateSupplierInvoiceNumber(string? supplierInvoiceNumber)
        {
            if (!string.IsNullOrEmpty(supplierInvoiceNumber) && supplierInvoiceNumber.Length > 20)
                throw new ArgumentException("Supplier invoice number cannot exceed 20 characters.", nameof(supplierInvoiceNumber));
        }

        private static void ValidateTransactionConsistency(int? purchaseOrderId, int? paymentMethodId, TransactionFinancials financials)
        {
            // Payments should have payment methods
            if (financials.IsPayment && !paymentMethodId.HasValue)
                throw new ArgumentException("Payment transactions must specify a payment method.", nameof(paymentMethodId));
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (SupplierTransactionId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            SupplierTransactionId = id;
        }

        public override string ToString()
        {
            var invoiceRef = HasSupplierInvoiceNumber ? $" (Invoice: {SupplierInvoiceNumber})" : "";
            return $"SupplierTransaction {SupplierTransactionId}: {TransactionCategory} {TransactionAmount:C} for Supplier {SupplierId}{invoiceRef} ({StatusSummary})";
        }

        public override bool Equals(object? obj)
        {
            return obj is SupplierTransaction other && SupplierTransactionId == other.SupplierTransactionId;
        }

        public override int GetHashCode()
        {
            return SupplierTransactionId.GetHashCode();
        }
    }
} 