using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a detail line from a customer invoice
    /// This entity handles sophisticated financial calculations including pricing, tax, and profit analysis
    /// </summary>
    public class InvoiceLine
    {
        /// <summary>
        /// Numeric ID used for reference to a line on an invoice within the database
        /// </summary>
        public int InvoiceLineId { get; private set; }

        /// <summary>
        /// Invoice that this line is associated with
        /// </summary>
        public int InvoiceId { get; private set; }

        /// <summary>
        /// Stock item for this invoice line
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Description of the item supplied (Usually the stock item name but can be overridden)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Type of package supplied
        /// </summary>
        public int PackageTypeId { get; private set; }

        /// <summary>
        /// Financial calculations for this line including pricing, tax, and profit
        /// </summary>
        public LineFinancials Financials { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private InvoiceLine() 
        {
            Financials = null!;
        }

        /// <summary>
        /// Creates a new invoice line
        /// </summary>
        /// <param name="invoiceId">Invoice ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="description">Description of the item (max 100 characters)</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="financials">Financial calculations for this line</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        public InvoiceLine(
            int invoiceId,
            int stockItemId,
            string description,
            int packageTypeId,
            LineFinancials financials,
            int lastEditedBy)
        {
            ValidateInvoiceId(invoiceId);
            ValidateStockItemId(stockItemId);
            ValidateDescription(description);
            ValidatePackageTypeId(packageTypeId);
            ValidateEditor(lastEditedBy);

            InvoiceId = invoiceId;
            StockItemId = stockItemId;
            Description = description.Trim();
            PackageTypeId = packageTypeId;
            Financials = financials ?? throw new ArgumentNullException(nameof(financials));
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new invoice line with calculated financials
        /// </summary>
        /// <param name="invoiceId">Invoice ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="description">Description of the item</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="quantity">Quantity supplied</param>
        /// <param name="unitPrice">Unit price charged (null for free items)</param>
        /// <param name="taxRate">Tax rate to be applied</param>
        /// <param name="costPrice">Cost price for profit calculation</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        /// <returns>New invoice line with calculated financials</returns>
        public static InvoiceLine Create(
            int invoiceId,
            int stockItemId,
            string description,
            int packageTypeId,
            int quantity,
            decimal? unitPrice,
            decimal taxRate,
            decimal costPrice,
            int lastEditedBy)
        {
            var financials = LineFinancials.Calculate(quantity, unitPrice, taxRate, costPrice);
            return new InvoiceLine(invoiceId, stockItemId, description, packageTypeId, financials, lastEditedBy);
        }

        /// <summary>
        /// Creates a new invoice line for free/promotional items
        /// </summary>
        /// <param name="invoiceId">Invoice ID reference</param>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="description">Description of the item</param>
        /// <param name="packageTypeId">Package type ID reference</param>
        /// <param name="quantity">Quantity of free items</param>
        /// <param name="costPrice">Cost price for profit calculation</param>
        /// <param name="lastEditedBy">ID of the person creating this line</param>
        /// <returns>New invoice line for free items</returns>
        public static InvoiceLine CreateFreeItem(
            int invoiceId,
            int stockItemId,
            string description,
            int packageTypeId,
            int quantity,
            decimal costPrice,
            int lastEditedBy)
        {
            var financials = LineFinancials.CreateFreeItems(quantity, costPrice);
            return new InvoiceLine(invoiceId, stockItemId, description, packageTypeId, financials, lastEditedBy);
        }

        /// <summary>
        /// Updates the quantity and recalculates financials
        /// </summary>
        /// <param name="newQuantity">New quantity</param>
        /// <param name="costPrice">Cost price for profit recalculation</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateQuantity(int newQuantity, decimal costPrice, int editedBy)
        {
            ValidateEditor(editedBy);

            Financials = Financials.WithQuantity(newQuantity, costPrice);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the unit price and recalculates financials
        /// </summary>
        /// <param name="newUnitPrice">New unit price (null for free items)</param>
        /// <param name="costPrice">Cost price for profit recalculation</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateUnitPrice(decimal? newUnitPrice, decimal costPrice, int editedBy)
        {
            ValidateEditor(editedBy);

            Financials = Financials.WithUnitPrice(newUnitPrice, costPrice);
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the description
        /// </summary>
        /// <param name="newDescription">New description</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateDescription(string newDescription, int editedBy)
        {
            ValidateDescription(newDescription);
            ValidateEditor(editedBy);

            Description = newDescription.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the package type
        /// </summary>
        /// <param name="newPackageTypeId">New package type ID</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePackageType(int newPackageTypeId, int editedBy)
        {
            ValidatePackageTypeId(newPackageTypeId);
            ValidateEditor(editedBy);

            PackageTypeId = newPackageTypeId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the complete financial information
        /// </summary>
        /// <param name="newFinancials">New financial calculations</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateFinancials(LineFinancials newFinancials, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newFinancials);
            ValidateEditor(editedBy);

            Financials = newFinancials;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Convenience properties delegating to financials
        /// </summary>
        public int Quantity => Financials.Quantity;
        public decimal? UnitPrice => Financials.UnitPrice;
        public decimal TaxRate => Financials.TaxRate;
        public decimal TaxAmount => Financials.TaxAmount;
        public decimal LineProfit => Financials.LineProfit;
        public decimal ExtendedPrice => Financials.ExtendedPrice;
        public decimal TotalIncludingTax => Financials.TotalIncludingTax;

        /// <summary>
        /// Business intelligence properties
        /// </summary>
        public bool IsFreeItem => Financials.IsFreeItem;
        public bool IsProfitable => Financials.IsProfitable;
        public bool HasLoss => Financials.HasLoss;
        public decimal? ProfitMarginPercentage => Financials.ProfitMarginPercentage;
        public decimal ProfitPerItem => Financials.ProfitPerItem;

        /// <summary>
        /// Gets a summary of the line performance
        /// </summary>
        public string PerformanceSummary
        {
            get
            {
                if (IsFreeItem) return "Promotional Item";
                if (IsProfitable) return $"Profitable ({ProfitMarginPercentage:F1}%)";
                if (HasLoss) return $"Loss ({ProfitMarginPercentage:F1}%)";
                return "Break Even";
            }
        }

        /// <summary>
        /// Gets the line value category for reporting
        /// </summary>
        public string ValueCategory
        {
            get
            {
                return ExtendedPrice switch
                {
                    0 => "Free Item",
                    <= 50 => "Low Value",
                    <= 500 => "Medium Value",
                    <= 2000 => "High Value",
                    _ => "Premium Value"
                };
            }
        }

        /// <summary>
        /// Indicates if this line should be flagged for review (loss or high value)
        /// </summary>
        public bool RequiresReview => HasLoss || ExtendedPrice > 2000m;

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (InvoiceLineId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            InvoiceLineId = id;
        }

        // Validation methods
        private static void ValidateInvoiceId(int invoiceId)
        {
            if (invoiceId <= 0)
                throw new ArgumentException("Invoice ID must be a valid reference.", nameof(invoiceId));
        }

        private static void ValidateStockItemId(int stockItemId)
        {
            if (stockItemId <= 0)
                throw new ArgumentException("Stock item ID must be a valid reference.", nameof(stockItemId));
        }

        private static void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty.", nameof(description));
            
            if (description.Length > 100)
                throw new ArgumentException("Description cannot exceed 100 characters.", nameof(description));
        }

        private static void ValidatePackageTypeId(int packageTypeId)
        {
            if (packageTypeId <= 0)
                throw new ArgumentException("Package type ID must be a valid reference.", nameof(packageTypeId));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            var performanceIndicator = IsProfitable ? "✓" : HasLoss ? "⚠" : "○";
            return $"InvoiceLine {InvoiceLineId}: {Description} - {Financials} {performanceIndicator}";
        }

        public override bool Equals(object? obj)
        {
            return obj is InvoiceLine other && InvoiceLineId != 0 && InvoiceLineId == other.InvoiceLineId;
        }

        public override int GetHashCode()
        {
            return InvoiceLineId.GetHashCode();
        }
    }
} 