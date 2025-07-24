using WideWorldImporters.Domain.ValueObjects;
using WideWorldImporters.Domain.Tests.TestHelpers;

namespace WideWorldImporters.Domain.Tests.ValueObjects
{
    public class LineFinancialsTests
    {
        #region Construction Tests
        
        [Fact]
        public void Constructor_WithValidParameters_CreatesLineFinancials()
        {
            // Arrange
            const int quantity = 10;
            const decimal unitPrice = 25.50m;
            const decimal taxRate = 8.25m;
            const decimal taxAmount = 21.04m;
            const decimal lineProfit = 55.00m;
            const decimal extendedPrice = 255.00m;
            
            // Act
            var lineFinancials = new LineFinancials(quantity, unitPrice, taxRate, taxAmount, lineProfit, extendedPrice);
            
            // Assert
            Assert.Equal(quantity, lineFinancials.Quantity);
            Assert.Equal(unitPrice, lineFinancials.UnitPrice);
            Assert.Equal(taxRate, lineFinancials.TaxRate);
            Assert.Equal(taxAmount, lineFinancials.TaxAmount);
            Assert.Equal(lineProfit, lineFinancials.LineProfit);
            Assert.Equal(extendedPrice, lineFinancials.ExtendedPrice);
        }
        
        [Fact]
        public void Constructor_WithNullUnitPrice_CreatesLineFinancials()
        {
            // Arrange
            const int quantity = 5;
            const decimal taxRate = 0m;
            const decimal taxAmount = 0m;
            const decimal lineProfit = -75.00m; // Loss for free items
            const decimal extendedPrice = 0m;
            
            // Act
            var lineFinancials = new LineFinancials(quantity, null, taxRate, taxAmount, lineProfit, extendedPrice);
            
            // Assert
            Assert.Equal(quantity, lineFinancials.Quantity);
            Assert.Null(lineFinancials.UnitPrice);
            Assert.Equal(taxRate, lineFinancials.TaxRate);
            Assert.Equal(taxAmount, lineFinancials.TaxAmount);
            Assert.Equal(lineProfit, lineFinancials.LineProfit);
            Assert.Equal(extendedPrice, lineFinancials.ExtendedPrice);
        }
        
        #endregion
        
        #region Calculate Method Tests
        
        [Fact]
        public void Calculate_WithValidParameters_ReturnsCorrectFinancials()
        {
            // Arrange
            const int quantity = 10;
            const decimal unitPrice = 25.50m;
            const decimal taxRate = 8.25m;
            const decimal costPrice = 20.00m;
            
            // Expected calculations
            const decimal expectedExtendedPrice = 255.00m; // 10 * 25.50
            const decimal expectedTaxAmount = 21.04m; // 255.00 * 0.0825
            const decimal expectedLineProfit = 55.00m; // 255.00 - (10 * 20.00)
            
            // Act
            var lineFinancials = LineFinancials.Calculate(quantity, unitPrice, taxRate, costPrice);
            
            // Assert
            Assert.Equal(quantity, lineFinancials.Quantity);
            Assert.Equal(unitPrice, lineFinancials.UnitPrice);
            Assert.Equal(taxRate, lineFinancials.TaxRate);
            Assert.Equal(expectedExtendedPrice, lineFinancials.ExtendedPrice);
            Assert.Equal(expectedTaxAmount, lineFinancials.TaxAmount, 2); // 2 decimal places
            Assert.Equal(expectedLineProfit, lineFinancials.LineProfit);
        }
        
        [Fact]
        public void Calculate_WithNullUnitPrice_TreatsAsZero()
        {
            // Arrange
            const int quantity = 5;
            const decimal taxRate = 8.25m;
            const decimal costPrice = 15.00m;
            
            // Act
            var lineFinancials = LineFinancials.Calculate(quantity, null, taxRate, costPrice);
            
            // Assert
            Assert.Equal(quantity, lineFinancials.Quantity);
            Assert.Null(lineFinancials.UnitPrice);
            Assert.Equal(0m, lineFinancials.ExtendedPrice);
            Assert.Equal(0m, lineFinancials.TaxAmount);
            Assert.Equal(-75.00m, lineFinancials.LineProfit); // -(5 * 15.00)
        }
        
