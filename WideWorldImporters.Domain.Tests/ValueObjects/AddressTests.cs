using WideWorldImporters.Domain.ValueObjects;
using WideWorldImporters.Domain.Tests.TestHelpers;

namespace WideWorldImporters.Domain.Tests.ValueObjects
{
    public class AddressTests
    {
        #region Construction Tests
        
        [Fact]
        public void Constructor_WithValidParameters_CreatesAddress()
        {
            // Arrange
            const string addressLine1 = "123 Main Street";
            const string addressLine2 = "Suite 100";
            const string postalCode = "12345";
            const int cityId = TestDataBuilder.ValidCityId;
            
            // Act
            var address = new Address(addressLine1, postalCode, cityId, addressLine2);
            
            // Assert
            Assert.Equal(addressLine1, address.AddressLine1);
            Assert.Equal(addressLine2, address.AddressLine2);
            Assert.Equal(postalCode, address.PostalCode);
            Assert.Equal(cityId, address.CityId);
        }
        
        [Fact]
        public void Constructor_WithMinimalParameters_CreatesAddress()
        {
            // Arrange
            const string addressLine1 = "456 Oak Avenue";
            const string postalCode = "67890";
            const int cityId = TestDataBuilder.ValidCityId;
            
            // Act
            var address = new Address(addressLine1, postalCode, cityId);
            
            // Assert
            Assert.Equal(addressLine1, address.AddressLine1);
            Assert.Null(address.AddressLine2);
            Assert.Equal(postalCode, address.PostalCode);
            Assert.Equal(cityId, address.CityId);
        }
        
        [Fact]
        public void Constructor_TrimsWhitespace_ProperlyFormatsAddress()
        {
            // Arrange
            const string addressLine1 = "  123 Main Street  ";
            const string addressLine2 = "  Suite 100  ";
            const string postalCode = "  12345  ";
            
            // Act
            var address = new Address(addressLine1, postalCode, TestDataBuilder.ValidCityId, addressLine2);
            
            // Assert
            Assert.Equal("123 Main Street", address.AddressLine1);
            Assert.Equal("Suite 100", address.AddressLine2);
            Assert.Equal("12345", address.PostalCode);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidAddressLine1_ThrowsArgumentException(string? invalidAddressLine1)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Address(invalidAddressLine1!, "12345", TestDataBuilder.ValidCityId));
            
            Assert.Equal("addressLine1", exception.ParamName);
            Assert.Contains("cannot be null or empty", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithAddressLine1TooLong_ThrowsArgumentException()
        {
            // Arrange
            var longAddressLine1 = TestDataBuilder.StringOfLength(61);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Address(longAddressLine1, "12345", TestDataBuilder.ValidCityId));
            
            Assert.Equal("addressLine1", exception.ParamName);
            Assert.Contains("cannot exceed 60 characters", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithAddressLine1MaxLength_Succeeds()
        {
            // Arrange
            var maxLengthAddressLine1 = TestDataBuilder.StringOfLength(60);
            
            // Act
            var address = new Address(maxLengthAddressLine1, "12345", TestDataBuilder.ValidCityId);
            
            // Assert
            Assert.Equal(maxLengthAddressLine1, address.AddressLine1);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidPostalCode_ThrowsArgumentException(string? invalidPostalCode)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Address("123 Main St", invalidPostalCode!, TestDataBuilder.ValidCityId));
            
            Assert.Equal("postalCode", exception.ParamName);
            Assert.Contains("cannot be null or empty", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithPostalCodeTooLong_ThrowsArgumentException()
        {
            // Arrange
            var longPostalCode = TestDataBuilder.StringOfLength(11);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Address("123 Main St", longPostalCode, TestDataBuilder.ValidCityId));
            
            Assert.Equal("postalCode", exception.ParamName);
            Assert.Contains("cannot exceed 10 characters", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithAddressLine2TooLong_ThrowsArgumentException()
        {
            // Arrange
            var longAddressLine2 = TestDataBuilder.StringOfLength(61);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, longAddressLine2));
            
            Assert.Equal("addressLine2", exception.ParamName);
            Assert.Contains("cannot exceed 60 characters", exception.Message);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Constructor_WithInvalidCityId_ThrowsArgumentException(int invalidCityId)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Address("123 Main St", "12345", invalidCityId));
            
            Assert.Equal("cityId", exception.ParamName);
            Assert.Contains("must be a valid reference", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithEmptyAddressLine2_SetsToNull()
        {
            // Act
            var address = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, "   ");
            
            // Assert
            Assert.Null(address.AddressLine2);
        }
        
        #endregion
        
        #region Equality Tests
        
        [Fact]
        public void Equals_WithIdenticalAddresses_ReturnsTrue()
        {
            // Arrange
            var address1 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, "Suite 100");
            var address2 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, "Suite 100");
            
            // Act & Assert
            Assert.True(address1.Equals(address2));
            Assert.True(address1 == address2);
            Assert.False(address1 != address2);
            Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
        }
        
