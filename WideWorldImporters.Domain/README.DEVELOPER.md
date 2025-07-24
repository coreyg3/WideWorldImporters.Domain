# WideWorldImporters Domain Model

## About WideWorldImporters

This project is a .NET domain model implementation based on **Microsoft's WideWorldImporters (WWI) sample database**. WideWorldImporters is a comprehensive sample database that showcases modern database design patterns, features, and techniques for SQL Server 2016 and later versions.

### What is WideWorldImporters?

WideWorldImporters represents a fictional wholesale novelty goods importer and distributor operating from the San Francisco bay area. The company sells to:
- **Retail customers** (stores that sell to consumers)
- **Other wholesalers** (companies that sell to stores)

The sample includes both:
- **OLTP database** - For day-to-day operational workloads
- **OLAP database** - For analytical and reporting workloads

### Business Domain Overview

The WideWorldImporters business operates across several key functional areas:

#### **Sales Operations**
- Customer management (retail customers, buying groups, customer categories)
- Order processing and fulfillment 
- Invoicing and billing
- Special deals and pricing arrangements

#### **Purchasing Operations**
- Supplier relationship management
- Purchase order processing
- Supplier transactions and payments
- Supplier categorization

#### **Warehouse Management**
- Stock item catalog and inventory
- Stock movements and transactions
- Cold room temperature monitoring
- Vehicle temperature tracking
- Packaging configurations

#### **Shared Services**
- Geographic data (countries, states, cities)
- Logistics (delivery methods, package types)
- People and organizational structure
- System parameters and configuration

### Official Microsoft Resources

