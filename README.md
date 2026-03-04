# Reqnroll.Helpers

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-14.0-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

A powerful utility library that extends [Reqnroll](https://reqnroll.net/) DataTable capabilities, providing seamless object mapping with support for read-only properties, computed properties, and flexible initialization patterns.

## 🚀 Features

- ✅ **Read-only Property Support** - Map to properties with `init` accessors and backing fields
- ✅ **Computed Property Handling** - Automatically skips get-only computed properties
- ✅ **Dual Table Format** - Supports both horizontal and vertical DataTable formats
- ✅ **Factory Method Support** - Custom object initialization with constructor parameters
- ✅ **Type Conversion** - Automatic conversion for primitives, DateTime, Guid, and more
- ✅ **Flexible Access Modifiers** - Works with public, internal, protected, and private properties
- ✅ **.NET 10 & C# 14** - Built with the latest .NET features

## 📦 Installation
dotnet add package Reqnroll.Helpers

Or via NuGet Package Manager:

Install-Package Reqnroll.Helpers


## 🎯 Quick Start

### Basic Usage - Horizontal Format
```
using Reqnroll.Helpers;

[Binding] 
public class PersonSteps 
{ 
    [Given(@"I have the following persons")] 
    public void GivenIHaveTheFollowingPersons(DataTable table) 
    { 
    // | FirstName | LastName | Age | 
    // | John      | Doe      | 30  | 
    // | Jane      | Smith    | 25  |

    var persons = table.CreateSetWithReadOnlySupport<Person>();
    
        // persons[0].FirstName == "John"
        // persons[1].FirstName == "Jane"
    }
}

public class Person 
{ 
    public string FirstName { get; init; } = string.Empty; 
    public string LastName { get; init; } = string.Empty; 
    public int Age { get; init; } 
}
```

### Vertical Format (Property/Value)
```
[Given(@"I have a person with the following details")] 
public void GivenIHaveAPerson(DataTable table) 
{
    // | Property  | Value | 
    // | FirstName | Alice | 
    // | LastName  | Brown | 
    // | Age       | 28    |
var person = table.CreateInstanceWithReadOnlySupport<Person>();

// person.FirstName == "Alice"
}
```

## 📚 Advanced Usage

### Factory Method for Custom Initialization
```
public class Order 
{ 
    public Order(string customerId) 
    {
        CustomerId = customerId; 
        OrderId = $"ORD-{Guid.NewGuid():N}"; 
    }

    public string OrderId { get; }
    public string CustomerId { get; }
    public decimal TotalAmount { get; set; }
}

[Given(@"I have the following orders for customer '(.*)'")] 
public void GivenIHaveOrders(string customerId, DataTable table) 
{ 
    // | TotalAmount | 
    // | 100.50      | 
    // | 250.00      |
var orders = table.CreateSetWithReadOnlySupport(() => new Order(customerId));

// Each order has the customerId and a unique OrderId
}
```

### Computed Properties - Reqnroll Assertion Pattern
```
public class Person 
{ 
    public string FirstName { get; set; } = string.Empty; 
    public string LastName { get; set; } = string.Empty; 
    public int Age { get; set; }

    // Computed properties - automatically calculated
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsAdult => Age >= 18;
    public string Status => IsAdult ? "Adult" : "Minor";
}

[Then(@"I should see the following persons")] 
public void ThenIShouldSeeTheFollowingPersons(DataTable expectedTable) 
{ 
    // | FirstName | LastName | Age | IsAdult | Status | 
    // | John      | Doe      | 25  | true    | Adult  | 
    // | Jane      | Smith    | 17  | false   | Minor  |
    var expected = expectedTable.CreateSetWithReadOnlySupport<Person>();

    // Computed properties (IsAdult, Status) are automatically calculated
    // No need to manually set them - they just work!
    Assert.That(actual, Is.EqualTo(expected));
}
```

### Multiple Data Types
```
public class Product 
{ 
    public string Name { get; set; } = string.Empty; 
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } 
    public DateTime ReleaseDate { get; set; }
    public Guid ProductId { get; set; } 
}
[Given(@"I have the following products")] 
public void GivenIHaveProducts(DataTable table) 
{ 
    // | Name   | Price | IsAvailable | ReleaseDate | ProductId                            |
    // | Widget | 19.99 | true        | 2026-03-15  | 550e8400-e29b-41d4-a716-446655440000 |

    var products = table.CreateSetWithReadOnlySupport<Product>();

    // Automatic type conversion for all standard types
}
```

## 🔧 API Reference

### Extension Methods

#### `CreateSetWithReadOnlySupport<T>()`
Creates a list of objects from a horizontal DataTable.

public static List<T> CreateSetWithReadOnlySupport<T>(this DataTable table) where T : new()

**Example:**
```
// | Name | Age | 
// | John | 30  | 
// | Jane | 25  | 

var list = table.CreateSetWithReadOnlySupport<Person>();
```
#### `CreateSetWithReadOnlySupport<T>(Func<T>)`
Creates a list of objects using a custom factory method.

public static List<T> CreateSetWithReadOnlySupport<T>(this DataTable table, Func<T> factory)

**Example:**
```
var list = table.CreateSetWithReadOnlySupport(() => new Person("DefaultCompany"));
```

#### `CreateInstanceWithReadOnlySupport<T>()`
Creates a single instance from a DataTable (horizontal or vertical format).

public static T CreateInstanceWithReadOnlySupport<T>(this DataTable table) where T : new()

**Example (Horizontal):**
```
// | Name | Age | 
// | John | 30  | 

var person = table.CreateInstanceWithReadOnlySupport<Person>();
```

**Example (Vertical):**
```
// | Property | Value | 
// | Name     | John  | 
// | Age      | 30    |
var person = table.CreateInstanceWithReadOnlySupport<Person>();
```

#### `CreateInstanceWithReadOnlySupport<T>(Func<T>)`
Creates a single instance using a custom factory method.

public static T CreateInstanceWithReadOnlySupport<T>(this DataTable table, Func<T> factory)

**Example:**
```
var person = table.CreateInstanceWithReadOnlySupport(() => new Person("DefaultCompany"));
```

## 🎨 Supported Scenarios

### ✅ Property Types
- `set` - Regular settable properties
- `init` - Init-only properties
- `private set` - Private setters
- Computed properties (get-only) - Automatically skipped
- All access modifiers (public, internal, protected, private)

### ✅ Data Types
- Primitives: `int`, `long`, `double`, `decimal`, `bool`
- `string`
- `DateTime`
- `Guid`
- Nullable types: `int?`, `bool?`, `DateTime?`
- Custom types via Reqnroll's Value Retrievers

### ✅ Table Formats
- **Horizontal**: Headers as property names, rows as instances
- **Vertical**: Two columns - "Property" and "Value"

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built for [Reqnroll](https://reqnroll.net/) - the .NET BDD framework
- Inspired by Reqnroll's table helpers

## 📮 Support

- 🐛 [Report Issues](https://github.com/taja01/Reqnroll.Helpers/issues)
- 💬 [Discussions](https://github.com/taja01/Reqnroll.Helpers/discussions)
- ⭐ If you find this helpful, please star the repository!

---

Made with ❤️ for the Reqnroll community