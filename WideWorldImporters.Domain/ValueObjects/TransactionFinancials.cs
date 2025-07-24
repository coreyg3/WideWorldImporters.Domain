using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing the financial amounts for a customer transaction
    /// This encapsulates transaction amounts, tax calculations, and outstanding balance tracking
    /// </summary>
    public class TransactionFinancials : IEquatable<TransactionFinancials>
    {
        /// <summary>
        /// Transaction amount (excluding tax)
        /// </summary>
        public decimal AmountExcludingTax { get; }

        /// <summary>
        /// Tax amount calculated
        /// </summary>
        public decimal TaxAmount { get; }

        /// <summary>
        /// Transaction amount (including tax)
        /// </summary>
        public decimal TransactionAmount { get; }

        /// <summary>
        /// Amount still outstanding for this transaction
        /// </summary>
        public decimal OutstandingBalance { get; }

        /// <summary>
        /// Creates new transaction financials with all amounts
        /// </summary>
        /// <param name="amountExcludingTax">Amount before tax</param>
        /// <param name="taxAmount">Tax component</param>
        /// <param name="transactionAmount">Total amount including tax</param>
        /// <param name="outstandingBalance">Outstanding balance</param>
        public TransactionFinancials(decimal amountExcludingTax, decimal taxAmount, decimal transactionAmount, decimal outstandingBalance)
        {
            ValidateAmounts(amountExcludingTax, taxAmount, transactionAmount, outstandingBalance);
            ValidateFinancialConsistency(amountExcludingTax, taxAmount, transactionAmount);

            AmountExcludingTax = amountExcludingTax;
            TaxAmount = taxAmount;
            TransactionAmount = transactionAmount;
            OutstandingBalance = outstandingBalance;
        }

        /// <summary>
        /// Creates transaction financials by calculating tax amount
        /// </summary>
        /// <param name="amountExcludingTax">Amount before tax</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="outstandingBalance">Outstanding balance (defaults to full transaction amount)</param>
        /// <returns>New transaction financials with calculated tax</returns>
        public static TransactionFinancials Calculate(decimal amountExcludingTax, decimal taxRate, decimal? outstandingBalance = null)
        {
            ValidateAmountExcludingTax(amountExcludingTax);
            ValidateTaxRate(taxRate);

            var taxAmount = amountExcludingTax * (taxRate / 100m);
            var transactionAmount = amountExcludingTax + taxAmount;
            var outstanding = outstandingBalance ?? transactionAmount;

            return new TransactionFinancials(amountExcludingTax, taxAmount, transactionAmount, outstanding);
        }

        /// <summary>
        /// Creates transaction financials for a payment (negative amounts, zero outstanding)
        /// </summary>
        /// <param name="paymentAmount">Payment amount (will be made negative)</param>
        /// <returns>Transaction financials representing a payment</returns>
        public static TransactionFinancials CreatePayment(decimal paymentAmount)
        {
            ValidatePaymentAmount(paymentAmount);

            var negativeAmount = -Math.Abs(paymentAmount);
            return new TransactionFinancials(negativeAmount, 0m, negativeAmount, 0m);
        }

        /// <summary>
        /// Creates transaction financials for a credit note
        /// </summary>
        /// <param name="creditAmount">Credit amount (will be made negative)</param>
        /// <param name="taxRate">Tax rate for the credit</param>
        /// <returns>Transaction financials representing a credit</returns>
        public static TransactionFinancials CreateCredit(decimal creditAmount, decimal taxRate)
        {
            ValidateAmountExcludingTax(creditAmount);
            ValidateTaxRate(taxRate);

            var negativeCreditAmount = -Math.Abs(creditAmount);
            var negativeTaxAmount = negativeCreditAmount * (taxRate / 100m);
            var negativeTransactionAmount = negativeCreditAmount + negativeTaxAmount;

            return new TransactionFinancials(negativeCreditAmount, negativeTaxAmount, negativeTransactionAmount, 0m);
        }

        /// <summary>
        /// Effective tax rate on the transaction
        /// </summary>
        public decimal EffectiveTaxRate => AmountExcludingTax != 0m ? (TaxAmount / AmountExcludingTax) * 100m : 0m;

        /// <summary>
        /// Amount that has been paid (transaction amount minus outstanding balance)
        /// </summary>
        public decimal PaidAmount => TransactionAmount - OutstandingBalance;

        /// <summary>
        /// Percentage of the transaction that has been paid
        /// </summary>
        public decimal PaymentPercentage => TransactionAmount != 0m ? (PaidAmount / TransactionAmount) * 100m : 0m;

        /// <summary>
        /// Indicates if this transaction is fully paid
        /// </summary>
        public bool IsFullyPaid => OutstandingBalance == 0m;

        /// <summary>
        /// Indicates if this transaction has any outstanding balance
        /// </summary>
        public bool HasOutstandingBalance => OutstandingBalance > 0m;

        /// <summary>
        /// Indicates if this is a payment transaction (negative amounts)
        /// </summary>
        public bool IsPayment => TransactionAmount < 0m;

        /// <summary>
        /// Indicates if this is a credit transaction (negative amounts with tax)
        /// </summary>
        public bool IsCredit => TransactionAmount < 0m && TaxAmount < 0m;

        /// <summary>
        /// Indicates if this is an invoice transaction (positive amounts)
        /// </summary>
        public bool IsInvoice => TransactionAmount > 0m;

        /// <summary>
        /// Gets the absolute transaction amount (always positive)
        /// </summary>
        public decimal AbsoluteTransactionAmount => Math.Abs(TransactionAmount);

        /// <summary>
        /// Creates a copy with updated outstanding balance
        /// </summary>
        /// <param name="newOutstandingBalance">New outstanding balance</param>
        /// <returns>New transaction financials with updated balance</returns>
        public TransactionFinancials WithOutstandingBalance(decimal newOutstandingBalance)
        {
            ValidateOutstandingBalance(newOutstandingBalance, TransactionAmount);
            return new TransactionFinancials(AmountExcludingTax, TaxAmount, TransactionAmount, newOutstandingBalance);
        }

        /// <summary>
        /// Applies a payment to this transaction, reducing the outstanding balance
        /// </summary>
        /// <param name="paymentAmount">Payment amount to apply</param>
        /// <returns>New transaction financials with reduced outstanding balance</returns>
        public TransactionFinancials ApplyPayment(decimal paymentAmount)
        {
            if (paymentAmount < 0)
                throw new ArgumentException("Payment amount cannot be negative.", nameof(paymentAmount));

            if (paymentAmount > OutstandingBalance)
                throw new ArgumentException($"Payment amount ({paymentAmount:C}) cannot exceed outstanding balance ({OutstandingBalance:C}).", nameof(paymentAmount));

            var newOutstandingBalance = OutstandingBalance - paymentAmount;
            return WithOutstandingBalance(newOutstandingBalance);
        }

        /// <summary>
        /// Gets the transaction category for reporting
        /// </summary>
        public string TransactionCategory
        {
            get
            {
                if (IsPayment) return "Payment";
                if (IsCredit) return "Credit Note";
                if (IsInvoice) return "Invoice";
                return "Other";
            }
        }

        /// <summary>
        /// Gets the payment status description
        /// </summary>
        public string PaymentStatus
        {
            get
            {
                if (IsPayment || IsCredit) return "N/A";
                if (IsFullyPaid) return "Paid in Full";
                if (PaymentPercentage > 0) return $"Partially Paid ({PaymentPercentage:F1}%)";
                return "Unpaid";
            }
        }

        // Validation methods
        private static void ValidateAmountExcludingTax(decimal amountExcludingTax)
        {
            // Can be negative for credits, so no sign validation
        }

        private static void ValidateTaxRate(decimal taxRate)
        {
            if (taxRate < 0)
                throw new ArgumentException("Tax rate cannot be negative.", nameof(taxRate));
            
            if (taxRate > 100)
                throw new ArgumentException("Tax rate cannot exceed 100%.", nameof(taxRate));
        }

        private static void ValidatePaymentAmount(decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be positive.", nameof(paymentAmount));
        }

        private static void ValidateAmounts(decimal amountExcludingTax, decimal taxAmount, decimal transactionAmount, decimal outstandingBalance)
        {
            ValidateOutstandingBalance(outstandingBalance, transactionAmount);
        }

        private static void ValidateOutstandingBalance(decimal outstandingBalance, decimal transactionAmount)
        {
            if (outstandingBalance < 0)
                throw new ArgumentException("Outstanding balance cannot be negative.", nameof(outstandingBalance));

            // For invoices (positive amounts), outstanding balance cannot exceed transaction amount
            if (transactionAmount > 0 && outstandingBalance > transactionAmount)
                throw new ArgumentException("Outstanding balance cannot exceed transaction amount for invoices.", nameof(outstandingBalance));

            // For payments/credits (negative amounts), outstanding balance should be zero
            if (transactionAmount < 0 && outstandingBalance != 0)
                throw new ArgumentException("Outstanding balance must be zero for payments and credits.", nameof(outstandingBalance));
        }

        private static void ValidateFinancialConsistency(decimal amountExcludingTax, decimal taxAmount, decimal transactionAmount)
        {
            var expectedTransactionAmount = amountExcludingTax + taxAmount;
            var tolerance = 0.01m; // Allow for small rounding differences

            if (Math.Abs(transactionAmount - expectedTransactionAmount) > tolerance)
                throw new ArgumentException("Transaction amount is inconsistent with amount excluding tax and tax amount.");
        }

        public bool Equals(TransactionFinancials? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return AmountExcludingTax == other.AmountExcludingTax &&
                   TaxAmount == other.TaxAmount &&
                   TransactionAmount == other.TransactionAmount &&
                   OutstandingBalance == other.OutstandingBalance;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TransactionFinancials);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AmountExcludingTax, TaxAmount, TransactionAmount, OutstandingBalance);
        }

        public override string ToString()
        {
            var categoryText = TransactionCategory;
            var amountText = $"{TransactionAmount:C}";
            var statusText = PaymentStatus != "N/A" ? $" ({PaymentStatus})" : "";
            return $"{categoryText}: {amountText}{statusText}";
        }

        public static bool operator ==(TransactionFinancials? left, TransactionFinancials? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TransactionFinancials? left, TransactionFinancials? right)
        {
            return !Equals(left, right);
        }
    }
} 