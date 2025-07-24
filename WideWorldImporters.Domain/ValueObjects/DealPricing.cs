using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing the pricing strategy for a special deal
    /// Enforces exactly one of three mutually exclusive pricing options: fixed discount, percentage discount, or special unit price
    /// </summary>
    public class DealPricing : IEquatable<DealPricing>
    {
        /// <summary>
        /// Discount per unit to be applied to sale price (optional)
        /// </summary>
        public decimal? DiscountAmount { get; }

        /// <summary>
        /// Discount percentage per unit to be applied to sale price (optional)
        /// </summary>
        public decimal? DiscountPercentage { get; }

        /// <summary>
        /// Special price per unit to be applied instead of sale price (optional)
        /// </summary>
        public decimal? UnitPrice { get; }

        /// <summary>
        /// Creates new deal pricing with exactly one pricing strategy
        /// </summary>
        /// <param name="discountAmount">Fixed discount amount (nullable)</param>
        /// <param name="discountPercentage">Percentage discount (nullable)</param>
        /// <param name="unitPrice">Special unit price (nullable)</param>
        private DealPricing(decimal? discountAmount, decimal? discountPercentage, decimal? unitPrice)
        {
            ValidateExactlyOnePricingOption(discountAmount, discountPercentage, unitPrice);
            ValidatePricingValues(discountAmount, discountPercentage, unitPrice);

            DiscountAmount = discountAmount;
            DiscountPercentage = discountPercentage;
            UnitPrice = unitPrice;
        }

        /// <summary>
        /// Creates deal pricing with a fixed discount amount
        /// </summary>
        /// <param name="discountAmount">Fixed discount amount per unit</param>
        /// <returns>Deal pricing with fixed discount</returns>
        public static DealPricing CreateFixedDiscount(decimal discountAmount)
        {
            if (discountAmount <= 0)
                throw new ArgumentException("Discount amount must be positive.", nameof(discountAmount));

            return new DealPricing(discountAmount, null, null);
        }

        /// <summary>
        /// Creates deal pricing with a percentage discount
        /// </summary>
        /// <param name="discountPercentage">Percentage discount (e.g., 15.5 for 15.5%)</param>
        /// <returns>Deal pricing with percentage discount</returns>
        public static DealPricing CreatePercentageDiscount(decimal discountPercentage)
        {
            if (discountPercentage <= 0)
                throw new ArgumentException("Discount percentage must be positive.", nameof(discountPercentage));

            if (discountPercentage >= 100)
                throw new ArgumentException("Discount percentage must be less than 100%.", nameof(discountPercentage));

            return new DealPricing(null, discountPercentage, null);
        }

        /// <summary>
        /// Creates deal pricing with a special unit price
        /// </summary>
        /// <param name="unitPrice">Special unit price</param>
        /// <returns>Deal pricing with special unit price</returns>
        public static DealPricing CreateSpecialPrice(decimal unitPrice)
        {
            if (unitPrice <= 0)
                throw new ArgumentException("Unit price must be positive.", nameof(unitPrice));

            return new DealPricing(null, null, unitPrice);
        }

        /// <summary>
        /// Gets the pricing strategy type
        /// </summary>
        public DealPricingType PricingType
        {
            get
            {
                if (DiscountAmount.HasValue) return DealPricingType.FixedDiscount;
                if (DiscountPercentage.HasValue) return DealPricingType.PercentageDiscount;
                if (UnitPrice.HasValue) return DealPricingType.SpecialPrice;
                throw new InvalidOperationException("Deal pricing must have exactly one pricing option set.");
            }
        }

        /// <summary>
        /// Indicates if this is a fixed discount deal
        /// </summary>
        public bool IsFixedDiscount => DiscountAmount.HasValue;

        /// <summary>
        /// Indicates if this is a percentage discount deal
        /// </summary>
        public bool IsPercentageDiscount => DiscountPercentage.HasValue;

        /// <summary>
        /// Indicates if this is a special price deal
        /// </summary>
        public bool IsSpecialPrice => UnitPrice.HasValue;

        /// <summary>
        /// Calculates the effective unit price after applying the deal to a base price
        /// </summary>
        /// <param name="baseUnitPrice">Original unit price</param>
        /// <returns>Effective unit price after applying the deal</returns>
        public decimal CalculateEffectivePrice(decimal baseUnitPrice)
        {
            if (baseUnitPrice < 0)
                throw new ArgumentException("Base unit price cannot be negative.", nameof(baseUnitPrice));

            return PricingType switch
            {
                DealPricingType.FixedDiscount => Math.Max(0, baseUnitPrice - DiscountAmount!.Value),
                DealPricingType.PercentageDiscount => baseUnitPrice * (1 - DiscountPercentage!.Value / 100m),
                DealPricingType.SpecialPrice => UnitPrice!.Value,
                _ => throw new InvalidOperationException("Unknown pricing type.")
            };
        }

        /// <summary>
        /// Calculates the savings amount compared to the base price
        /// </summary>
        /// <param name="baseUnitPrice">Original unit price</param>
        /// <returns>Savings amount per unit</returns>
        public decimal CalculateSavings(decimal baseUnitPrice)
        {
            if (baseUnitPrice < 0)
                throw new ArgumentException("Base unit price cannot be negative.", nameof(baseUnitPrice));

            var effectivePrice = CalculateEffectivePrice(baseUnitPrice);
            return Math.Max(0, baseUnitPrice - effectivePrice);
        }

        /// <summary>
        /// Calculates the savings percentage compared to the base price
        /// </summary>
        /// <param name="baseUnitPrice">Original unit price</param>
        /// <returns>Savings percentage</returns>
        public decimal CalculateSavingsPercentage(decimal baseUnitPrice)
        {
            if (baseUnitPrice <= 0)
                return 0m;

            var savings = CalculateSavings(baseUnitPrice);
            return (savings / baseUnitPrice) * 100m;
        }

        /// <summary>
        /// Gets a description of the pricing strategy
        /// </summary>
        public string PricingDescription
        {
            get
            {
                return PricingType switch
                {
                    DealPricingType.FixedDiscount => $"{DiscountAmount:C} off",
                    DealPricingType.PercentageDiscount => $"{DiscountPercentage:F1}% off",
                    DealPricingType.SpecialPrice => $"Special price {UnitPrice:C}",
                    _ => "Unknown pricing"
                };
            }
        }

        /// <summary>
        /// Gets the numeric value of the pricing strategy (for comparison/sorting)
        /// </summary>
        public decimal PricingValue
        {
            get
            {
                return PricingType switch
                {
                    DealPricingType.FixedDiscount => DiscountAmount!.Value,
                    DealPricingType.PercentageDiscount => DiscountPercentage!.Value,
                    DealPricingType.SpecialPrice => UnitPrice!.Value,
                    _ => 0m
                };
            }
        }

        // Validation methods
        private static void ValidateExactlyOnePricingOption(decimal? discountAmount, decimal? discountPercentage, decimal? unitPrice)
        {
            var optionsSet = 0;
            if (discountAmount.HasValue) optionsSet++;
            if (discountPercentage.HasValue) optionsSet++;
            if (unitPrice.HasValue) optionsSet++;

            if (optionsSet != 1)
                throw new ArgumentException("Exactly one pricing option must be specified (DiscountAmount, DiscountPercentage, or UnitPrice).");
        }

        private static void ValidatePricingValues(decimal? discountAmount, decimal? discountPercentage, decimal? unitPrice)
        {
            if (discountAmount.HasValue && discountAmount.Value <= 0)
                throw new ArgumentException("Discount amount must be positive.", nameof(discountAmount));

            if (discountPercentage.HasValue)
            {
                if (discountPercentage.Value <= 0)
                    throw new ArgumentException("Discount percentage must be positive.", nameof(discountPercentage));
                if (discountPercentage.Value >= 100)
                    throw new ArgumentException("Discount percentage must be less than 100%.", nameof(discountPercentage));
            }

            if (unitPrice.HasValue && unitPrice.Value <= 0)
                throw new ArgumentException("Unit price must be positive.", nameof(unitPrice));
        }

        public bool Equals(DealPricing? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return DiscountAmount == other.DiscountAmount &&
                   DiscountPercentage == other.DiscountPercentage &&
                   UnitPrice == other.UnitPrice;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DealPricing);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DiscountAmount, DiscountPercentage, UnitPrice);
        }

        public override string ToString()
        {
            return PricingDescription;
        }

        public static bool operator ==(DealPricing? left, DealPricing? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DealPricing? left, DealPricing? right)
        {
            return !Equals(left, right);
        }
    }

    /// <summary>
    /// Enumeration of pricing strategy types for special deals
    /// </summary>
    public enum DealPricingType
    {
        /// <summary>
        /// Fixed amount discount per unit
        /// </summary>
        FixedDiscount,

        /// <summary>
        /// Percentage discount off the regular price
        /// </summary>
        PercentageDiscount,

        /// <summary>
        /// Special unit price (replaces regular price)
        /// </summary>
        SpecialPrice
    }
} 