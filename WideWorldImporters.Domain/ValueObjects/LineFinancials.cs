using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing the financial calculations for an invoice line
    /// This encapsulates pricing, tax calculations, profit analysis, and extended amounts
    /// </summary>
    public class LineFinancials : IEquatable<LineFinancials>
    {
        /// <summary>
        /// Unit price charged (nullable for promotional/free items)
        /// </summary>
        public decimal? UnitPrice { get; }

        /// <summary>
        /// Tax rate to be applied (as percentage)
        /// </summary>
        public decimal TaxRate { get; }

        /// <summary>
        /// Tax amount calculated
        /// </summary>
        public decimal TaxAmount { get; }

        /// <summary>
        /// Profit made on this line item at current cost price
        /// </summary>
        public decimal LineProfit { get; }

        /// <summary>
        /// Extended line price charged (total before tax)
        /// </summary>
        public decimal ExtendedPrice { get; }

        /// <summary>
        /// Quantity for this line (needed for financial calculations)
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// Creates new line financials with all calculated values
        /// </summary>
        /// <param name="quantity">Quantity of items</param>
        /// <param name="unitPrice">Unit price (can be null for free items)</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="taxAmount">Calculated tax amount</param>
        /// <param name="lineProfit">Calculated profit for this line</param>
        /// <param name="extendedPrice">Total line price before tax</param>
        public LineFinancials(int quantity, decimal? unitPrice, decimal taxRate, decimal taxAmount, decimal lineProfit, decimal extendedPrice)
        {
            ValidateQuantity(quantity);
            ValidateUnitPrice(unitPrice);
            ValidateTaxRate(taxRate);
            ValidateCalculatedAmounts(taxAmount, lineProfit, extendedPrice);
            ValidateFinancialConsistency(quantity, unitPrice, taxRate, taxAmount, extendedPrice);

            Quantity = quantity;
            UnitPrice = unitPrice;
            TaxRate = taxRate;
            TaxAmount = taxAmount;
            LineProfit = lineProfit;
            ExtendedPrice = extendedPrice;
        }

        /// <summary>
        /// Creates line financials by calculating values from unit price and quantity
        /// </summary>
        /// <param name="quantity">Quantity of items</param>
        /// <param name="unitPrice">Unit price per item</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="costPrice">Cost price for profit calculation</param>
        /// <returns>New line financials with calculated values</returns>
        public static LineFinancials Calculate(int quantity, decimal? unitPrice, decimal taxRate, decimal costPrice = 0m)
        {
            ValidateQuantity(quantity);
            ValidateUnitPrice(unitPrice);
            ValidateTaxRate(taxRate);
            ValidateCostPrice(costPrice);

            var effectiveUnitPrice = unitPrice ?? 0m;
            var extendedPrice = effectiveUnitPrice * quantity;
            var taxAmount = extendedPrice * (taxRate / 100m);
            var lineProfit = extendedPrice - (costPrice * quantity);

            return new LineFinancials(quantity, unitPrice, taxRate, taxAmount, lineProfit, extendedPrice);
        }

        /// <summary>
        /// Creates line financials for free/promotional items
        /// </summary>
        /// <param name="quantity">Quantity of free items</param>
        /// <param name="costPrice">Cost price for profit calculation</param>
        /// <returns>Line financials for free items</returns>
        public static LineFinancials CreateFreeItems(int quantity, decimal costPrice = 0m)
        {
            return Calculate(quantity, unitPrice: null, taxRate: 0m, costPrice);
        }

        /// <summary>
        /// Total amount including tax
        /// </summary>
        public decimal TotalIncludingTax => ExtendedPrice + TaxAmount;

        /// <summary>
        /// Effective unit price (treating null as zero)
        /// </summary>
        public decimal EffectiveUnitPrice => UnitPrice ?? 0m;

        /// <summary>
        /// Indicates if this line represents free/promotional items
        /// </summary>
        public bool IsFreeItem => UnitPrice == null || UnitPrice == 0m;

        /// <summary>
        /// Indicates if this line is profitable
        /// </summary>
        public bool IsProfitable => LineProfit > 0m;

        /// <summary>
        /// Indicates if this line has a loss
        /// </summary>
        public bool HasLoss => LineProfit < 0m;

        /// <summary>
        /// Profit margin percentage (if applicable)
        /// </summary>
        public decimal? ProfitMarginPercentage
        {
            get
            {
                if (ExtendedPrice == 0m) return null;
                return (LineProfit / ExtendedPrice) * 100m;
            }
        }

        /// <summary>
        /// Average profit per item
        /// </summary>
        public decimal ProfitPerItem => Quantity > 0 ? LineProfit / Quantity : 0m;

        /// <summary>
        /// Effective tax rate on the extended price
        /// </summary>
        public decimal EffectiveTaxRate => ExtendedPrice > 0m ? (TaxAmount / ExtendedPrice) * 100m : 0m;

        /// <summary>
        /// Creates a copy with updated quantity (recalculating totals)
        /// </summary>
        /// <param name="newQuantity">New quantity</param>
        /// <param name="costPrice">Cost price for profit recalculation</param>
        /// <returns>New line financials with updated quantity</returns>
        public LineFinancials WithQuantity(int newQuantity, decimal costPrice = 0m)
        {
            return Calculate(newQuantity, UnitPrice, TaxRate, costPrice);
        }

        /// <summary>
        /// Creates a copy with updated unit price (recalculating totals)
        /// </summary>
        /// <param name="newUnitPrice">New unit price</param>
        /// <param name="costPrice">Cost price for profit recalculation</param>
        /// <returns>New line financials with updated unit price</returns>
        public LineFinancials WithUnitPrice(decimal? newUnitPrice, decimal costPrice = 0m)
        {
            return Calculate(Quantity, newUnitPrice, TaxRate, costPrice);
        }

        // Validation methods
        private static void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }

        private static void ValidateUnitPrice(decimal? unitPrice)
        {
            if (unitPrice.HasValue && unitPrice.Value < 0)
                throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        private static void ValidateTaxRate(decimal taxRate)
        {
            if (taxRate < 0)
                throw new ArgumentException("Tax rate cannot be negative.", nameof(taxRate));
            
            if (taxRate > 100)
                throw new ArgumentException("Tax rate cannot exceed 100%.", nameof(taxRate));
        }

        private static void ValidateCostPrice(decimal costPrice)
        {
            if (costPrice < 0)
                throw new ArgumentException("Cost price cannot be negative.", nameof(costPrice));
        }

        private static void ValidateCalculatedAmounts(decimal taxAmount, decimal lineProfit, decimal extendedPrice)
        {
            if (taxAmount < 0)
                throw new ArgumentException("Tax amount cannot be negative.", nameof(taxAmount));
            
            if (extendedPrice < 0)
                throw new ArgumentException("Extended price cannot be negative.", nameof(extendedPrice));
            
            // LineProfit can be negative (loss), so no validation needed
        }

        private static void ValidateFinancialConsistency(int quantity, decimal? unitPrice, decimal taxRate, decimal taxAmount, decimal extendedPrice)
        {
            var expectedExtendedPrice = (unitPrice ?? 0m) * quantity;
            var expectedTaxAmount = expectedExtendedPrice * (taxRate / 100m);

            // Allow for small rounding differences
            var tolerance = 0.01m;

            if (Math.Abs(extendedPrice - expectedExtendedPrice) > tolerance)
                throw new ArgumentException("Extended price is inconsistent with unit price and quantity.");
            
            if (Math.Abs(taxAmount - expectedTaxAmount) > tolerance)
                throw new ArgumentException("Tax amount is inconsistent with extended price and tax rate.");
        }

        public bool Equals(LineFinancials? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Quantity == other.Quantity &&
                   UnitPrice == other.UnitPrice &&
                   TaxRate == other.TaxRate &&
                   TaxAmount == other.TaxAmount &&
                   LineProfit == other.LineProfit &&
                   ExtendedPrice == other.ExtendedPrice;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LineFinancials);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Quantity, UnitPrice, TaxRate, TaxAmount, LineProfit, ExtendedPrice);
        }

        public override string ToString()
        {
            var unitPriceText = UnitPrice?.ToString("C") ?? "FREE";
            var profitText = IsProfitable ? $"+{LineProfit:C}" : LineProfit.ToString("C");
            return $"{Quantity} × {unitPriceText} = {ExtendedPrice:C} (Tax: {TaxAmount:C}, Profit: {profitText})";
        }

        public static bool operator ==(LineFinancials? left, LineFinancials? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LineFinancials? left, LineFinancials? right)
        {
            return !Equals(left, right);
        }
    }
} 