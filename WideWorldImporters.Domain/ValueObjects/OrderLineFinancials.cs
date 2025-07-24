using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing the basic financial information for an order line
    /// This handles simple pricing calculations for order processing (simpler than invoice billing)
    /// </summary>
    public class OrderLineFinancials : IEquatable<OrderLineFinancials>
    {
        /// <summary>
        /// Quantity to be supplied
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// Unit price to be charged (nullable for promotional/free items)
        /// </summary>
        public decimal? UnitPrice { get; }

        /// <summary>
        /// Tax rate to be applied (as percentage)
        /// </summary>
        public decimal TaxRate { get; }

        /// <summary>
        /// Creates new order line financials
        /// </summary>
        /// <param name="quantity">Quantity to be supplied</param>
        /// <param name="unitPrice">Unit price (can be null for free items)</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        public OrderLineFinancials(int quantity, decimal? unitPrice, decimal taxRate)
        {
            ValidateQuantity(quantity);
            ValidateUnitPrice(unitPrice);
            ValidateTaxRate(taxRate);

            Quantity = quantity;
            UnitPrice = unitPrice;
            TaxRate = taxRate;
        }

        /// <summary>
        /// Creates order line financials for standard items
        /// </summary>
        /// <param name="quantity">Quantity ordered</param>
        /// <param name="unitPrice">Unit price per item</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <returns>New order line financials</returns>
        public static OrderLineFinancials Create(int quantity, decimal unitPrice, decimal taxRate)
        {
            return new OrderLineFinancials(quantity, unitPrice, taxRate);
        }

        /// <summary>
        /// Creates order line financials for free/promotional items
        /// </summary>
        /// <param name="quantity">Quantity of free items</param>
        /// <returns>Order line financials for free items</returns>
        public static OrderLineFinancials CreateFreeItems(int quantity)
        {
            return new OrderLineFinancials(quantity, unitPrice: null, taxRate: 0m);
        }

        /// <summary>
        /// Effective unit price (treating null as zero)
        /// </summary>
        public decimal EffectiveUnitPrice => UnitPrice ?? 0m;

        /// <summary>
        /// Extended line price (quantity × unit price) before tax
        /// </summary>
        public decimal ExtendedPrice => EffectiveUnitPrice * Quantity;

        /// <summary>
        /// Tax amount calculated on the extended price
        /// </summary>
        public decimal TaxAmount => ExtendedPrice * (TaxRate / 100m);

        /// <summary>
        /// Total amount including tax
        /// </summary>
        public decimal TotalIncludingTax => ExtendedPrice + TaxAmount;

        /// <summary>
        /// Indicates if this line represents free/promotional items
        /// </summary>
        public bool IsFreeItem => UnitPrice == null || UnitPrice == 0m;

        /// <summary>
        /// Indicates if this line has taxable amount
        /// </summary>
        public bool IsTaxable => TaxRate > 0m && ExtendedPrice > 0m;

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
        /// Creates a copy with updated quantity
        /// </summary>
        /// <param name="newQuantity">New quantity</param>
        /// <returns>New order line financials with updated quantity</returns>
        public OrderLineFinancials WithQuantity(int newQuantity)
        {
            return new OrderLineFinancials(newQuantity, UnitPrice, TaxRate);
        }

        /// <summary>
        /// Creates a copy with updated unit price
        /// </summary>
        /// <param name="newUnitPrice">New unit price</param>
        /// <returns>New order line financials with updated unit price</returns>
        public OrderLineFinancials WithUnitPrice(decimal? newUnitPrice)
        {
            return new OrderLineFinancials(Quantity, newUnitPrice, TaxRate);
        }

        /// <summary>
        /// Creates a copy with updated tax rate
        /// </summary>
        /// <param name="newTaxRate">New tax rate</param>
        /// <returns>New order line financials with updated tax rate</returns>
        public OrderLineFinancials WithTaxRate(decimal newTaxRate)
        {
            return new OrderLineFinancials(Quantity, UnitPrice, newTaxRate);
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

        public bool Equals(OrderLineFinancials? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return Quantity == other.Quantity &&
                   UnitPrice == other.UnitPrice &&
                   TaxRate == other.TaxRate;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as OrderLineFinancials);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Quantity, UnitPrice, TaxRate);
        }

        public override string ToString()
        {
            var unitPriceText = UnitPrice?.ToString("C") ?? "FREE";
            return $"{Quantity} × {unitPriceText} = {ExtendedPrice:C} (Tax: {TaxAmount:C})";
        }

        public static bool operator ==(OrderLineFinancials? left, OrderLineFinancials? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OrderLineFinancials? left, OrderLineFinancials? right)
        {
            return !Equals(left, right);
        }
    }
} 