        [Fact]
        public void Calculate_WithZeroTaxRate_HasNoTax()
        {
            // Arrange
            const int quantity = 8;
            const decimal unitPrice = 12.50m;
            const decimal taxRate = 0m;
            const decimal costPrice = 10.00m;
            
            // Act
            var lineFinancials = LineFinancials.Calculate(quantity, unitPrice, taxRate, costPrice);
            
            // Assert
            Assert.Equal(100.00m, lineFinancials.ExtendedPrice); // 8 * 12.50
            Assert.Equal(0m, lineFinancials.TaxAmount);
            Assert.Equal(20.00m, lineFinancials.LineProfit); // 100 - (8 * 10)
        }
        
        #endregion
        
        #region CreateFreeItems Tests
        
        [Fact]
        public void CreateFreeItems_WithValidParameters_ReturnsCorrectFinancials()
        {
            // Arrange
            const int quantity = 3;
            const decimal costPrice = 25.00m;
            
            // Act
            var lineFinancials = LineFinancials.CreateFreeItems(quantity, costPrice);
            
            // Assert
            Assert.Equal(quantity, lineFinancials.Quantity);
            Assert.Null(lineFinancials.UnitPrice);
            Assert.Equal(0m, lineFinancials.TaxRate);
            Assert.Equal(0m, lineFinancials.TaxAmount);
            Assert.Equal(0m, lineFinancials.ExtendedPrice);
            Assert.Equal(-75.00m, lineFinancials.LineProfit); // -(3 * 25.00)
        }
        
        #endregion
        
        #region Calculated Property Tests
        
        [Fact]
        public void TotalIncludingTax_ReturnsCorrectSum()
        {
            // Arrange
            var lineFinancials = LineFinancials.Calculate(10, 25.50m, 8.25m, 20.00m);
            
            // Act
            var totalIncludingTax = lineFinancials.TotalIncludingTax;
            
            // Assert
            var expectedTotal = lineFinancials.ExtendedPrice + lineFinancials.TaxAmount;
            Assert.Equal(expectedTotal, totalIncludingTax);
        }
        
        [Fact]
        public void EffectiveUnitPrice_WithNullUnitPrice_ReturnsZero()
        {
            // Arrange
            var lineFinancials = LineFinancials.CreateFreeItems(5);
            
            // Act
            var effectiveUnitPrice = lineFinancials.EffectiveUnitPrice;
            
            // Assert
            Assert.Equal(0m, effectiveUnitPrice);
        }
        
        [Fact]
        public void EffectiveUnitPrice_WithValidUnitPrice_ReturnsUnitPrice()
        {
            // Arrange
            var lineFinancials = LineFinancials.Calculate(10, 25.50m, 8.25m, 20.00m);
            
            // Act
            var effectiveUnitPrice = lineFinancials.EffectiveUnitPrice;
            
            // Assert
            Assert.Equal(25.50m, effectiveUnitPrice);
        }
        
        [Theory]
        [InlineData(null, true)]
        [InlineData(0.0, true)]
        [InlineData(0.01, false)]
        [InlineData(25.50, false)]
        public void IsFreeItem_ReturnsCorrectValue(double? unitPriceDouble, bool expectedIsFree)
        {
            // Arrange
            decimal? unitPrice = unitPriceDouble.HasValue ? (decimal)unitPriceDouble.Value : null;
            var lineFinancials = LineFinancials.Calculate(10, unitPrice, 8.25m, 20.00m);
            
            // Act & Assert
            Assert.Equal(expectedIsFree, lineFinancials.IsFreeItem);
        }
        
        [Theory]
        [InlineData(55.00, true, false)]
        [InlineData(0, false, false)]
        [InlineData(-25.00, false, true)]
        public void ProfitabilityProperties_ReturnCorrectValues(decimal lineProfit, bool expectedProfitable, bool expectedHasLoss)
        {
            // Arrange
            var lineFinancials = new LineFinancials(10, 25.50m, 8.25m, 21.04m, lineProfit, 255.00m);
            
            // Act & Assert
            Assert.Equal(expectedProfitable, lineFinancials.IsProfitable);
            Assert.Equal(expectedHasLoss, lineFinancials.HasLoss);
        }
        
        [Fact]
        public void ProfitMarginPercentage_WithPositiveExtendedPrice_ReturnsCorrectPercentage()
        {
            // Arrange
            var lineFinancials = new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m);
            
