using WideWorldImporters.Domain.Sales;
using WideWorldImporters.Domain.Tests.TestHelpers;

namespace WideWorldImporters.Domain.Tests.Entities
{
    public class OrderTests
    {
        #region Construction Tests
        
        [Fact]
        public void Constructor_WithValidParameters_CreatesOrder()
        {
            // Arrange
            var orderDate = TestDataBuilder.ValidOrderDate();
            var deliveryDate = TestDataBuilder.ValidDeliveryDate();
            
            // Act
            var order = new Order(
                customerId: TestDataBuilder.ValidCustomerId,
                salespersonPersonId: TestDataBuilder.ValidPersonId,
                contactPersonId: TestDataBuilder.ValidPersonId + 1,
                orderDate: orderDate,
                expectedDeliveryDate: deliveryDate,
                lastEditedBy: TestDataBuilder.ValidPersonId,
                customerPurchaseOrderNumber: TestDataBuilder.ValidPurchaseOrderNumber(),
                isUndersupplyBackordered: true,
                comments: TestDataBuilder.ValidOrderComments(),
                deliveryInstructions: "Handle with care",
                internalComments: "Internal note"
            );
            
            // Assert
            Assert.Equal(TestDataBuilder.ValidCustomerId, order.CustomerId);
            Assert.Equal(TestDataBuilder.ValidPersonId, order.SalespersonPersonId);
            Assert.Equal(TestDataBuilder.ValidPersonId + 1, order.ContactPersonId);
            Assert.Equal(orderDate, order.OrderDate);
            Assert.Equal(deliveryDate, order.ExpectedDeliveryDate);
            Assert.Equal(TestDataBuilder.ValidPurchaseOrderNumber(), order.CustomerPurchaseOrderNumber);
            Assert.True(order.IsUndersupplyBackordered);
            Assert.Equal(TestDataBuilder.ValidOrderComments(), order.Comments);
            Assert.Equal("Handle with care", order.DeliveryInstructions);
            Assert.Equal("Internal note", order.InternalComments);
            Assert.Null(order.BackorderOrderId);
            Assert.False(order.IsPickerAssigned);
            Assert.False(order.IsPickingCompleted);
            Assert.Equal(TestDataBuilder.ValidPersonId, order.LastEditedBy);
        }
        
        [Fact]
        public void CreateStandardOrder_WithValidParameters_CreatesStandardOrder()
        {
            // Arrange
            var orderDate = TestDataBuilder.ValidOrderDate();
            var deliveryDate = TestDataBuilder.ValidDeliveryDate();
            
            // Act
            var order = Order.CreateStandardOrder(
                customerId: TestDataBuilder.ValidCustomerId,
                salespersonPersonId: TestDataBuilder.ValidPersonId,
                contactPersonId: TestDataBuilder.ValidPersonId + 1,
                orderDate: orderDate,
                expectedDeliveryDate: deliveryDate,
                lastEditedBy: TestDataBuilder.ValidPersonId,
                customerPurchaseOrderNumber: TestDataBuilder.ValidPurchaseOrderNumber()
            );
            
            // Assert
            Assert.Equal(TestDataBuilder.ValidCustomerId, order.CustomerId);
            Assert.True(order.IsUndersupplyBackordered); // Default for standard orders
            Assert.Null(order.BackorderOrderId);
            Assert.False(order.IsBackorder);
        }
        
        [Fact]
        public void CreateBackorder_WithValidParameters_CreatesBackorder()
        {
            // Arrange
            const int originalOrderId = 123;
            var orderDate = TestDataBuilder.ValidOrderDate();
            var deliveryDate = TestDataBuilder.ValidDeliveryDate();
            
            // Act
            var backorder = Order.CreateBackorder(
                originalOrderId: originalOrderId,
                customerId: TestDataBuilder.ValidCustomerId,
                salespersonPersonId: TestDataBuilder.ValidPersonId,
                contactPersonId: TestDataBuilder.ValidPersonId + 1,
                orderDate: orderDate,
                expectedDeliveryDate: deliveryDate,
                lastEditedBy: TestDataBuilder.ValidPersonId,
                customerPurchaseOrderNumber: TestDataBuilder.ValidPurchaseOrderNumber()
            );
            
            // Assert
            Assert.Equal(originalOrderId, backorder.BackorderOrderId);
            Assert.True(backorder.IsBackorder);
            Assert.True(backorder.IsUndersupplyBackordered);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidCustomerId_ThrowsArgumentException(int invalidCustomerId)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidOrder(customerId: invalidCustomerId));
            
