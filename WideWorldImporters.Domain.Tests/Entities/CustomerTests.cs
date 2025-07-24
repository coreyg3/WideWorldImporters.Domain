using WideWorldImporters.Domain.Sales;
using WideWorldImporters.Domain.ValueObjects;
using WideWorldImporters.Domain.Tests.TestHelpers;

namespace WideWorldImporters.Domain.Tests.Entities
{
    public class CustomerTests
    {
        #region Construction Tests
        
        [Fact]
        public void Constructor_WithValidParameters_CreatesCustomer()
        {
            // Arrange
            var customerName = TestDataBuilder.ValidCustomerName();
            var deliveryAddress = TestDataBuilder.ValidAddress();
            var postalAddress = TestDataBuilder.ValidMinimalAddress();
            var contactInfo = TestDataBuilder.ValidContactInformation();
            var accountOpenedDate = TestDataBuilder.ValidAccountOpenedDate();
            
            // Act
            var customer = new Customer(
                customerName: customerName,
                billToCustomerId: TestDataBuilder.ValidCustomerId,
                customerCategoryId: TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: TestDataBuilder.ValidPersonId,
                deliveryMethodId: TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: deliveryAddress,
                postalAddress: postalAddress,
                contactInformation: contactInfo,
                accountOpenedDate: accountOpenedDate,
                standardDiscountPercentage: 5.0m,
                paymentDays: 30,
                lastEditedBy: TestDataBuilder.ValidPersonId);
            
            // Assert
            Assert.Equal(customerName, customer.CustomerName);
            Assert.Equal(TestDataBuilder.ValidCustomerId, customer.BillToCustomerId);
            Assert.Equal(TestDataBuilder.ValidCategoryId, customer.CustomerCategoryId);
            Assert.Equal(TestDataBuilder.ValidPersonId, customer.PrimaryContactPersonId);
            Assert.Equal(TestDataBuilder.ValidDeliveryMethodId, customer.DeliveryMethodId);
            Assert.Equal(deliveryAddress, customer.DeliveryAddress);
            Assert.Equal(postalAddress, customer.PostalAddress);
            Assert.Equal(contactInfo, customer.ContactInformation);
            Assert.Equal(accountOpenedDate, customer.AccountOpenedDate);
            Assert.Equal(5.0m, customer.StandardDiscountPercentage);
            Assert.Equal(30, customer.PaymentDays);
            Assert.True(customer.IsStatementSent); // Default value
            Assert.False(customer.IsOnCreditHold); // Default value
            Assert.Equal(TestDataBuilder.ValidPersonId, customer.LastEditedBy);
        }
        
        [Fact]
        public void Constructor_WithOptionalParameters_CreatesCustomerWithCorrectDefaults()
        {
            // Arrange & Act
            var customer = new Customer(
                customerName: TestDataBuilder.ValidCustomerName(),
                billToCustomerId: TestDataBuilder.ValidCustomerId,
                customerCategoryId: TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: TestDataBuilder.ValidPersonId,
                deliveryMethodId: TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: TestDataBuilder.ValidAddress(),
                postalAddress: TestDataBuilder.ValidMinimalAddress(),
                contactInformation: TestDataBuilder.ValidContactInformation(),
                accountOpenedDate: TestDataBuilder.ValidAccountOpenedDate(),
                standardDiscountPercentage: 0m,
                paymentDays: 0,
                lastEditedBy: TestDataBuilder.ValidPersonId,
                buyingGroupId: 5,
                alternateContactPersonId: 10,
                creditLimit: 50000m,
                isStatementSent: false,
                isOnCreditHold: true,
                deliveryRun: "RUN1",
                runPosition: "POS1");
            
            // Assert
            Assert.Equal(5, customer.BuyingGroupId);
            Assert.Equal(10, customer.AlternateContactPersonId);
            Assert.Equal(50000m, customer.CreditLimit);
            Assert.False(customer.IsStatementSent);
            Assert.True(customer.IsOnCreditHold);
            Assert.Equal("RUN1", customer.DeliveryRun);
            Assert.Equal("POS1", customer.RunPosition);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidCustomerName_ThrowsArgumentException(string? invalidName)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Customer(
                customerName: invalidName!,
                billToCustomerId: TestDataBuilder.ValidCustomerId,
                customerCategoryId: TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: TestDataBuilder.ValidPersonId,
                deliveryMethodId: TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: TestDataBuilder.ValidAddress(),
                postalAddress: TestDataBuilder.ValidMinimalAddress(),
                contactInformation: TestDataBuilder.ValidContactInformation(),
                accountOpenedDate: TestDataBuilder.ValidAccountOpenedDate(),
                standardDiscountPercentage: 5.0m,
                paymentDays: 30,
                lastEditedBy: TestDataBuilder.ValidPersonId));
            