        [Fact]
        public void Equals_WithDifferentAddressLine1_ReturnsFalse()
        {
            // Arrange
            var address1 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId);
            var address2 = new Address("456 Oak Ave", "12345", TestDataBuilder.ValidCityId);
            
            // Act & Assert
            Assert.False(address1.Equals(address2));
            Assert.False(address1 == address2);
            Assert.True(address1 != address2);
        }
        
        [Fact]
        public void Equals_WithDifferentAddressLine2_ReturnsFalse()
        {
            // Arrange
            var address1 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, "Suite 100");
            var address2 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, "Suite 200");
            
            // Act & Assert
            Assert.False(address1.Equals(address2));
        }
        
        [Fact]
        public void Equals_WithDifferentPostalCode_ReturnsFalse()
        {
            // Arrange
            var address1 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId);
            var address2 = new Address("123 Main St", "67890", TestDataBuilder.ValidCityId);
            
            // Act & Assert
            Assert.False(address1.Equals(address2));
        }
        
        [Fact]
        public void Equals_WithDifferentCityId_ReturnsFalse()
        {
            // Arrange
            var address1 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId);
            var address2 = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId + 1);
            
            // Act & Assert
            Assert.False(address1.Equals(address2));
        }
        
        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var address = TestDataBuilder.ValidAddress();
            
            // Act & Assert
            Assert.False(address.Equals(null));
            Assert.False(address == null);
            Assert.True(address != null);
        }
        
        [Fact]
        public void Equals_WithSameReference_ReturnsTrue()
        {
            // Arrange
            var address = TestDataBuilder.ValidAddress();
            var sameAddress = address;
            
            // Act & Assert
            Assert.True(address.Equals(sameAddress));
            Assert.True(ReferenceEquals(address, sameAddress));
        }
        
        [Fact]
        public void Equals_WithObjectOfDifferentType_ReturnsFalse()
        {
            // Arrange
            var address = TestDataBuilder.ValidAddress();
            var otherObject = "not an address";
            
            // Act & Assert
            Assert.False(address.Equals(otherObject));
        }
        
        #endregion
        
        #region ToString Tests
        
        [Fact]
        public void ToString_WithBothAddressLines_FormatsCorrectly()
        {
            // Arrange
            var address = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId, "Suite 100");
            
            // Act
            var result = address.ToString();
            
            // Assert
            Assert.Equal("123 Main St, Suite 100, 12345", result);
        }
        
        [Fact]
        public void ToString_WithOnlyAddressLine1_FormatsCorrectly()
        {
            // Arrange
            var address = new Address("123 Main St", "12345", TestDataBuilder.ValidCityId);
            
            // Act
            var result = address.ToString();
            
            // Assert
            Assert.Equal("123 Main St, 12345", result);
        }
        
        #endregion
        
        #region Immutability Tests
        
        [Fact]
        public void Address_IsImmutable_PropertiesAreReadOnly()
        {
            // Arrange
            var address = TestDataBuilder.ValidAddress();
            
            // Act & Assert - Properties should be read-only
            Assert.True(typeof(Address).GetProperty(nameof(Address.AddressLine1))!.CanWrite == false);
            Assert.True(typeof(Address).GetProperty(nameof(Address.AddressLine2))!.CanWrite == false);
            Assert.True(typeof(Address).GetProperty(nameof(Address.PostalCode))!.CanWrite == false);
            Assert.True(typeof(Address).GetProperty(nameof(Address.CityId))!.CanWrite == false);
        }
        
        #endregion
    }
} 