            Assert.Equal("customerId", exception.ParamName);
            Assert.Contains("must be a valid customer reference", exception.Message);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidSalespersonId_ThrowsArgumentException(int invalidSalespersonId)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidOrder(salespersonPersonId: invalidSalespersonId));
            
            Assert.Equal("salespersonPersonId", exception.ParamName);
            Assert.Contains("must be a valid reference", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithFutureOrderDate_ThrowsArgumentException()
        {
            // Arrange
            var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidOrder(orderDate: futureDate));
            
            Assert.Equal("orderDate", exception.ParamName);
            Assert.Contains("cannot be in the future", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithDeliveryDateBeforeOrderDate_ThrowsArgumentException()
        {
            // Arrange
            var orderDate = TestDataBuilder.ValidOrderDate();
            var earlierDeliveryDate = orderDate.AddDays(-1);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidOrder(
                orderDate: orderDate, 
                expectedDeliveryDate: earlierDeliveryDate));
            
            Assert.Equal("expectedDeliveryDate", exception.ParamName);
            Assert.Contains("cannot be before order date", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithPurchaseOrderNumberTooLong_ThrowsArgumentException()
        {
            // Arrange
            var longPurchaseOrderNumber = TestDataBuilder.StringOfLength(21);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                CreateValidOrder(customerPurchaseOrderNumber: longPurchaseOrderNumber));
            
            Assert.Equal("customerPurchaseOrderNumber", exception.ParamName);
            Assert.Contains("cannot exceed 20 characters", exception.Message);
        }
        
        #endregion
        
        #region Picking Workflow Tests
        
        [Fact]
        public void AssignPicker_WithValidPickerId_AssignsPicker()
        {
            // Arrange
            var order = CreateValidOrder();
            const int pickerId = 99;
            const int editorId = 88;
            
            // Act
            order.AssignPicker(pickerId, editorId);
            
            // Assert
            Assert.Equal(pickerId, order.PickedByPersonId);
            Assert.True(order.IsPickerAssigned);
            Assert.False(order.IsPickingCompleted);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void AssignPicker_WhenPickingCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            order.CompletePicking(DateTime.UtcNow, TestDataBuilder.ValidPersonId);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                order.AssignPicker(88, TestDataBuilder.ValidPersonId));
            
            Assert.Contains("already been picked", exception.Message);
        }
        
        [Fact]
        public void UnassignPicker_WhenPickerAssigned_RemovesAssignment()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            const int editorId = 88;
            
            // Act
            order.UnassignPicker(editorId);
            
            // Assert
            Assert.Null(order.PickedByPersonId);
            Assert.False(order.IsPickerAssigned);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void UnassignPicker_WhenNoPickerAssigned_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                order.UnassignPicker(TestDataBuilder.ValidPersonId));
            
