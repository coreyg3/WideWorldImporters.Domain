using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Sales
{
    /// <summary>
    /// Represents a special pricing deal with sophisticated targeting and temporal validity
    /// This entity manages marketing campaigns, customer-specific pricing, and promotional offers
    /// </summary>
    public class SpecialDeal
    {
        /// <summary>
        /// ID (sequence based) for a special deal
        /// </summary>
        public int SpecialDealId { get; private set; }

        /// <summary>
        /// Stock item that the deal applies to (if NULL, then only discounts are permitted not unit prices)
        /// </summary>
        public int? StockItemId { get; private set; }

        /// <summary>
        /// ID of the customer that the special pricing applies to (if NULL then all customers)
        /// </summary>
        public int? CustomerId { get; private set; }

        /// <summary>
        /// ID of the buying group that the special pricing applies to (optional)
        /// </summary>
        public int? BuyingGroupId { get; private set; }

        /// <summary>
        /// ID of the customer category that the special pricing applies to (optional)
        /// </summary>
        public int? CustomerCategoryId { get; private set; }

        /// <summary>
        /// ID of the stock group that the special pricing applies to (optional)
        /// </summary>
        public int? StockGroupId { get; private set; }

        /// <summary>
        /// Description of the special deal
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string DealDescription { get; private set; } = string.Empty;

        /// <summary>
        /// Date that the special pricing starts from
        /// </summary>
        public DateOnly StartDate { get; private set; }

        /// <summary>
        /// Date that the special pricing ends on
        /// </summary>
        public DateOnly EndDate { get; private set; }

        /// <summary>
        /// Pricing strategy for this deal (discount amount, percentage, or special price)
        /// </summary>
        public DealPricing Pricing { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private SpecialDeal() { Pricing = null!; }

        /// <summary>
        /// Creates a new special deal
        /// </summary>
        /// <param name="dealDescription">Description of the deal (max 30 characters)</param>
        /// <param name="startDate">Start date for the deal</param>
        /// <param name="endDate">End date for the deal</param>
        /// <param name="pricing">Pricing strategy for the deal</param>
        /// <param name="lastEditedBy">ID of the person creating this deal</param>
        /// <param name="stockItemId">Specific stock item (required for special price deals)</param>
        /// <param name="customerId">Specific customer (optional)</param>
        /// <param name="buyingGroupId">Buying group (optional)</param>
        /// <param name="customerCategoryId">Customer category (optional)</param>
        /// <param name="stockGroupId">Stock group (optional)</param>
        public SpecialDeal(
            string dealDescription,
            DateOnly startDate,
            DateOnly endDate,
            DealPricing pricing,
            int lastEditedBy,
            int? stockItemId = null,
            int? customerId = null,
            int? buyingGroupId = null,
            int? customerCategoryId = null,
            int? stockGroupId = null)
        {
            ValidateDealDescription(dealDescription);
            ValidateDateRange(startDate, endDate);
            ValidateEditor(lastEditedBy);
            ValidateTargetingOptions(customerId, buyingGroupId, customerCategoryId);
            ValidateStockTargetingOptions(stockItemId, stockGroupId);
            ValidateSpecialPriceRequiresStockItem(pricing, stockItemId);

            DealDescription = dealDescription.Trim();
            StartDate = startDate;
            EndDate = endDate;
            Pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
            StockItemId = stockItemId;
            CustomerId = customerId;
            BuyingGroupId = buyingGroupId;
            CustomerCategoryId = customerCategoryId;
            StockGroupId = stockGroupId;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a customer-specific deal
        /// </summary>
        /// <param name="customerId">Specific customer ID</param>
        /// <param name="dealDescription">Deal description</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="pricing">Pricing strategy</param>
        /// <param name="lastEditedBy">Person creating the deal</param>
        /// <param name="stockItemId">Specific stock item (optional)</param>
        /// <returns>New customer-specific deal</returns>
        public static SpecialDeal CreateCustomerDeal(
            int customerId,
            string dealDescription,
            DateOnly startDate,
            DateOnly endDate,
            DealPricing pricing,
            int lastEditedBy,
            int? stockItemId = null)
        {
            return new SpecialDeal(
                dealDescription,
                startDate,
                endDate,
                pricing,
                lastEditedBy,
                stockItemId: stockItemId,
                customerId: customerId);
        }

        /// <summary>
        /// Creates a buying group deal
        /// </summary>
        /// <param name="buyingGroupId">Buying group ID</param>
        /// <param name="dealDescription">Deal description</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="pricing">Pricing strategy</param>
        /// <param name="lastEditedBy">Person creating the deal</param>
        /// <param name="stockGroupId">Stock group (optional)</param>
        /// <returns>New buying group deal</returns>
        public static SpecialDeal CreateBuyingGroupDeal(
            int buyingGroupId,
            string dealDescription,
            DateOnly startDate,
            DateOnly endDate,
            DealPricing pricing,
            int lastEditedBy,
            int? stockGroupId = null)
        {
            return new SpecialDeal(
                dealDescription,
                startDate,
                endDate,
                pricing,
                lastEditedBy,
                buyingGroupId: buyingGroupId,
                stockGroupId: stockGroupId);
        }

        /// <summary>
        /// Creates a category-wide deal
        /// </summary>
        /// <param name="customerCategoryId">Customer category ID</param>
        /// <param name="dealDescription">Deal description</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="pricing">Pricing strategy</param>
        /// <param name="lastEditedBy">Person creating the deal</param>
        /// <param name="stockGroupId">Stock group (optional)</param>
        /// <returns>New category deal</returns>
        public static SpecialDeal CreateCategoryDeal(
            int customerCategoryId,
            string dealDescription,
            DateOnly startDate,
            DateOnly endDate,
            DealPricing pricing,
            int lastEditedBy,
            int? stockGroupId = null)
        {
            return new SpecialDeal(
                dealDescription,
                startDate,
                endDate,
                pricing,
                lastEditedBy,
                customerCategoryId: customerCategoryId,
                stockGroupId: stockGroupId);
        }

        /// <summary>
        /// Creates a stock item specific deal
        /// </summary>
        /// <param name="stockItemId">Stock item ID</param>
        /// <param name="dealDescription">Deal description</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="pricing">Pricing strategy</param>
        /// <param name="lastEditedBy">Person creating the deal</param>
        /// <returns>New stock item deal</returns>
        public static SpecialDeal CreateStockItemDeal(
            int stockItemId,
            string dealDescription,
            DateOnly startDate,
            DateOnly endDate,
            DealPricing pricing,
            int lastEditedBy)
        {
            return new SpecialDeal(
                dealDescription,
                startDate,
                endDate,
                pricing,
                lastEditedBy,
                stockItemId: stockItemId);
        }

        /// <summary>
        /// Updates the deal description
        /// </summary>
        /// <param name="newDescription">New description</param>
        /// <param name="editedBy">Person making the change</param>
        public void UpdateDescription(string newDescription, int editedBy)
        {
            ValidateDealDescription(newDescription);
            ValidateEditor(editedBy);

            DealDescription = newDescription.Trim();
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the deal date range
        /// </summary>
        /// <param name="newStartDate">New start date</param>
        /// <param name="newEndDate">New end date</param>
        /// <param name="editedBy">Person making the change</param>
        public void UpdateDateRange(DateOnly newStartDate, DateOnly newEndDate, int editedBy)
        {
            ValidateDateRange(newStartDate, newEndDate);
            ValidateEditor(editedBy);

            StartDate = newStartDate;
            EndDate = newEndDate;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the pricing strategy
        /// </summary>
        /// <param name="newPricing">New pricing strategy</param>
        /// <param name="editedBy">Person making the change</param>
        public void UpdatePricing(DealPricing newPricing, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newPricing);
            ValidateSpecialPriceRequiresStockItem(newPricing, StockItemId);
            ValidateEditor(editedBy);

            Pricing = newPricing;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Extends the deal end date
        /// </summary>
        /// <param name="newEndDate">New end date (must be after current end date)</param>
        /// <param name="editedBy">Person making the change</param>
        public void ExtendDeal(DateOnly newEndDate, int editedBy)
        {
            if (newEndDate <= EndDate)
                throw new ArgumentException("New end date must be after current end date.", nameof(newEndDate));

            ValidateEditor(editedBy);

            EndDate = newEndDate;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if this deal is currently active (within date range)
        /// </summary>
        /// <param name="checkDate">Date to check (defaults to today)</param>
        /// <returns>True if the deal is active on the specified date</returns>
        public bool IsActive(DateOnly? checkDate = null)
        {
            var date = checkDate ?? DateOnly.FromDateTime(DateTime.Today);
            return date >= StartDate && date <= EndDate;
        }

        /// <summary>
        /// Checks if this deal applies to a specific customer context
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="customerCategoryId">Customer category ID (optional)</param>
        /// <param name="buyingGroupId">Buying group ID (optional)</param>
        /// <returns>True if the deal applies to this customer context</returns>
        public bool AppliesTo(int customerId, int? customerCategoryId = null, int? buyingGroupId = null)
        {
            // Deal must be active
            if (!IsActive()) return false;

            // Check customer-specific targeting
            if (CustomerId.HasValue)
                return CustomerId.Value == customerId;

            // Check buying group targeting
            if (BuyingGroupId.HasValue && buyingGroupId.HasValue)
                return BuyingGroupId.Value == buyingGroupId.Value;

            // Check category targeting
            if (CustomerCategoryId.HasValue && customerCategoryId.HasValue)
                return CustomerCategoryId.Value == customerCategoryId.Value;

            // If no specific targeting, applies to all customers
            return !CustomerId.HasValue && !BuyingGroupId.HasValue && !CustomerCategoryId.HasValue;
        }

        /// <summary>
        /// Checks if this deal applies to a specific stock item context
        /// </summary>
        /// <param name="stockItemId">Stock item ID</param>
        /// <param name="stockGroupId">Stock group ID (optional)</param>
        /// <returns>True if the deal applies to this stock item context</returns>
        public bool AppliesToStock(int stockItemId, int? stockGroupId = null)
        {
            // Check specific stock item targeting
            if (StockItemId.HasValue)
                return StockItemId.Value == stockItemId;

            // Check stock group targeting
            if (StockGroupId.HasValue && stockGroupId.HasValue)
                return StockGroupId.Value == stockGroupId.Value;

            // If no stock targeting, applies to all stock items
            return !StockItemId.HasValue && !StockGroupId.HasValue;
        }

        /// <summary>
        /// Checks if this deal applies to a complete context (customer + stock)
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="stockItemId">Stock item ID</param>
        /// <param name="customerCategoryId">Customer category ID (optional)</param>
        /// <param name="buyingGroupId">Buying group ID (optional)</param>
        /// <param name="stockGroupId">Stock group ID (optional)</param>
        /// <returns>True if the deal applies to this complete context</returns>
        public bool AppliesToContext(
            int customerId,
            int stockItemId,
            int? customerCategoryId = null,
            int? buyingGroupId = null,
            int? stockGroupId = null)
        {
            return AppliesTo(customerId, customerCategoryId, buyingGroupId) &&
                   AppliesToStock(stockItemId, stockGroupId);
        }

        /// <summary>
        /// Calculates the effective price for a base unit price using this deal
        /// </summary>
        /// <param name="baseUnitPrice">Original unit price</param>
        /// <returns>Effective price after applying the deal</returns>
        public decimal CalculatePrice(decimal baseUnitPrice)
        {
            return Pricing.CalculateEffectivePrice(baseUnitPrice);
        }

        /// <summary>
        /// Calculates the savings from this deal
        /// </summary>
        /// <param name="baseUnitPrice">Original unit price</param>
        /// <returns>Savings amount per unit</returns>
        public decimal CalculateSavings(decimal baseUnitPrice)
        {
            return Pricing.CalculateSavings(baseUnitPrice);
        }

        /// <summary>
        /// Gets the deal targeting scope description
        /// </summary>
        public string TargetingScope
        {
            get
            {
                var scopes = new List<string>();

                if (CustomerId.HasValue)
                    scopes.Add($"Customer {CustomerId}");
                else if (BuyingGroupId.HasValue)
                    scopes.Add($"Buying Group {BuyingGroupId}");
                else if (CustomerCategoryId.HasValue)
                    scopes.Add($"Category {CustomerCategoryId}");
                else
                    scopes.Add("All Customers");

                if (StockItemId.HasValue)
                    scopes.Add($"Stock Item {StockItemId}");
                else if (StockGroupId.HasValue)
                    scopes.Add($"Stock Group {StockGroupId}");
                else
                    scopes.Add("All Stock");

                return string.Join(" | ", scopes);
            }
        }

        /// <summary>
        /// Gets the deal status description
        /// </summary>
        public string DealStatus
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                
                if (today < StartDate)
                    return $"Upcoming (starts {StartDate:yyyy-MM-dd})";
                
                if (today > EndDate)
                    return $"Expired (ended {EndDate:yyyy-MM-dd})";
                
                return $"Active (ends {EndDate:yyyy-MM-dd})";
            }
        }

        /// <summary>
        /// Gets the deal specificity level (higher is more specific)
        /// </summary>
        public int SpecificityLevel
        {
            get
            {
                var level = 0;
                
                // Customer targeting specificity
                if (CustomerId.HasValue) level += 100;
                else if (BuyingGroupId.HasValue) level += 50;
                else if (CustomerCategoryId.HasValue) level += 25;
                
                // Stock targeting specificity
                if (StockItemId.HasValue) level += 10;
                else if (StockGroupId.HasValue) level += 5;
                
                return level;
            }
        }

        /// <summary>
        /// Indicates if this deal has expired
        /// </summary>
        public bool IsExpired => DateOnly.FromDateTime(DateTime.Today) > EndDate;

        /// <summary>
        /// Indicates if this deal is upcoming (not yet started)
        /// </summary>
        public bool IsUpcoming => DateOnly.FromDateTime(DateTime.Today) < StartDate;

        /// <summary>
        /// Gets the number of days remaining for active deals
        /// </summary>
        public int? DaysRemaining
        {
            get
            {
                if (!IsActive()) return null;
                return EndDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
            }
        }

        /// <summary>
        /// Indicates if this deal is customer-specific
        /// </summary>
        public bool IsCustomerSpecific => CustomerId.HasValue;

        /// <summary>
        /// Indicates if this deal is stock-specific
        /// </summary>
        public bool IsStockSpecific => StockItemId.HasValue;

        /// <summary>
        /// Indicates if this deal uses special pricing (vs. discounts)
        /// </summary>
        public bool UsesSpecialPricing => Pricing.IsSpecialPrice;

        // Validation methods
        private static void ValidateDealDescription(string dealDescription)
        {
            if (string.IsNullOrWhiteSpace(dealDescription))
                throw new ArgumentException("Deal description cannot be null or empty.", nameof(dealDescription));
            
            if (dealDescription.Length > 30)
                throw new ArgumentException("Deal description cannot exceed 30 characters.", nameof(dealDescription));
        }

        private static void ValidateDateRange(DateOnly startDate, DateOnly endDate)
        {
            if (startDate == default)
                throw new ArgumentException("Start date must be a valid date.", nameof(startDate));
                
            if (endDate == default)
                throw new ArgumentException("End date must be a valid date.", nameof(endDate));
                
            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date.", nameof(endDate));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("Editor must be a valid person ID.", nameof(editedBy));
        }

        private static void ValidateTargetingOptions(int? customerId, int? buyingGroupId, int? customerCategoryId)
        {
            var optionsSet = 0;
            if (customerId.HasValue) optionsSet++;
            if (buyingGroupId.HasValue) optionsSet++;
            if (customerCategoryId.HasValue) optionsSet++;

            if (optionsSet > 1)
                throw new ArgumentException("Cannot specify multiple customer targeting options (CustomerId, BuyingGroupId, CustomerCategoryId).");
        }

        private static void ValidateStockTargetingOptions(int? stockItemId, int? stockGroupId)
        {
            if (stockItemId.HasValue && stockGroupId.HasValue)
                throw new ArgumentException("Cannot specify both StockItemId and StockGroupId.");
        }

        private static void ValidateSpecialPriceRequiresStockItem(DealPricing pricing, int? stockItemId)
        {
            if (pricing.IsSpecialPrice && !stockItemId.HasValue)
                throw new ArgumentException("Special price deals require a specific stock item.");
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (SpecialDealId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            SpecialDealId = id;
        }

        public override string ToString()
        {
            return $"Deal {SpecialDealId}: {DealDescription} - {Pricing.PricingDescription} ({DealStatus})";
        }

        public override bool Equals(object? obj)
        {
            return obj is SpecialDeal other && SpecialDealId == other.SpecialDealId;
        }

        public override int GetHashCode()
        {
            return SpecialDealId.GetHashCode();
        }
    }
} 