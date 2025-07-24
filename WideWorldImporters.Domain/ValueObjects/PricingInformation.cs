using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing pricing information for stock items
    /// </summary>
    public class PricingInformation : IEquatable<PricingInformation>
    {
        /// <summary>
        /// Selling price (ex-tax) for one unit of this product
        /// </summary>
        public decimal UnitPrice { get; }

        /// <summary>
        /// Recommended retail price for this stock item (optional)
        /// </summary>
        public decimal? RecommendedRetailPrice { get; }

        /// <summary>
        /// Tax rate to be applied (as percentage, e.g., 10.5 for 10.5%)
        /// </summary>
        public decimal TaxRate { get; }

        /// <summary>
        /// Creates new pricing information
        /// </summary>
        /// <param name="unitPrice">Unit price (ex-tax)</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="recommendedRetailPrice">Recommended retail price (optional)</param>
        public PricingInformation(decimal unitPrice, decimal taxRate, decimal? recommendedRetailPrice = null)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

            if (taxRate < 0)
                throw new ArgumentException("Tax rate cannot be negative.", nameof(taxRate));

            if (recommendedRetailPrice.HasValue && recommendedRetailPrice.Value < 0)
                throw new ArgumentException("Recommended retail price cannot be negative.", nameof(recommendedRetailPrice));

            UnitPrice = unitPrice;
            TaxRate = taxRate;
            RecommendedRetailPrice = recommendedRetailPrice;
        }

        /// <summary>
        /// Calculates the unit price including tax
        /// </summary>
        public decimal UnitPriceIncludingTax => UnitPrice * (1 + TaxRate / 100);

        /// <summary>
        /// Gets the profit margin if recommended retail price is available
        /// </summary>
        public decimal? ProfitMarginPercentage
        {
            get
            {
                if (!RecommendedRetailPrice.HasValue || RecommendedRetailPrice.Value == 0)
                    return null;

                return ((RecommendedRetailPrice.Value - UnitPriceIncludingTax) / RecommendedRetailPrice.Value) * 100;
            }
        }

        public bool Equals(PricingInformation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return UnitPrice == other.UnitPrice &&
                   RecommendedRetailPrice == other.RecommendedRetailPrice &&
                   TaxRate == other.TaxRate;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PricingInformation);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UnitPrice, RecommendedRetailPrice, TaxRate);
        }

        public override string ToString()
        {
            var result = $"Unit: {UnitPrice:C} (Tax: {TaxRate}%)";
            if (RecommendedRetailPrice.HasValue)
                result += $", RRP: {RecommendedRetailPrice.Value:C}";
            return result;
        }

        public static bool operator ==(PricingInformation? left, PricingInformation? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PricingInformation? left, PricingInformation? right)
        {
            return !Equals(left, right);
        }
    }
} 