            Assert.Contains("No picker is currently assigned", exception.Message);
        }
        
        [Fact]
        public void CompletePicking_WithValidParameters_CompletesPickingSuccessfully()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            var completionTime = DateTime.UtcNow.AddMinutes(-5);
            const int editorId = 88;
            
            // Act
            order.CompletePicking(completionTime, editorId);
            
            // Assert
            Assert.True(order.IsPickingCompleted);
            Assert.Equal(completionTime, order.PickingCompletedWhen);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void CompletePicking_WithoutAssignedPicker_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                order.CompletePicking(DateTime.UtcNow, TestDataBuilder.ValidPersonId));
            
            Assert.Contains("without an assigned picker", exception.Message);
        }
        
        [Fact]
        public void CompletePicking_WithFutureCompletionTime_ThrowsArgumentException()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            var futureTime = DateTime.UtcNow.AddHours(1);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                order.CompletePicking(futureTime, TestDataBuilder.ValidPersonId));
            
            Assert.Equal("pickingCompletedWhen", exception.ParamName);
            Assert.Contains("cannot be in the future", exception.Message);
        }
        
        [Fact]
        public void ReopenPicking_WhenPickingCompleted_ReopensSuccessfully()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            order.CompletePicking(DateTime.UtcNow.AddMinutes(-5), TestDataBuilder.ValidPersonId);
            const int editorId = 88;
            
            // Act
            order.ReopenPicking(editorId);
            
            // Assert
            Assert.False(order.IsPickingCompleted);
            Assert.Null(order.PickingCompletedWhen);
            Assert.True(order.IsPickerAssigned); // Picker should still be assigned
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void ReopenPicking_WhenPickingNotCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                order.ReopenPicking(TestDataBuilder.ValidPersonId));
            
            Assert.Contains("not completed", exception.Message);
        }
        
        #endregion
        
        #region Business Logic Tests
        
        [Fact]
        public void UpdateExpectedDeliveryDate_WithValidDate_UpdatesDate()
        {
            // Arrange
            var order = CreateValidOrder();
            var newDeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            const int editorId = 99;
            
            // Act
            order.UpdateExpectedDeliveryDate(newDeliveryDate, editorId);
            
            // Assert
            Assert.Equal(newDeliveryDate, order.ExpectedDeliveryDate);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void UpdateSalesperson_WhenNotPicked_UpdatesSalesperson()
        {
            // Arrange
            var order = CreateValidOrder();
            const int newSalespersonId = 77;
            const int editorId = 99;
            
            // Act
            order.UpdateSalesperson(newSalespersonId, editorId);
            
            // Assert
            Assert.Equal(newSalespersonId, order.SalespersonPersonId);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void UpdateSalesperson_WhenPickingCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            order.CompletePicking(DateTime.UtcNow.AddMinutes(-5), TestDataBuilder.ValidPersonId);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                order.UpdateSalesperson(77, TestDataBuilder.ValidPersonId));
            
            Assert.Contains("Cannot change salesperson for a picked order", exception.Message);
        }
        
        [Fact]
        public void UpdateBackorderPolicy_WhenNotPicked_UpdatesPolicy()
        {
            // Arrange
            var order = CreateValidOrder(isUndersupplyBackordered: true);
            const int editorId = 99;
            
            // Act
            order.UpdateBackorderPolicy(false, editorId);
            
            // Assert
            Assert.False(order.IsUndersupplyBackordered);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        [Fact]
        public void UpdateBackorderPolicy_WhenPickingCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = CreateValidOrder();
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            order.CompletePicking(DateTime.UtcNow.AddMinutes(-5), TestDataBuilder.ValidPersonId);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                order.UpdateBackorderPolicy(false, TestDataBuilder.ValidPersonId));
            
            Assert.Contains("Cannot change backorder policy for a picked order", exception.Message);
        }
        
        [Fact]
        public void UpdateAllComments_WithValidComments_UpdatesAllComments()
        {
            // Arrange
            var order = CreateValidOrder();
            const string newComments = "Updated customer comments";
            const string newDeliveryInstructions = "Updated delivery instructions";
            const string newInternalComments = "Updated internal comments";
            const int editorId = 99;
            
            // Act
            order.UpdateAllComments(newComments, newDeliveryInstructions, newInternalComments, editorId);
            
            // Assert
            Assert.Equal(newComments, order.Comments);
            Assert.Equal(newDeliveryInstructions, order.DeliveryInstructions);
            Assert.Equal(newInternalComments, order.InternalComments);
            Assert.Equal(editorId, order.LastEditedBy);
        }
        
        #endregion
        
        #region Calculated Properties Tests
        
        [Fact]
        public void IsDeliveryOverdue_WhenExpectedDatePassed_ReturnsTrue()
        {
            // Arrange
            var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var order = CreateValidOrder(expectedDeliveryDate: pastDate);
            
            // Act & Assert
            Assert.True(order.IsDeliveryOverdue);
        }
        
        [Fact]
        public void IsDeliveryOverdue_WhenPickingCompleted_ReturnsFalse()
        {
            // Arrange
            var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var order = CreateValidOrder(expectedDeliveryDate: pastDate);
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            order.CompletePicking(DateTime.UtcNow.AddMinutes(-5), TestDataBuilder.ValidPersonId);
            
            // Act & Assert
            Assert.False(order.IsDeliveryOverdue); // Picked orders are not overdue
        }
        
        [Fact]
        public void OrderStatus_ReturnsCorrectStatus()
        {
            // Arrange
            var order = CreateValidOrder();
            
            // Act & Assert - Pending
            Assert.Equal("Pending", order.OrderStatus);
            
            // Assign picker
            order.AssignPicker(99, TestDataBuilder.ValidPersonId);
            Assert.Equal("Picking", order.OrderStatus);
            
            // Complete picking
            order.CompletePicking(DateTime.UtcNow.AddMinutes(-5), TestDataBuilder.ValidPersonId);
            Assert.Equal("Picked", order.OrderStatus);
        }
        
        [Fact]
        public void DeliveryUrgency_ReturnsCorrectUrgency()
        {
            // Arrange & Act & Assert
            
            // Overdue
            var overdueOrder = CreateValidOrder(expectedDeliveryDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
            Assert.Equal("OVERDUE", overdueOrder.DeliveryUrgency);
            
            // Due today
            var dueTodayOrder = CreateValidOrder(expectedDeliveryDate: DateOnly.FromDateTime(DateTime.Today));
            Assert.Equal("Due Today", dueTodayOrder.DeliveryUrgency);
            
            // Urgent (1 day)
            var urgentOrder = CreateValidOrder(expectedDeliveryDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
            Assert.Equal("Urgent", urgentOrder.DeliveryUrgency);
            
            // Normal (3 days)
            var normalOrder = CreateValidOrder(expectedDeliveryDate: DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            Assert.Equal("Normal", normalOrder.DeliveryUrgency);
            
            // Low priority (7 days)
            var lowPriorityOrder = CreateValidOrder(expectedDeliveryDate: DateOnly.FromDateTime(DateTime.Today.AddDays(7)));
            Assert.Equal("Low Priority", lowPriorityOrder.DeliveryUrgency);
        }
        
        [Fact]
        public void AgeDays_ReturnsCorrectAge()
        {
            // Arrange
            var orderDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3));
            var order = CreateValidOrder(orderDate: orderDate);
            
            // Act & Assert
            Assert.Equal(3, order.AgeDays);
        }
        
        [Fact]
        public void HasCustomerCommunications_ReturnsCorrectValue()
        {
            // Arrange
            var orderWithComments = CreateValidOrder(comments: "Customer facing comment");
            var orderWithDeliveryInstructions = CreateValidOrder(deliveryInstructions: "Special delivery");
            var orderWithoutCommunications = CreateValidOrder();
            
            // Act & Assert
            Assert.True(orderWithComments.HasCustomerCommunications);
            Assert.True(orderWithDeliveryInstructions.HasCustomerCommunications);
            Assert.False(orderWithoutCommunications.HasCustomerCommunications);
        }
        
        #endregion
        
        #region Equality and ToString Tests
        
        [Fact]
        public void Equals_WithSameOrderId_ReturnsTrue()
        {
            // Arrange
            var order1 = CreateValidOrder();
            var order2 = CreateValidOrder();
            
            // Set same ID using internal method
            order1.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(order1, new object[] { 123 });
            order2.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(order2, new object[] { 123 });
            
            // Act & Assert
            Assert.True(order1.Equals(order2));
            Assert.Equal(order1.GetHashCode(), order2.GetHashCode());
        }
        
        [Fact]
        public void ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var order = CreateValidOrder();
            order.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(order, new object[] { 123 });
            
            // Act
            var result = order.ToString();
            
            // Assert
            Assert.Contains("Order 123", result);
            Assert.Contains($"Customer {TestDataBuilder.ValidCustomerId}", result);
        }
        
        [Fact]
        public void ToString_ForBackorder_ReturnsCorrectFormat()
        {
            // Arrange
            var backorder = Order.CreateBackorder(
                originalOrderId: 456,
                customerId: TestDataBuilder.ValidCustomerId,
                salespersonPersonId: TestDataBuilder.ValidPersonId,
                contactPersonId: TestDataBuilder.ValidPersonId + 1,
                orderDate: TestDataBuilder.ValidOrderDate(),
                expectedDeliveryDate: TestDataBuilder.ValidDeliveryDate(),
                lastEditedBy: TestDataBuilder.ValidPersonId
            );
            backorder.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(backorder, new object[] { 789 });
            
            // Act
            var result = backorder.ToString();
            
            // Assert
            Assert.Contains("Backorder 789", result);
        }
        
        #endregion
        
        #region Helper Methods
        
        private Order CreateValidOrder(
            int? customerId = null,
            int? salespersonPersonId = null,
            int? contactPersonId = null,
            DateOnly? orderDate = null,
            DateOnly? expectedDeliveryDate = null,
            int? lastEditedBy = null,
            string? customerPurchaseOrderNumber = null,
            bool? isUndersupplyBackordered = null,
            string? comments = null,
            string? deliveryInstructions = null,
            string? internalComments = null,
            int? backorderOrderId = null)
        {
            return new Order(
                customerId: customerId ?? TestDataBuilder.ValidCustomerId,
                salespersonPersonId: salespersonPersonId ?? TestDataBuilder.ValidPersonId,
                contactPersonId: contactPersonId ?? (TestDataBuilder.ValidPersonId + 1),
                orderDate: orderDate ?? TestDataBuilder.ValidOrderDate(),
                expectedDeliveryDate: expectedDeliveryDate ?? TestDataBuilder.ValidDeliveryDate(),
                lastEditedBy: lastEditedBy ?? TestDataBuilder.ValidPersonId,
                customerPurchaseOrderNumber: customerPurchaseOrderNumber,
                isUndersupplyBackordered: isUndersupplyBackordered ?? true,
                comments: comments,
                deliveryInstructions: deliveryInstructions,
                internalComments: internalComments,
                backorderOrderId: backorderOrderId);
        }
        
        #endregion
    }
} 