            Assert.Equal("customerName", exception.ParamName);
            Assert.Contains("cannot be null or empty", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithCustomerNameTooLong_ThrowsArgumentException()
        {
            // Arrange
            var longName = TestDataBuilder.StringOfLength(101);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(customerName: longName));
            
            Assert.Equal("customerName", exception.ParamName);
            Assert.Contains("cannot exceed 100 characters", exception.Message);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidBillToCustomerId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(billToCustomerId: invalidId));
            
            Assert.Equal("billToCustomerId", exception.ParamName);
            Assert.Contains("must be a valid customer reference", exception.Message);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidCategoryId_ThrowsArgumentException(int invalidId)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(customerCategoryId: invalidId));
            
            Assert.Equal("customerCategoryId", exception.ParamName);
            Assert.Contains("must be a valid reference", exception.Message);
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Constructor_WithInvalidDiscountPercentage_ThrowsArgumentException(decimal invalidDiscount)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(standardDiscountPercentage: invalidDiscount));
            
            Assert.Equal("discountPercentage", exception.ParamName);
            Assert.Contains("must be between 0 and 100", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithNegativePaymentDays_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(paymentDays: -1));
            
            Assert.Equal("paymentDays", exception.ParamName);
            Assert.Contains("cannot be negative", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithNegativeCreditLimit_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(creditLimit: -1000m));
            
            Assert.Equal("creditLimit", exception.ParamName);
            Assert.Contains("cannot be negative", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithDeliveryRunTooLong_ThrowsArgumentException()
        {
            // Arrange
            var longDeliveryRun = TestDataBuilder.StringOfLength(6);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CreateValidCustomer(deliveryRun: longDeliveryRun));
            
            Assert.Equal("deliveryRun", exception.ParamName);
            Assert.Contains("cannot exceed 5 characters", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithNullDeliveryAddress_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Customer(
                customerName: TestDataBuilder.ValidCustomerName(),
                billToCustomerId: TestDataBuilder.ValidCustomerId,
                customerCategoryId: TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: TestDataBuilder.ValidPersonId,
                deliveryMethodId: TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: null!,
                postalAddress: TestDataBuilder.ValidMinimalAddress(),
                contactInformation: TestDataBuilder.ValidContactInformation(),
                accountOpenedDate: TestDataBuilder.ValidAccountOpenedDate(),
                standardDiscountPercentage: 5.0m,
                paymentDays: 30,
                lastEditedBy: TestDataBuilder.ValidPersonId));
        }
        
        [Fact]
        public void Constructor_WithNullPostalAddress_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Customer(
                customerName: TestDataBuilder.ValidCustomerName(),
                billToCustomerId: TestDataBuilder.ValidCustomerId,
                customerCategoryId: TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: TestDataBuilder.ValidPersonId,
                deliveryMethodId: TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: TestDataBuilder.ValidAddress(),
                postalAddress: null!,
                contactInformation: TestDataBuilder.ValidContactInformation(),
                accountOpenedDate: TestDataBuilder.ValidAccountOpenedDate(),
                standardDiscountPercentage: 5.0m,
                paymentDays: 30,
                lastEditedBy: TestDataBuilder.ValidPersonId));
        }
        
        [Fact]
        public void Constructor_WithNullContactInformation_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Customer(
                customerName: TestDataBuilder.ValidCustomerName(),
                billToCustomerId: TestDataBuilder.ValidCustomerId,
                customerCategoryId: TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: TestDataBuilder.ValidPersonId,
                deliveryMethodId: TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: TestDataBuilder.ValidAddress(),
                postalAddress: TestDataBuilder.ValidMinimalAddress(),
                contactInformation: null!,
                accountOpenedDate: TestDataBuilder.ValidAccountOpenedDate(),
                standardDiscountPercentage: 5.0m,
                paymentDays: 30,
                lastEditedBy: TestDataBuilder.ValidPersonId));
        }
        
        #endregion
        
        #region Business Logic Tests
        
        [Fact]
        public void UpdateCustomerName_WithValidName_UpdatesName()
        {
            // Arrange
            var customer = CreateValidCustomer();
            const string newName = "Updated Customer Name";
            const int editorId = 99;
            
            // Act
            customer.UpdateCustomerName(newName, editorId);
            
            // Assert
            Assert.Equal(newName, customer.CustomerName);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void UpdateCustomerName_WithInvalidName_ThrowsArgumentException()
        {
            // Arrange
            var customer = CreateValidCustomer();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => customer.UpdateCustomerName("", TestDataBuilder.ValidPersonId));
            Assert.Throws<ArgumentException>(() => customer.UpdateCustomerName(TestDataBuilder.StringOfLength(101), TestDataBuilder.ValidPersonId));
        }
        
        [Fact]
        public void PlaceOnCreditHold_WhenNotOnHold_PlacesOnHold()
        {
            // Arrange
            var customer = CreateValidCustomer(isOnCreditHold: false);
            const int editorId = 99;
            
            // Act
            customer.PlaceOnCreditHold(editorId);
            
            // Assert
            Assert.True(customer.IsOnCreditHold);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void PlaceOnCreditHold_WhenAlreadyOnHold_ThrowsInvalidOperationException()
        {
            // Arrange
            var customer = CreateValidCustomer(isOnCreditHold: true);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => customer.PlaceOnCreditHold(TestDataBuilder.ValidPersonId));
            Assert.Contains("already on credit hold", exception.Message);
        }
        
        [Fact]
        public void RemoveFromCreditHold_WhenOnHold_RemovesFromHold()
        {
            // Arrange
            var customer = CreateValidCustomer(isOnCreditHold: true);
            const int editorId = 99;
            
            // Act
            customer.RemoveFromCreditHold(editorId);
            
            // Assert
            Assert.False(customer.IsOnCreditHold);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void RemoveFromCreditHold_WhenNotOnHold_ThrowsInvalidOperationException()
        {
            // Arrange
            var customer = CreateValidCustomer(isOnCreditHold: false);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => customer.RemoveFromCreditHold(TestDataBuilder.ValidPersonId));
            Assert.Contains("not on credit hold", exception.Message);
        }
        
        [Fact]
        public void SetCreditLimit_WithValidLimit_UpdatesLimit()
        {
            // Arrange
            var customer = CreateValidCustomer();
            const decimal newLimit = 75000m;
            const int editorId = 99;
            
            // Act
            customer.SetCreditLimit(newLimit, editorId);
            
            // Assert
            Assert.Equal(newLimit, customer.CreditLimit);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void SetCreditLimit_WithNull_ClearsLimit()
        {
            // Arrange
            var customer = CreateValidCustomer(creditLimit: 50000m);
            const int editorId = 99;
            
            // Act
            customer.SetCreditLimit(null, editorId);
            
            // Assert
            Assert.Null(customer.CreditLimit);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void UpdateStandardDiscount_WithValidDiscount_UpdatesDiscount()
        {
            // Arrange
            var customer = CreateValidCustomer();
            const decimal newDiscount = 12.5m;
            const int editorId = 99;
            
            // Act
            customer.UpdateStandardDiscount(newDiscount, editorId);
            
            // Assert
            Assert.Equal(newDiscount, customer.StandardDiscountPercentage);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Theory]
        [InlineData(-0.1)]
        [InlineData(100.1)]
        public void UpdateStandardDiscount_WithInvalidDiscount_ThrowsArgumentException(decimal invalidDiscount)
        {
            // Arrange
            var customer = CreateValidCustomer();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => customer.UpdateStandardDiscount(invalidDiscount, TestDataBuilder.ValidPersonId));
        }
        
        [Fact]
        public void UpdatePaymentTerms_WithValidDays_UpdatesTerms()
        {
            // Arrange
            var customer = CreateValidCustomer();
            const int newPaymentDays = 45;
            const int editorId = 99;
            
            // Act
            customer.UpdatePaymentTerms(newPaymentDays, editorId);
            
            // Assert
            Assert.Equal(newPaymentDays, customer.PaymentDays);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void UpdateDeliveryAddress_WithValidAddress_UpdatesAddress()
        {
            // Arrange
            var customer = CreateValidCustomer();
            var newAddress = new Address("789 New Street", "54321", TestDataBuilder.ValidCityId);
            const int editorId = 99;
            
            // Act
            customer.UpdateDeliveryAddress(newAddress, editorId);
            
            // Assert
            Assert.Equal(newAddress, customer.DeliveryAddress);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void UpdateContactInformation_WithValidInfo_UpdatesInfo()
        {
            // Arrange
            var customer = CreateValidCustomer();
            var newContactInfo = new ContactInformation("(555) 999-8888", "(555) 999-8889", "https://newsite.com");
            const int editorId = 99;
            
            // Act
            customer.UpdateContactInformation(newContactInfo, editorId);
            
            // Assert
            Assert.Equal(newContactInfo, customer.ContactInformation);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void UpdateDeliveryLogistics_WithValidParameters_UpdatesLogistics()
        {
            // Arrange
            var customer = CreateValidCustomer();
            const int newDeliveryMethodId = 5;
            const string newDeliveryRun = "RUN2";
            const string newRunPosition = "POS2";
            const int editorId = 99;
            
            // Act
            customer.UpdateDeliveryLogistics(newDeliveryMethodId, newDeliveryRun, newRunPosition, editorId);
            
            // Assert
            Assert.Equal(newDeliveryMethodId, customer.DeliveryMethodId);
            Assert.Equal(newDeliveryRun, customer.DeliveryRun);
            Assert.Equal(newRunPosition, customer.RunPosition);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        [Fact]
        public void UpdateStatementPreference_WithValidPreference_UpdatesPreference()
        {
            // Arrange
            var customer = CreateValidCustomer(isStatementSent: true);
            const int editorId = 99;
            
            // Act
            customer.UpdateStatementPreference(false, editorId);
            
            // Assert
            Assert.False(customer.IsStatementSent);
            Assert.Equal(editorId, customer.LastEditedBy);
        }
        
        #endregion
        
        #region Editor Validation Tests
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void BusinessMethods_WithInvalidEditor_ThrowArgumentException(int invalidEditor)
        {
            // Arrange
            var customer = CreateValidCustomer();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => customer.UpdateCustomerName("New Name", invalidEditor));
            Assert.Throws<ArgumentException>(() => customer.PlaceOnCreditHold(invalidEditor));
            Assert.Throws<ArgumentException>(() => customer.RemoveFromCreditHold(invalidEditor));
            Assert.Throws<ArgumentException>(() => customer.SetCreditLimit(10000m, invalidEditor));
            Assert.Throws<ArgumentException>(() => customer.UpdateStandardDiscount(5.0m, invalidEditor));
            Assert.Throws<ArgumentException>(() => customer.UpdatePaymentTerms(30, invalidEditor));
        }
        
        #endregion
        
        #region Equality and ToString Tests
        
        [Fact]
        public void Equals_WithSameCustomerId_ReturnsTrue()
        {
            // Arrange
            var customer1 = CreateValidCustomer();
            var customer2 = CreateValidCustomer();
            
            // Set same ID using internal method
            customer1.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(customer1, new object[] { 123 });
            customer2.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(customer2, new object[] { 123 });
            
            // Act & Assert
            Assert.True(customer1.Equals(customer2));
            Assert.Equal(customer1.GetHashCode(), customer2.GetHashCode());
        }
        
        [Fact]
        public void ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var customer = CreateValidCustomer();
            customer.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(customer, new object[] { 123 });
            
            // Act
            var result = customer.ToString();
            
            // Assert
            Assert.Contains("Customer:", result);
            Assert.Contains(TestDataBuilder.ValidCustomerName(), result);
            Assert.Contains("ID: 123", result);
        }
        
        #endregion
        
        #region Infrastructure Tests
        
        [Fact]
        public void SetId_WhenIdIsZero_SetsId()
        {
            // Arrange
            var customer = CreateValidCustomer();
            
            // Act
            customer.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(customer, new object[] { 123 });
            
            // Assert
            Assert.Equal(123, customer.CustomerId);
        }
        
        [Fact]
        public void SetId_WhenIdAlreadySet_ThrowsInvalidOperationException()
        {
            // Arrange
            var customer = CreateValidCustomer();
            var setIdMethod = customer.GetType().GetMethod("SetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            setIdMethod.Invoke(customer, new object[] { 123 });
            
            // Act & Assert
            var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() => 
                setIdMethod.Invoke(customer, new object[] { 456 }));
            
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Contains("can only be set once", exception.InnerException!.Message);
        }
        
        #endregion
        
        #region Helper Methods
        
        private Customer CreateValidCustomer(
            string? customerName = null,
            int? billToCustomerId = null,
            int? customerCategoryId = null,
            int? primaryContactPersonId = null,
            int? deliveryMethodId = null,
            Address? deliveryAddress = null,
            Address? postalAddress = null,
            ContactInformation? contactInformation = null,
            DateOnly? accountOpenedDate = null,
            decimal? standardDiscountPercentage = null,
            int? paymentDays = null,
            int? lastEditedBy = null,
            int? buyingGroupId = null,
            int? alternateContactPersonId = null,
            decimal? creditLimit = null,
            bool? isStatementSent = null,
            bool? isOnCreditHold = null,
            string? deliveryRun = null,
            string? runPosition = null,
            byte[]? deliveryLocation = null)
        {
            return new Customer(
                customerName: customerName ?? TestDataBuilder.ValidCustomerName(),
                billToCustomerId: billToCustomerId ?? TestDataBuilder.ValidCustomerId,
                customerCategoryId: customerCategoryId ?? TestDataBuilder.ValidCategoryId,
                primaryContactPersonId: primaryContactPersonId ?? TestDataBuilder.ValidPersonId,
                deliveryMethodId: deliveryMethodId ?? TestDataBuilder.ValidDeliveryMethodId,
                deliveryAddress: deliveryAddress ?? TestDataBuilder.ValidAddress(),
                postalAddress: postalAddress ?? TestDataBuilder.ValidMinimalAddress(),
                contactInformation: contactInformation ?? TestDataBuilder.ValidContactInformation(),
                accountOpenedDate: accountOpenedDate ?? TestDataBuilder.ValidAccountOpenedDate(),
                standardDiscountPercentage: standardDiscountPercentage ?? 5.0m,
                paymentDays: paymentDays ?? 30,
                lastEditedBy: lastEditedBy ?? TestDataBuilder.ValidPersonId,
                buyingGroupId: buyingGroupId,
                alternateContactPersonId: alternateContactPersonId,
                creditLimit: creditLimit,
                isStatementSent: isStatementSent ?? true,
                isOnCreditHold: isOnCreditHold ?? false,
                deliveryRun: deliveryRun,
                runPosition: runPosition,
                deliveryLocation: deliveryLocation);
        }
        
        #endregion
    }
} 