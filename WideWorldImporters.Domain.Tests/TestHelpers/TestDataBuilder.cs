using WideWorldImporters.Domain.ValueObjects;

namespace WideWorldImporters.Domain.Tests.TestHelpers
{
    /// <summary>
    /// Provides consistent test data for domain testing
    /// </summary>
    public static class TestDataBuilder
    {
        // Common test constants
        public const int ValidPersonId = 1;
        public const int ValidCustomerId = 100;
        public const int ValidCategoryId = 5;
        public const int ValidCityId = 10;
        public const int ValidDeliveryMethodId = 2;
        
        // Valid Address builders
        public static Address ValidAddress() => new(
            addressLine1: "123 Main Street",
            postalCode: "12345",
            cityId: ValidCityId,
            addressLine2: "Suite 100"
        );
        
        public static Address ValidMinimalAddress() => new(
            addressLine1: "456 Oak Avenue",
            postalCode: "67890",
            cityId: ValidCityId
        );
        
        // Valid ContactInformation builders
        public static ContactInformation ValidContactInformation() => new(
            phoneNumber: "(555) 123-4567",
            faxNumber: "(555) 123-4568",
            websiteUrl: "https://example.com"
        );
        
        public static ContactInformation ValidMinimalContactInformation() => new(
            phoneNumber: "(555) 987-6543",
            faxNumber: "(555) 987-6544",
            websiteUrl: "https://minimal.com"
        );
        
        // Valid LineFinancials builders
        public static LineFinancials ValidLineFinancials() => LineFinancials.Calculate(
            quantity: 10,
            unitPrice: 25.50m,
            taxRate: 8.25m,
            costPrice: 20.00m
        );
        
        public static LineFinancials FreeItemLineFinancials() => LineFinancials.CreateFreeItems(
            quantity: 5,
            costPrice: 15.00m
        );
        
        // Date helpers
        public static DateOnly ValidOrderDate() => DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        public static DateOnly ValidDeliveryDate() => DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        public static DateOnly ValidAccountOpenedDate() => DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        
        // String builders for various lengths
        public static string StringOfLength(int length, char character = 'A') => new(character, length);
        
        public static string ValidCustomerName() => "Test Customer Inc.";
        public static string ValidOrderComments() => "Standard delivery instructions";
        public static string ValidPurchaseOrderNumber() => "PO123456";
    }
} 