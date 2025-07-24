using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a customer financial transaction in the accounts receivable ledger
    /// This entity tracks all customer-related financial activities including invoices, payments, and credits
    /// </summary>
    public class CustomerTransaction
    {
        /// <summary>
        /// Numeric ID used to refer to a customer transaction within the database
        /// </summary>
        public int CustomerTransactionId { get; private set; }

        /// <summary>
        /// Customer for this transaction
        /// </summary>
        public int CustomerId { get; private set; }

        /// <summary>
        /// Type of transaction (invoice, payment, credit note, etc.)
        /// </summary>
        public int TransactionTypeId { get; private set; }

        /// <summary>
        /// ID of an invoice (for transactions associated with an invoice)
        /// </summary>
        public int? InvoiceId { get; private set; }

        /// <summary>
        /// ID of a payment method (for transactions involving payments)
        /// </summary>
        public int? PaymentMethodId { get; private set; }

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
        private CustomerTransaction() { Financials = null!; }

        /// <summary>
        /// Creates a new customer transaction
        /// </summary>
        /// <param name="customerId">Customer ID reference</param>
        /// <param name="transactionTypeId">Transaction type ID reference</param>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <param name="financials">Transaction financial amounts</param>
        /// <param name="lastEditedBy">ID of the person creating this transaction</param>
        /// <param name="invoiceId">Optional invoice ID for invoice-related transactions</param>
        /// <param name="paymentMethodId">Optional payment method ID for payment transactions</param>
        public CustomerTransaction(
            int customerId,
            int transactionTypeId,
            DateOnly transactionDate,
            TransactionFinancials financials,
            int lastEditedBy,
            int? invoiceId = null,
            int? paymentMethodId = null)
        {
            ValidateCustomerId(customerId);
            ValidateTransactionTypeId(transactionTypeId);
            ValidateTransactionDate(transactionDate);
            ValidateEditor(lastEditedBy);
            ValidateTransactionConsistency(invoiceId, paymentMethodId, financials);

            CustomerId = customerId;
            TransactionTypeId = transactionTypeId;
            TransactionDate = transactionDate;
            Financials = financials ?? throw new ArgumentNullException(nameof(financials));
            InvoiceId = invoiceId;
            PaymentMethodId = paymentMethodId;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an invoice transaction
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be invoice type)</param>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="transactionDate">Transaction date</param>
        /// <param name="amountExcludingTax">Amount before tax</param>
        /// <param name="taxAmount">Tax amount</param>
        /// <param name="lastEditedBy">Person creating the transaction</param>
        /// <returns>New invoice transaction</returns>
        public static CustomerTransaction CreateInvoiceTransaction(
            int customerId,
            int transactionTypeId,
            int invoiceId,
            DateOnly transactionDate,
            decimal amountExcludingTax,
            decimal taxAmount,
            int lastEditedBy)
        {
            var transactionAmount = amountExcludingTax + taxAmount;
            var financials = new TransactionFinancials(amountExcludingTax, taxAmount, transactionAmount, transactionAmount);

            return new CustomerTransaction(
                customerId,
                transactionTypeId,
                transactionDate,
                financials,
                lastEditedBy,
                invoiceId: invoiceId);
        }

        /// <summary>
        /// Creates a payment transaction
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be payment type)</param>
        /// <param name="paymentMethodId">Payment method ID</param>
        /// <param name="transactionDate">Payment date</param>
        /// <param name="paymentAmount">Payment amount</param>
        /// <param name="lastEditedBy">Person recording the payment</param>
        /// <param name="invoiceId">Optional invoice ID this payment is for</param>
        /// <returns>New payment transaction</returns>
        public static CustomerTransaction CreatePaymentTransaction(
            int customerId,
            int transactionTypeId,
            int paymentMethodId,
            DateOnly transactionDate,
            decimal paymentAmount,
            int lastEditedBy,
            int? invoiceId = null)
        {
            var financials = TransactionFinancials.CreatePayment(paymentAmount);

            return new CustomerTransaction(
                customerId,
                transactionTypeId,
                transactionDate,
                financials,
                lastEditedBy,
                invoiceId: invoiceId,
                paymentMethodId: paymentMethodId);
        }

        /// <summary>
        /// Creates a credit note transaction
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be credit type)</param>
        /// <param name="transactionDate">Credit date</param>
        /// <param name="creditAmount">Credit amount (before tax)</param>
        /// <param name="taxRate">Tax rate for the credit</param>
        /// <param name="lastEditedBy">Person creating the credit</param>
        /// <param name="invoiceId">Optional invoice ID this credit relates to</param>
        /// <returns>New credit transaction</returns>
        public static CustomerTransaction CreateCreditTransaction(
            int customerId,
            int transactionTypeId,
            DateOnly transactionDate,
            decimal creditAmount,
            decimal taxRate,
            int lastEditedBy,
            int? invoiceId = null)
        {
            var financials = TransactionFinancials.CreateCredit(creditAmount, taxRate);

            return new CustomerTransaction(
                customerId,
                transactionTypeId,
                transactionDate,
                financials,
                lastEditedBy,
                invoiceId: invoiceId);
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
        /// Links this transaction to an invoice
        /// </summary>
        /// <param name="invoiceId">Invoice ID to link to</param>
        /// <param name="editedBy">Person making the link</param>
        public void LinkToInvoice(int invoiceId, int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot link invoice for a finalized transaction.");

            if (invoiceId <= 0)
                throw new ArgumentException("Invoice ID must be a valid reference.", nameof(invoiceId));

            ValidateEditor(editedBy);

            InvoiceId = invoiceId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the invoice link from this transaction
        /// </summary>
        /// <param name="editedBy">Person removing the link</param>
        public void UnlinkFromInvoice(int editedBy)
        {
            if (IsFinalized)
                throw new InvalidOperationException("Cannot unlink invoice for a finalized transaction.");

            ValidateEditor(editedBy);

            InvoiceId = null;
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
        /// Indicates if this transaction is linked to an invoice
        /// </summary>
        public bool IsLinkedToInvoice => InvoiceId.HasValue;

        /// <summary>
        /// Indicates if this transaction has a payment method specified
        /// </summary>
        public bool HasPaymentMethod => PaymentMethodId.HasValue;

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
        private static void ValidateCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be a valid customer reference.", nameof(customerId));
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

        private static void ValidateTransactionConsistency(int? invoiceId, int? paymentMethodId, TransactionFinancials financials)
        {
            // Payments should have payment methods
            if (financials.IsPayment && !paymentMethodId.HasValue)
                throw new ArgumentException("Payment transactions must specify a payment method.", nameof(paymentMethodId));

            // Invoice transactions should reference an invoice
            if (financials.IsInvoice && !invoiceId.HasValue)
                throw new ArgumentException("Invoice transactions should reference an invoice.", nameof(invoiceId));
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (CustomerTransactionId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            CustomerTransactionId = id;
        }

        public override string ToString()
        {
            return $"CustomerTransaction {CustomerTransactionId}: {TransactionCategory} {TransactionAmount:C} for Customer {CustomerId} ({StatusSummary})";
        }

        public override bool Equals(object? obj)
        {
            return obj is CustomerTransaction other && CustomerTransactionId == other.CustomerTransactionId;
        }

        public override int GetHashCode()
        {
            return CustomerTransactionId.GetHashCode();
        }
    }
} 