- **GitHub Repository**: [microsoft/sql-server-samples](https://github.com/microsoft/sql-server-samples)
- **Download Links**: Available in the repository's [Releases section](https://github.com/microsoft/sql-server-samples/releases)
- **Documentation**: Comprehensive samples and setup guides in the official repository

This domain model extracts the core business entities and relationships from the original database schema, implementing them using modern .NET domain-driven design patterns.

---

# WideWorldImporters Domain Architecture

## Table of Contents
- [Overview](#overview)
- [Foreign Key ID Approach](#foreign-key-id-approach)
- [Value Objects Pattern](#value-objects-pattern)
- [Design Principles](#design-principles)
- [Trade-offs and Alternatives](#trade-offs-and-alternatives)
- [Implementation Guidelines](#implementation-guidelines)

## Overview

This domain model implements two key architectural patterns that prioritize **explicit dependencies**, **performance predictability**, and **bounded context isolation** over developer convenience:

1. **Foreign Key IDs** instead of navigation properties
2. **Value Objects** for complex business concepts

These decisions reflect a mature, distributed-systems-ready architecture that emphasizes long-term maintainability and scalability.

## Foreign Key ID Approach

### What It Is

Instead of using navigation properties like this:
```csharp
// ❌ Navigation Property Approach
public class Order
{
    public Customer Customer { get; set; }
    public Person Salesperson { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
}
```

We use foreign key identifiers:
```csharp
// ✅ Foreign Key ID Approach
public class Order
{
    public int CustomerId { get; private set; }
    public int SalespersonPersonId { get; private set; }
    public int DeliveryMethodId { get; private set; }
}
```

### Architectural Reasoning

#### 1. **Aggregate Boundary Enforcement**
Following Domain-Driven Design principles, entities should only reference other aggregates by their identifiers, not by object references. This enforces clear boundaries and prevents inadvertent coupling.

```csharp
// Clear aggregate boundary - Order references Customer by ID only
public class Order
{
    public int CustomerId { get; private set; }
    
    // Business logic works with IDs, maintaining encapsulation
    public bool CanChangePricing() => 
        SomeBusinessRule(CustomerId, SalespersonPersonId);
}
```

#### 2. **Performance Predictability**
- **Memory Footprint**: Only primitive values are loaded, eliminating object graph memory overhead
- **Query Performance**: No risk of N+1 queries or unintended lazy loading
- **Caching Strategy**: Simpler per-entity-type caching without complex dependency invalidation

#### 3. **Microservices Readiness**
- **Service Boundaries**: Natural fit for service-to-service communication via IDs
- **Event Payloads**: Events contain IDs rather than full object graphs
- **Data Consistency**: Eventual consistency patterns work naturally with ID references

#### 4. **Testing and Mocking Simplification**
```csharp
// ✅ Simple unit test - no mocking required
[Test]
public void CanPlaceOnCreditHold_WhenNotAlreadyOnHold()
{
    var customer = new Customer(/* parameters with simple values */);
    customer.PlaceOnCreditHold(editedBy: 123);
    Assert.IsTrue(customer.IsOnCreditHold);
}

// ❌ Complex setup required with navigation properties
[Test]
public void CanPlaceOnCreditHold_WithNavigationProperties()
{
    var mockPerson = Mock.Of<Person>();
    var mockCategory = Mock.Of<CustomerCategory>();
    // ... extensive mocking setup required
}
```

#### 5. **Serialization and API Safety**
- **No Circular References**: JSON serialization never encounters circular dependency issues
- **Explicit Loading**: Developers must explicitly decide what related data to load
- **Bandwidth Control**: API responses contain only necessary data

### When This Approach Excels

- **High-performance systems** requiring predictable memory usage
- **Distributed architectures** with service boundaries
- **Event-driven systems** where events contain entity references
- **Large development teams** needing clear module boundaries
- **Legacy system integration** where database schemas use foreign keys

## Value Objects Pattern

### What It Is

Value Objects encapsulate related attributes that define a concept by its values rather than identity:

```csharp
// ❌ Primitive Obsession
public class Customer
{
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string PostalCode { get; set; }
    public int CityId { get; set; }
    
    public string PhoneNumber { get; set; }
    public string FaxNumber { get; set; }
    public string WebsiteUrl { get; set; }
}

// ✅ Value Objects
public class Customer
{
    public Address DeliveryAddress { get; private set; }
    public Address PostalAddress { get; private set; }
    public ContactInformation ContactInformation { get; private set; }
}
```

### Core Principles

#### 1. **Immutability**
Value objects cannot be modified after creation. Changes require creating new instances:

```csharp
public class Address : IEquatable<Address>
{
    public string AddressLine1 { get; } // Read-only properties
    public string PostalCode { get; }
    
    // Constructor validates and sets values
    public Address(string addressLine1, string postalCode, int cityId) { ... }
}
```

#### 2. **Equality by Value**
Two value objects are equal if all their attributes are equal:

```csharp
public bool Equals(Address? other)
{
    return other is not null &&
           AddressLine1 == other.AddressLine1 &&
           PostalCode == other.PostalCode &&
           CityId == other.CityId;
}
```

#### 3. **Encapsulated Business Logic**
Value objects contain behavior relevant to their concept:

```csharp
public class PackagingConfiguration
{
    public int CalculateOuterPackagesNeeded(int units)
    {
        return (int)Math.Ceiling((double)units / QuantityPerOuter);
    }
    
    public int CalculateTotalUnits(int outerPackages)
    {
        return outerPackages * QuantityPerOuter;
    }
}
```

### Business Benefits

#### 1. **Domain Concept Clarity**
```csharp
// ❌ Unclear what these parameters represent
public void UpdateCustomer(string line1, string line2, string postal, int city)

// ✅ Clear business intent
public void UpdateDeliveryAddress(Address newAddress)
```

#### 2. **Invariant Enforcement**
```csharp
public class LineFinancials
{
    // Constructor ensures financial consistency
    private static void ValidateFinancialConsistency(
        int quantity, decimal? unitPrice, decimal taxRate, 
        decimal taxAmount, decimal extendedPrice)
    {
        var expectedExtendedPrice = (unitPrice ?? 0m) * quantity;
        var expectedTaxAmount = expectedExtendedPrice * (taxRate / 100m);
        
        if (Math.Abs(extendedPrice - expectedExtendedPrice) > 0.01m)
            throw new ArgumentException("Extended price is inconsistent with unit price and quantity.");
    }
}
```

#### 3. **Reusable Business Logic**
Complex calculations are centralized and reusable:

```csharp
public class LineFinancials
{
    public decimal? ProfitMarginPercentage => 
        ExtendedPrice == 0m ? null : (LineProfit / ExtendedPrice) * 100m;
        
    public bool IsProfitable => LineProfit > 0m;
    
    public string PerformanceCategory => ExtendedPrice switch
    {
        0 => "Free Item",
        <= 50 => "Low Value",
        <= 500 => "Medium Value",
        <= 2000 => "High Value",
        _ => "Premium Value"
    };
}
```

## Design Principles

### 1. **Explicit Over Implicit**
- Relationships are explicitly referenced by ID
- Data loading is explicitly controlled
- Dependencies are explicitly declared

### 2. **Performance Over Convenience**
- Predictable memory usage over automatic object loading
- Controlled query patterns over convenient navigation
- Explicit serialization over automatic graph traversal

### 3. **Boundaries Over Coupling**
- Clear aggregate boundaries enforced by ID references
- Service boundaries naturally supported
- Module independence maintained

### 4. **Immutability Over Mutation**
- Value objects are immutable by design
- Entity state changes through explicit methods
- Temporal consistency through careful state management

## Trade-offs and Alternatives

### Foreign Key IDs vs. Navigation Properties

| Aspect | Foreign Key IDs | Navigation Properties |
|--------|-----------------|----------------------|
| **Memory Usage** | ✅ Minimal | ❌ Potentially large object graphs |
| **Query Performance** | ✅ Predictable | ❌ Risk of N+1 problems |
| **Development Speed** | ❌ More explicit code | ✅ Faster for simple scenarios |
| **Testing** | ✅ Simple unit tests | ❌ Complex mocking setup |
| **Microservices** | ✅ Natural fit | ❌ Object graphs don't serialize well |
| **Caching** | ✅ Simple strategies | ❌ Complex invalidation |

### When to Consider Navigation Properties
- **Rapid prototyping** where development speed is critical
- **Small, monolithic applications** with simple data access patterns
- **Reporting scenarios** requiring extensive data traversal
- **Teams new to DDD** who need gentler learning curves

### Value Objects vs. Primitive Properties

| Aspect | Value Objects | Primitive Properties |
|--------|---------------|---------------------|
| **Type Safety** | ✅ Compile-time validation | ❌ Runtime errors possible |
| **Business Logic** | ✅ Centralized | ❌ Scattered across application |
| **Validation** | ✅ Constructor validation | ❌ Repeated validation logic |
| **Testing** | ✅ Isolated business logic tests | ❌ Validation testing scattered |
| **Refactoring** | ✅ Safe, compiler-enforced | ❌ Manual, error-prone |

## Implementation Guidelines

### Creating New Entities

```csharp
public class NewEntity
{
    // ✅ Use foreign key IDs for references
    public int RelatedEntityId { get; private set; }
    
    // ✅ Use value objects for complex concepts
    public ComplexConcept ConceptData { get; private set; }
    
    // ✅ Private setters for encapsulation
    public string SimpleProperty { get; private set; }
    
    // ✅ Constructor with validation
    public NewEntity(int relatedEntityId, ComplexConcept conceptData)
    {
        if (relatedEntityId <= 0)
            throw new ArgumentException("Related entity ID must be valid.");
            
        RelatedEntityId = relatedEntityId;
        ConceptData = conceptData ?? throw new ArgumentNullException();
    }
    
    // ✅ Explicit business methods
    public void UpdateConcept(ComplexConcept newConcept, int editedBy)
    {
        ArgumentNullException.ThrowIfNull(newConcept);
        ValidateEditor(editedBy);
        
        ConceptData = newConcept;
        LastEditedBy = editedBy;
    }
}
```

### Creating New Value Objects

```csharp
public class NewValueObject : IEquatable<NewValueObject>
{
    // ✅ Readonly properties
    public string Property1 { get; }
    public decimal Property2 { get; }
    
    // ✅ Constructor with validation
    public NewValueObject(string property1, decimal property2)
    {
        if (string.IsNullOrWhiteSpace(property1))
            throw new ArgumentException("Property1 cannot be empty.");
        if (property2 < 0)
            throw new ArgumentException("Property2 cannot be negative.");
            
        Property1 = property1.Trim();
        Property2 = property2;
    }
    
    // ✅ Business behavior methods
    public decimal CalculateSomething() => Property2 * 1.1m;
    
    // ✅ Immutable updates
    public NewValueObject WithProperty1(string newProperty1) =>
        new(newProperty1, Property2);
    
    // ✅ Value equality implementation
    public bool Equals(NewValueObject? other) =>
        other is not null &&
        Property1 == other.Property1 &&
        Property2 == other.Property2;
        
    public override bool Equals(object? obj) => Equals(obj as NewValueObject);
    public override int GetHashCode() => HashCode.Combine(Property1, Property2);
    
    // ✅ Meaningful string representation
    public override string ToString() => $"{Property1}: {Property2:C}";
}
```

### Loading Related Data

When you need related entity data, load it explicitly in your application services:

```csharp
public class OrderService
{
    public async Task<OrderDetailViewModel> GetOrderDetails(int orderId)
    {
        var order = await _orderRepository.GetById(orderId);
        var customer = await _customerRepository.GetById(order.CustomerId);
        var salesperson = await _personRepository.GetById(order.SalespersonPersonId);
        
        return new OrderDetailViewModel
        {
            Order = order,
            Customer = customer,
            Salesperson = salesperson
        };
    }
}
```

---

This architecture prioritizes **explicit design**, **predictable performance**, and **clear boundaries** over developer convenience. The result is a domain model that scales well, tests easily, and maintains clear separation of concerns as the system grows in complexity. 