using System.ComponentModel.DataAnnotations;
using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Main entity table for stock items
    /// </summary>
    public class StockItem
    {
        /// <summary>
        /// Numeric ID used for reference to a stock item within the database
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Full name of a stock item (but not a full description)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string StockItemName { get; private set; } = string.Empty;

        /// <summary>
        /// Usual supplier for this stock item
        /// </summary>
        public int SupplierId { get; private set; }

        /// <summary>
        /// Color (optional) for this stock item
        /// </summary>
        public int? ColorId { get; private set; }

        /// <summary>
        /// Packaging configuration for this stock item
        /// </summary>
        public PackagingConfiguration PackagingConfiguration { get; private set; }

        /// <summary>
        /// Brand for the stock item (if the item is branded)
        /// </summary>
        [MaxLength(50)]
        public string? Brand { get; private set; }

        /// <summary>
        /// Size of this item (eg: 100mm)
        /// </summary>
        [MaxLength(20)]
        public string? Size { get; private set; }

        /// <summary>
        /// Number of days typically taken from order to receipt of this stock item
        /// </summary>
        public int LeadTimeDays { get; private set; }

        /// <summary>
        /// Does this stock item need to be in a chiller?
        /// </summary>
        public bool IsChillerStock { get; private set; }

        /// <summary>
        /// Barcode for this stock item
        /// </summary>
        [MaxLength(50)]
        public string? Barcode { get; private set; }

        /// <summary>
        /// Pricing information for this stock item
        /// </summary>
        public PricingInformation PricingInformation { get; private set; }

        /// <summary>
        /// Typical weight for one unit of this product (packaged)
        /// </summary>
        public decimal TypicalWeightPerUnit { get; private set; }

        /// <summary>
        /// Marketing comments for this stock item (shared outside the organization)
        /// </summary>
        public string? MarketingComments { get; private set; }

        /// <summary>
        /// Internal comments (not exposed outside organization)
        /// </summary>
        public string? InternalComments { get; private set; }

        /// <summary>
        /// Photo of the product
        /// </summary>
        public byte[]? Photo { get; private set; }

        /// <summary>
        /// Custom fields added by system users
        /// </summary>
        public string? CustomFields { get; private set; }

        /// <summary>
        /// Advertising tags associated with this stock item (JSON array retrieved from CustomFields)
        /// </summary>
        public string? Tags { get; private set; }

        /// <summary>
        /// Combination of columns used by full text search (computed: StockItemName + ' ' + MarketingComments)
        /// </summary>
        public string SearchDetails { get; private set; } = string.Empty;

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
        private StockItem() 
        {
            PackagingConfiguration = null!;
            PricingInformation = null!;
        }

        /// <summary>
        /// Creates a new stock item
        /// </summary>
        /// <param name="stockItemName">Stock item name (required, max 100 characters)</param>
        /// <param name="supplierId">Supplier ID reference</param>
        /// <param name="packagingConfiguration">Packaging configuration</param>
        /// <param name="leadTimeDays">Lead time in days</param>
        /// <param name="pricingInformation">Pricing information</param>
        /// <param name="typicalWeightPerUnit">Weight per unit</param>
        /// <param name="lastEditedBy">ID of the person creating this stock item</param>
        /// <param name="colorId">Optional color ID</param>
        /// <param name="brand">Optional brand (max 50 characters)</param>
        /// <param name="size">Optional size (max 20 characters)</param>
        /// <param name="isChillerStock">Requires chiller storage</param>
        /// <param name="barcode">Optional barcode (max 50 characters)</param>
        /// <param name="marketingComments">Optional marketing comments</param>
        /// <param name="internalComments">Optional internal comments</param>
        /// <param name="photo">Optional photo data</param>
        /// <param name="customFields">Optional custom fields JSON</param>
        public StockItem(
            string stockItemName,
            int supplierId,
            PackagingConfiguration packagingConfiguration,
            int leadTimeDays,
            PricingInformation pricingInformation,
            decimal typicalWeightPerUnit,
            int lastEditedBy,
            int? colorId = null,
            string? brand = null,
            string? size = null,
            bool isChillerStock = false,
            string? barcode = null,
            string? marketingComments = null,
            string? internalComments = null,
            byte[]? photo = null,
            string? customFields = null)
        {
            ValidateStockItemName(stockItemName);
            ValidateLeadTimeDays(leadTimeDays);
            ValidateTypicalWeightPerUnit(typicalWeightPerUnit);
            ValidateBrand(brand);
            ValidateSize(size);
            ValidateBarcode(barcode);
            ValidateEditor(lastEditedBy);

            if (supplierId <= 0)
                throw new ArgumentException("Supplier ID must be a valid reference.", nameof(supplierId));

            StockItemName = stockItemName.Trim();
            SupplierId = supplierId;
            ColorId = colorId;
            PackagingConfiguration = packagingConfiguration ?? throw new ArgumentNullException(nameof(packagingConfiguration));
            Brand = string.IsNullOrWhiteSpace(brand) ? null : brand.Trim();
            Size = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
            LeadTimeDays = leadTimeDays;
            IsChillerStock = isChillerStock;
            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
            PricingInformation = pricingInformation ?? throw new ArgumentNullException(nameof(pricingInformation));
            TypicalWeightPerUnit = typicalWeightPerUnit;
            MarketingComments = string.IsNullOrWhiteSpace(marketingComments) ? null : marketingComments.Trim();
            InternalComments = string.IsNullOrWhiteSpace(internalComments) ? null : internalComments.Trim();
            Photo = photo;
            CustomFields = string.IsNullOrWhiteSpace(customFields) ? null : customFields.Trim();
            LastEditedBy = lastEditedBy;
        }

        /// <summary>
        /// Updates the stock item name
        /// </summary>
        /// <param name="newStockItemName">New stock item name</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateStockItemName(string newStockItemName, int editedBy)
        {
            ValidateStockItemName(newStockItemName);
            ValidateEditor(editedBy);

            StockItemName = newStockItemName.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the supplier
        /// </summary>
        /// <param name="newSupplierId">New supplier ID</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateSupplier(int newSupplierId, int editedBy)
        {
            if (newSupplierId <= 0)
                throw new ArgumentException("Supplier ID must be a valid reference.", nameof(newSupplierId));
            ValidateEditor(editedBy);

            SupplierId = newSupplierId;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the packaging configuration
        /// </summary>
        /// <param name="newPackagingConfiguration">New packaging configuration</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePackagingConfiguration(PackagingConfiguration newPackagingConfiguration, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newPackagingConfiguration);
            ValidateEditor(editedBy);

            PackagingConfiguration = newPackagingConfiguration;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the pricing information
        /// </summary>
        /// <param name="newPricingInformation">New pricing information</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePricingInformation(PricingInformation newPricingInformation, int editedBy)
        {
            ArgumentNullException.ThrowIfNull(newPricingInformation);
            ValidateEditor(editedBy);

            PricingInformation = newPricingInformation;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates physical attributes
        /// </summary>
        /// <param name="brand">Brand (optional)</param>
        /// <param name="size">Size (optional)</param>
        /// <param name="typicalWeightPerUnit">Weight per unit</param>
        /// <param name="colorId">Color ID (optional)</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePhysicalAttributes(string? brand, string? size, decimal typicalWeightPerUnit, int? colorId, int editedBy)
        {
            ValidateBrand(brand);
            ValidateSize(size);
            ValidateTypicalWeightPerUnit(typicalWeightPerUnit);
            ValidateEditor(editedBy);

            Brand = string.IsNullOrWhiteSpace(brand) ? null : brand.Trim();
            Size = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
            TypicalWeightPerUnit = typicalWeightPerUnit;
            ColorId = colorId;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates logistics information
        /// </summary>
        /// <param name="leadTimeDays">Lead time in days</param>
        /// <param name="isChillerStock">Requires chiller storage</param>
        /// <param name="barcode">Barcode (optional)</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateLogistics(int leadTimeDays, bool isChillerStock, string? barcode, int editedBy)
        {
            ValidateLeadTimeDays(leadTimeDays);
            ValidateBarcode(barcode);
            ValidateEditor(editedBy);

            LeadTimeDays = leadTimeDays;
            IsChillerStock = isChillerStock;
            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates marketing comments
        /// </summary>
        /// <param name="newMarketingComments">New marketing comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateMarketingComments(string? newMarketingComments, int editedBy)
        {
            ValidateEditor(editedBy);

            MarketingComments = string.IsNullOrWhiteSpace(newMarketingComments) ? null : newMarketingComments.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates internal comments
        /// </summary>
        /// <param name="newInternalComments">New internal comments</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateInternalComments(string? newInternalComments, int editedBy)
        {
            ValidateEditor(editedBy);

            InternalComments = string.IsNullOrWhiteSpace(newInternalComments) ? null : newInternalComments.Trim();
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates the product photo
        /// </summary>
        /// <param name="newPhoto">New photo data</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdatePhoto(byte[]? newPhoto, int editedBy)
        {
            ValidateEditor(editedBy);

            Photo = newPhoto;
            LastEditedBy = editedBy;
        }

        /// <summary>
        /// Updates custom fields
        /// </summary>
        /// <param name="newCustomFields">New custom fields JSON</param>
        /// <param name="editedBy">ID of the person making the change</param>
        public void UpdateCustomFields(string? newCustomFields, int editedBy)
        {
            ValidateEditor(editedBy);

            CustomFields = string.IsNullOrWhiteSpace(newCustomFields) ? null : newCustomFields.Trim();
            LastEditedBy = editedBy;
        }

        // Internal methods for infrastructure layer
        internal void SetId(int id)
        {
            if (StockItemId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            StockItemId = id;
        }

        internal void SetTemporalProperties(DateTime validFrom, DateTime validTo)
        {
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        internal void SetComputedColumns(string? tags, string searchDetails)
        {
            Tags = tags;
            SearchDetails = searchDetails;
        }

        // Validation methods
        private static void ValidateStockItemName(string stockItemName)
        {
            if (string.IsNullOrWhiteSpace(stockItemName))
                throw new ArgumentException("Stock item name cannot be null or empty.", nameof(stockItemName));
            
            if (stockItemName.Length > 100)
                throw new ArgumentException("Stock item name cannot exceed 100 characters.", nameof(stockItemName));
        }

        private static void ValidateLeadTimeDays(int leadTimeDays)
        {
            if (leadTimeDays < 0)
                throw new ArgumentException("Lead time days cannot be negative.", nameof(leadTimeDays));
        }

        private static void ValidateTypicalWeightPerUnit(decimal typicalWeightPerUnit)
        {
            if (typicalWeightPerUnit < 0)
                throw new ArgumentException("Typical weight per unit cannot be negative.", nameof(typicalWeightPerUnit));
        }

        private static void ValidateBrand(string? brand)
        {
            if (!string.IsNullOrEmpty(brand) && brand.Length > 50)
                throw new ArgumentException("Brand cannot exceed 50 characters.", nameof(brand));
        }

        private static void ValidateSize(string? size)
        {
            if (!string.IsNullOrEmpty(size) && size.Length > 20)
                throw new ArgumentException("Size cannot exceed 20 characters.", nameof(size));
        }

        private static void ValidateBarcode(string? barcode)
        {
            if (!string.IsNullOrEmpty(barcode) && barcode.Length > 50)
                throw new ArgumentException("Barcode cannot exceed 50 characters.", nameof(barcode));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("EditedBy must be a valid person ID.", nameof(editedBy));
        }

        public override string ToString()
        {
            return $"StockItem: {StockItemName} (ID: {StockItemId})";
        }

        public override bool Equals(object? obj)
        {
            return obj is StockItem other && StockItemId == other.StockItemId;
        }

        public override int GetHashCode()
        {
            return StockItemId.GetHashCode();
        }
    }
} 