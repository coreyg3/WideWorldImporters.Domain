using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.Warehouse
{
    /// <summary>
    /// Represents a stock item transaction for tracking inventory movements
    /// This entity tracks all movements of stock items including receipts, shipments, adjustments, and transfers
    /// </summary>
    public class StockItemTransaction
    {
        /// <summary>
        /// Numeric ID used to refer to a stock item transaction within the database
        /// </summary>
        public int StockItemTransactionId { get; private set; }

        /// <summary>
        /// StockItem for this transaction
        /// </summary>
        public int StockItemId { get; private set; }

        /// <summary>
        /// Type of transaction (receipt, shipment, adjustment, transfer, etc.)
        /// </summary>
        public int TransactionTypeId { get; private set; }

        /// <summary>
        /// Customer for this transaction (if applicable - typically for outgoing stock)
        /// </summary>
        public int? CustomerId { get; private set; }

        /// <summary>
        /// ID of an invoice (for transactions associated with an invoice)
        /// </summary>
        public int? InvoiceId { get; private set; }

        /// <summary>
        /// Supplier for this stock transaction (if applicable - typically for incoming stock)
        /// </summary>
        public int? SupplierId { get; private set; }

        /// <summary>
        /// ID of a purchase order (for transactions associated with a purchase order)
        /// </summary>
        public int? PurchaseOrderId { get; private set; }

        /// <summary>
        /// Date and time when the transaction occurred
        /// </summary>
        public DateTime TransactionOccurredWhen { get; private set; }

        /// <summary>
        /// Quantity of stock movement (positive is incoming stock, negative is outgoing)
        /// </summary>
        public decimal Quantity { get; private set; }

        /// <summary>
        /// ID of the person who last edited this record
        /// </summary>
        public int LastEditedBy { get; private set; }

        /// <summary>
        /// When this record was last edited
        /// </summary>
        public DateTime LastEditedWhen { get; private set; }

        // Private parameterless constructor for EF Core
        private StockItemTransaction() { }

        /// <summary>
        /// Creates a new stock item transaction
        /// </summary>
        /// <param name="stockItemId">Stock item ID reference</param>
        /// <param name="transactionTypeId">Transaction type ID reference</param>
        /// <param name="quantity">Quantity moved (positive for incoming, negative for outgoing)</param>
        /// <param name="transactionOccurredWhen">When the transaction occurred</param>
        /// <param name="lastEditedBy">ID of the person creating this transaction</param>
        /// <param name="customerId">Optional customer ID for customer-related transactions</param>
        /// <param name="invoiceId">Optional invoice ID for invoice-related transactions</param>
        /// <param name="supplierId">Optional supplier ID for supplier-related transactions</param>
        /// <param name="purchaseOrderId">Optional purchase order ID for purchase order-related transactions</param>
        public StockItemTransaction(
            int stockItemId,
            int transactionTypeId,
            decimal quantity,
            DateTime transactionOccurredWhen,
            int lastEditedBy,
            int? customerId = null,
            int? invoiceId = null,
            int? supplierId = null,
            int? purchaseOrderId = null)
        {
            ValidateStockItemId(stockItemId);
            ValidateTransactionTypeId(transactionTypeId);
            ValidateQuantity(quantity);
            ValidateTransactionTime(transactionOccurredWhen);
            ValidateEditor(lastEditedBy);
            ValidateTransactionConsistency(customerId, invoiceId, supplierId, purchaseOrderId, quantity);

            StockItemId = stockItemId;
            TransactionTypeId = transactionTypeId;
            Quantity = quantity;
            TransactionOccurredWhen = transactionOccurredWhen;
            CustomerId = customerId;
            InvoiceId = invoiceId;
            SupplierId = supplierId;
            PurchaseOrderId = purchaseOrderId;
            LastEditedBy = lastEditedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a stock receipt transaction (incoming stock from supplier)
        /// </summary>
        /// <param name="stockItemId">Stock item ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be receipt type)</param>
        /// <param name="quantity">Quantity received (will be made positive)</param>
        /// <param name="supplierId">Supplier ID</param>
        /// <param name="transactionOccurredWhen">When the receipt occurred</param>
        /// <param name="lastEditedBy">Person recording the receipt</param>
        /// <param name="purchaseOrderId">Optional purchase order this receipt is for</param>
        /// <returns>New stock receipt transaction</returns>
        public static StockItemTransaction CreateReceiptTransaction(
            int stockItemId,
            int transactionTypeId,
            decimal quantity,
            int supplierId,
            DateTime transactionOccurredWhen,
            int lastEditedBy,
            int? purchaseOrderId = null)
        {
            if (supplierId <= 0)
                throw new ArgumentException("Supplier ID must be specified for receipt transactions.", nameof(supplierId));

            var positiveQuantity = Math.Abs(quantity); // Ensure positive for incoming stock

            return new StockItemTransaction(
                stockItemId,
                transactionTypeId,
                positiveQuantity,
                transactionOccurredWhen,
                lastEditedBy,
                supplierId: supplierId,
                purchaseOrderId: purchaseOrderId);
        }

        /// <summary>
        /// Creates a stock shipment transaction (outgoing stock to customer)
        /// </summary>
        /// <param name="stockItemId">Stock item ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be shipment type)</param>
        /// <param name="quantity">Quantity shipped (will be made negative)</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="transactionOccurredWhen">When the shipment occurred</param>
        /// <param name="lastEditedBy">Person recording the shipment</param>
        /// <param name="invoiceId">Optional invoice this shipment is for</param>
        /// <returns>New stock shipment transaction</returns>
        public static StockItemTransaction CreateShipmentTransaction(
            int stockItemId,
            int transactionTypeId,
            decimal quantity,
            int customerId,
            DateTime transactionOccurredWhen,
            int lastEditedBy,
            int? invoiceId = null)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be specified for shipment transactions.", nameof(customerId));

            var negativeQuantity = -Math.Abs(quantity); // Ensure negative for outgoing stock

            return new StockItemTransaction(
                stockItemId,
                transactionTypeId,
                negativeQuantity,
                transactionOccurredWhen,
                lastEditedBy,
                customerId: customerId,
                invoiceId: invoiceId);
        }

        /// <summary>
        /// Creates a stock adjustment transaction (inventory adjustments)
        /// </summary>
        /// <param name="stockItemId">Stock item ID</param>
        /// <param name="transactionTypeId">Transaction type ID (should be adjustment type)</param>
        /// <param name="quantity">Adjustment quantity (positive for increase, negative for decrease)</param>
        /// <param name="transactionOccurredWhen">When the adjustment occurred</param>
        /// <param name="lastEditedBy">Person making the adjustment</param>
        /// <returns>New stock adjustment transaction</returns>
        public static StockItemTransaction CreateAdjustmentTransaction(
            int stockItemId,
            int transactionTypeId,
            decimal quantity,
            DateTime transactionOccurredWhen,
            int lastEditedBy)
        {
            return new StockItemTransaction(
                stockItemId,
                transactionTypeId,
                quantity,
                transactionOccurredWhen,
                lastEditedBy);
        }

        /// <summary>
        /// Updates the quantity for this transaction
        /// </summary>
        /// <param name="newQuantity">New quantity</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateQuantity(decimal newQuantity, int editedBy)
        {
            ValidateQuantity(newQuantity);
            ValidateEditor(editedBy);

            Quantity = newQuantity;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the transaction occurrence time
        /// </summary>
        /// <param name="newTransactionTime">New transaction time</param>
        /// <param name="editedBy">Person making the update</param>
        public void UpdateTransactionTime(DateTime newTransactionTime, int editedBy)
        {
            ValidateTransactionTime(newTransactionTime);
            ValidateEditor(editedBy);

            TransactionOccurredWhen = newTransactionTime;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Links this transaction to a customer
        /// </summary>
        /// <param name="customerId">Customer ID to link to</param>
        /// <param name="editedBy">Person making the link</param>
        public void LinkToCustomer(int customerId, int editedBy)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be a valid reference.", nameof(customerId));

            ValidateEditor(editedBy);

            CustomerId = customerId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Links this transaction to a supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID to link to</param>
        /// <param name="editedBy">Person making the link</param>
        public void LinkToSupplier(int supplierId, int editedBy)
        {
            if (supplierId <= 0)
                throw new ArgumentException("Supplier ID must be a valid reference.", nameof(supplierId));

            ValidateEditor(editedBy);

            SupplierId = supplierId;
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
            if (invoiceId <= 0)
                throw new ArgumentException("Invoice ID must be a valid reference.", nameof(invoiceId));

            ValidateEditor(editedBy);

            InvoiceId = invoiceId;
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
            if (purchaseOrderId <= 0)
                throw new ArgumentException("Purchase order ID must be a valid reference.", nameof(purchaseOrderId));

            ValidateEditor(editedBy);

            PurchaseOrderId = purchaseOrderId;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the customer link from this transaction
        /// </summary>
        /// <param name="editedBy">Person removing the link</param>
        public void UnlinkFromCustomer(int editedBy)
        {
            ValidateEditor(editedBy);

            CustomerId = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the supplier link from this transaction
        /// </summary>
        /// <param name="editedBy">Person removing the link</param>
        public void UnlinkFromSupplier(int editedBy)
        {
            ValidateEditor(editedBy);

            SupplierId = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the invoice link from this transaction
        /// </summary>
        /// <param name="editedBy">Person removing the link</param>
        public void UnlinkFromInvoice(int editedBy)
        {
            ValidateEditor(editedBy);

            InvoiceId = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes the purchase order link from this transaction
        /// </summary>
        /// <param name="editedBy">Person removing the link</param>
        public void UnlinkFromPurchaseOrder(int editedBy)
        {
            ValidateEditor(editedBy);

            PurchaseOrderId = null;
            LastEditedBy = editedBy;
            LastEditedWhen = DateTime.UtcNow;
        }

        /// <summary>
        /// Indicates if this is an incoming stock transaction (positive quantity)
        /// </summary>
        public bool IsIncomingStock => Quantity > 0;

        /// <summary>
        /// Indicates if this is an outgoing stock transaction (negative quantity)
        /// </summary>
        public bool IsOutgoingStock => Quantity < 0;

        /// <summary>
        /// Gets the absolute quantity (always positive)
        /// </summary>
        public decimal AbsoluteQuantity => Math.Abs(Quantity);

        /// <summary>
        /// Indicates if this transaction is linked to a customer
        /// </summary>
        public bool IsLinkedToCustomer => CustomerId.HasValue;

        /// <summary>
        /// Indicates if this transaction is linked to a supplier
        /// </summary>
        public bool IsLinkedToSupplier => SupplierId.HasValue;

        /// <summary>
        /// Indicates if this transaction is linked to an invoice
        /// </summary>
        public bool IsLinkedToInvoice => InvoiceId.HasValue;

        /// <summary>
        /// Indicates if this transaction is linked to a purchase order
        /// </summary>
        public bool IsLinkedToPurchaseOrder => PurchaseOrderId.HasValue;

        /// <summary>
        /// Age of this transaction in hours from when it occurred
        /// </summary>
        public double AgeInHours => (DateTime.UtcNow - TransactionOccurredWhen).TotalHours;

        /// <summary>
        /// Age of this transaction in days from when it occurred
        /// </summary>
        public int AgeInDays => (int)Math.Floor(AgeInHours / 24);

        /// <summary>
        /// Gets the transaction direction description
        /// </summary>
        public string TransactionDirection => IsIncomingStock ? "Incoming" : "Outgoing";

        /// <summary>
        /// Gets the transaction context (customer, supplier, or internal)
        /// </summary>
        public string TransactionContext
        {
            get
            {
                if (IsLinkedToCustomer) return $"Customer {CustomerId}";
                if (IsLinkedToSupplier) return $"Supplier {SupplierId}";
                return "Internal";
            }
        }

        /// <summary>
        /// Gets linked document information
        /// </summary>
        public string LinkedDocuments
        {
            get
            {
                var documents = new List<string>();
                if (IsLinkedToInvoice) documents.Add($"Invoice {InvoiceId}");
                if (IsLinkedToPurchaseOrder) documents.Add($"PO {PurchaseOrderId}");
                return documents.Any() ? string.Join(", ", documents) : "None";
            }
        }

        /// <summary>
        /// Gets a comprehensive transaction summary
        /// </summary>
        public string TransactionSummary
        {
            get
            {
                var direction = TransactionDirection;
                var quantity = $"{AbsoluteQuantity:N3}";
                var context = TransactionContext;
                var documents = LinkedDocuments != "None" ? $" ({LinkedDocuments})" : "";
                
                return $"{direction}: {quantity} units - {context}{documents}";
            }
        }

        // Validation methods
        private static void ValidateStockItemId(int stockItemId)
        {
            if (stockItemId <= 0)
                throw new ArgumentException("Stock item ID must be a valid stock item reference.", nameof(stockItemId));
        }

        private static void ValidateTransactionTypeId(int transactionTypeId)
        {
            if (transactionTypeId <= 0)
                throw new ArgumentException("Transaction type ID must be a valid reference.", nameof(transactionTypeId));
        }

        private static void ValidateQuantity(decimal quantity)
        {
            if (quantity == 0)
                throw new ArgumentException("Quantity cannot be zero - stock transactions must have a quantity movement.", nameof(quantity));

            // Allow up to 3 decimal places as per database schema
            if (decimal.Round(quantity, 3) != quantity)
                throw new ArgumentException("Quantity cannot have more than 3 decimal places.", nameof(quantity));
        }

        private static void ValidateTransactionTime(DateTime transactionOccurredWhen)
        {
            if (transactionOccurredWhen == default)
                throw new ArgumentException("Transaction occurrence time must be specified.", nameof(transactionOccurredWhen));

            // Allow transactions up to 1 hour in the future to account for time zone differences
            if (transactionOccurredWhen > DateTime.UtcNow.AddHours(1))
                throw new ArgumentException("Transaction occurrence time cannot be significantly in the future.", nameof(transactionOccurredWhen));
        }

        private static void ValidateEditor(int editedBy)
        {
            if (editedBy <= 0)
                throw new ArgumentException("Editor must be a valid person ID.", nameof(editedBy));
        }

        private static void ValidateTransactionConsistency(int? customerId, int? invoiceId, int? supplierId, int? purchaseOrderId, decimal quantity)
        {
            // Outgoing stock (negative quantity) typically should have customer references
            if (quantity < 0 && !customerId.HasValue && invoiceId.HasValue)
                throw new ArgumentException("Outgoing stock transactions with invoices should typically reference a customer.", nameof(customerId));

            // Incoming stock (positive quantity) typically should have supplier references
            if (quantity > 0 && !supplierId.HasValue && purchaseOrderId.HasValue)
                throw new ArgumentException("Incoming stock transactions with purchase orders should typically reference a supplier.", nameof(supplierId));

            // Cannot have both customer and supplier for the same transaction
            if (customerId.HasValue && supplierId.HasValue)
                throw new ArgumentException("A stock transaction cannot be associated with both a customer and a supplier simultaneously.");
        }

        /// <summary>
        /// Sets the ID (typically called by infrastructure layer after persistence)
        /// </summary>
        internal void SetId(int id)
        {
            if (StockItemTransactionId != 0)
                throw new InvalidOperationException("ID can only be set once.");
            
            StockItemTransactionId = id;
        }

        public override string ToString()
        {
            return $"StockItemTransaction {StockItemTransactionId}: {TransactionSummary} for StockItem {StockItemId}";
        }

        public override bool Equals(object? obj)
        {
            return obj is StockItemTransaction other && StockItemTransactionId == other.StockItemTransactionId;
        }

        public override int GetHashCode()
        {
            return StockItemTransactionId.GetHashCode();
        }
    }
} 