            // Act
            var marginPercentage = lineFinancials.ProfitMarginPercentage;
            
            // Assert
            Assert.NotNull(marginPercentage);
            var expectedPercentage = (55.00m / 255.00m) * 100m;
            Assert.Equal(expectedPercentage, marginPercentage.Value, 2);
        }
        
        [Fact]
        public void ProfitMarginPercentage_WithZeroExtendedPrice_ReturnsNull()
        {
            // Arrange
            var lineFinancials = LineFinancials.CreateFreeItems(5);
            
            // Act
            var marginPercentage = lineFinancials.ProfitMarginPercentage;
            
            // Assert
            Assert.Null(marginPercentage);
        }
        
        [Fact]
        public void ProfitPerItem_ReturnsCorrectValue()
        {
            // Arrange
            var lineFinancials = new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m);
            
            // Act
            var profitPerItem = lineFinancials.ProfitPerItem;
            
            // Assert
            Assert.Equal(5.50m, profitPerItem); // 55.00 / 10
        }
        
        [Fact]
        public void EffectiveTaxRate_WithPositiveExtendedPrice_ReturnsCorrectRate()
        {
            // Arrange
            var lineFinancials = new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m);
            
            // Act
            var effectiveTaxRate = lineFinancials.EffectiveTaxRate;
            
            // Assert
            var expectedRate = (21.04m / 255.00m) * 100m;
            Assert.Equal(expectedRate, effectiveTaxRate, 2);
        }
        
        #endregion
        
        #region Immutable Update Tests
        
        [Fact]
        public void WithQuantity_CreatesNewInstanceWithUpdatedQuantity()
        {
            // Arrange
            var original = LineFinancials.Calculate(10, 25.50m, 8.25m, 20.00m);
            const int newQuantity = 15;
            const decimal costPrice = 20.00m;
            
            // Act
            var updated = original.WithQuantity(newQuantity, costPrice);
            
            // Assert
            Assert.NotSame(original, updated);
            Assert.Equal(10, original.Quantity); // Original unchanged
            Assert.Equal(newQuantity, updated.Quantity);
            Assert.Equal(25.50m, updated.UnitPrice);
            Assert.Equal(382.50m, updated.ExtendedPrice); // 15 * 25.50
        }
        
        [Fact]
        public void WithUnitPrice_CreatesNewInstanceWithUpdatedUnitPrice()
        {
            // Arrange
            var original = LineFinancials.Calculate(10, 25.50m, 8.25m, 20.00m);
            const decimal newUnitPrice = 30.00m;
            const decimal costPrice = 20.00m;
            
            // Act
            var updated = original.WithUnitPrice(newUnitPrice, costPrice);
            
            // Assert
            Assert.NotSame(original, updated);
            Assert.Equal(25.50m, original.UnitPrice); // Original unchanged
            Assert.Equal(newUnitPrice, updated.UnitPrice);
            Assert.Equal(10, updated.Quantity);
            Assert.Equal(300.00m, updated.ExtendedPrice); // 10 * 30.00
        }
        
        #endregion
        
        #region Validation Tests
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Constructor_WithInvalidQuantity_ThrowsArgumentException(int invalidQuantity)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(invalidQuantity, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m));
            
            Assert.Equal("quantity", exception.ParamName);
            Assert.Contains("must be positive", exception.Message);
        }
        
        [Theory]
        [InlineData(-0.01)]
        [InlineData(-25.50)]
        public void Constructor_WithNegativeUnitPrice_ThrowsArgumentException(decimal negativeUnitPrice)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, negativeUnitPrice, 8.25m, 21.04m, 55.00m, 255.00m));
            
            Assert.Equal("unitPrice", exception.ParamName);
            Assert.Contains("cannot be negative", exception.Message);
        }
        
        [Theory]
        [InlineData(-0.01)]
        [InlineData(-8.25)]
        public void Constructor_WithNegativeTaxRate_ThrowsArgumentException(decimal negativeTaxRate)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, 25.50m, negativeTaxRate, 21.04m, 55.00m, 255.00m));
            
            Assert.Equal("taxRate", exception.ParamName);
            Assert.Contains("cannot be negative", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithTaxRateOver100_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, 25.50m, 100.01m, 21.04m, 55.00m, 255.00m));
            
            Assert.Equal("taxRate", exception.ParamName);
            Assert.Contains("cannot exceed 100%", exception.Message);
        }
        
        [Theory]
        [InlineData(-0.01)]
        [InlineData(-21.04)]
        public void Constructor_WithNegativeTaxAmount_ThrowsArgumentException(decimal negativeTaxAmount)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, 25.50m, 8.25m, negativeTaxAmount, 55.00m, 255.00m));
            
            Assert.Equal("taxAmount", exception.ParamName);
            Assert.Contains("cannot be negative", exception.Message);
        }
        
        [Theory]
        [InlineData(-0.01)]
        [InlineData(-255.00)]
        public void Constructor_WithNegativeExtendedPrice_ThrowsArgumentException(decimal negativeExtendedPrice)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, negativeExtendedPrice));
            
            Assert.Equal("extendedPrice", exception.ParamName);
            Assert.Contains("cannot be negative", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithInconsistentExtendedPrice_ThrowsArgumentException()
        {
            // Arrange - Extended price should be 255.00 (10 * 25.50), but provide 300.00
            const decimal inconsistentExtendedPrice = 300.00m;
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, inconsistentExtendedPrice));
            
            Assert.Contains("Extended price is inconsistent", exception.Message);
        }
        
        [Fact]
        public void Constructor_WithInconsistentTaxAmount_ThrowsArgumentException()
        {
            // Arrange - Tax amount should be ~21.04 (255.00 * 0.0825), but provide 30.00
            const decimal inconsistentTaxAmount = 30.00m;
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new LineFinancials(10, 25.50m, 8.25m, inconsistentTaxAmount, 55.00m, 255.00m));
            
            Assert.Contains("Tax amount is inconsistent", exception.Message);
        }
        
        #endregion
        
        #region Equality Tests
        
        [Fact]
        public void Equals_WithIdenticalLineFinancials_ReturnsTrue()
        {
            // Arrange
            var lineFinancials1 = new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m);
            var lineFinancials2 = new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m);
            
            // Act & Assert
            Assert.True(lineFinancials1.Equals(lineFinancials2));
            Assert.True(lineFinancials1 == lineFinancials2);
            Assert.False(lineFinancials1 != lineFinancials2);
            Assert.Equal(lineFinancials1.GetHashCode(), lineFinancials2.GetHashCode());
        }
        
        [Fact]
        public void Equals_WithDifferentQuantity_ReturnsFalse()
        {
            // Arrange
            var lineFinancials1 = LineFinancials.Calculate(10, 25.50m, 8.25m, 20.00m);
            var lineFinancials2 = LineFinancials.Calculate(15, 25.50m, 8.25m, 20.00m);
            
            // Act & Assert
            Assert.False(lineFinancials1.Equals(lineFinancials2));
        }
        
        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var lineFinancials = TestDataBuilder.ValidLineFinancials();
            
            // Act & Assert
            Assert.False(lineFinancials.Equals(null));
            Assert.False(lineFinancials == null);
            Assert.True(lineFinancials != null);
        }
        
        #endregion
        
        #region ToString Tests
        
        [Fact]
        public void ToString_WithValidUnitPrice_FormatsCorrectly()
        {
            // Arrange
            var lineFinancials = new LineFinancials(10, 25.50m, 8.25m, 21.04m, 55.00m, 255.00m);
            
            // Act
            var result = lineFinancials.ToString();
            
            // Assert
            Assert.Contains("10 ×", result);
            Assert.Contains("$25.50", result);
            Assert.Contains("$255.00", result);
            Assert.Contains("Tax: $21.04", result);
            Assert.Contains("+$55.00", result); // Positive profit
        }
        
        [Fact]
        public void ToString_WithFreeItem_ShowsFree()
        {
            // Arrange
            var lineFinancials = LineFinancials.CreateFreeItems(5, 15.00m);
            
            // Act
            var result = lineFinancials.ToString();
            
            // Assert
            Assert.Contains("5 ×", result);
            Assert.Contains("FREE", result);
            Assert.Contains("$0.00", result);
            Assert.Contains("($75.00)", result); // Negative profit (loss) - check actual format
        }
        
        #endregion
    }
} 