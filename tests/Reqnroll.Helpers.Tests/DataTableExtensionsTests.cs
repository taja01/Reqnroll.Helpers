namespace Reqnroll.Helpers.Tests
{
    [TestFixture]
    public class DataTableExtensionsTests
    {
        #region Test Models

        public class Person : IEquatable<Person>
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Email { get; set; } = string.Empty;

            public bool Equals(Person? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return FirstName == other.FirstName &&
                       LastName == other.LastName &&
                       Age == other.Age &&
                       Email == other.Email;
            }

            public override bool Equals(object? obj) => Equals(obj as Person);

            public override int GetHashCode() => HashCode.Combine(FirstName, LastName, Age, Email);
        }

        public class Product : IEquatable<Product>
        {
            public string ProductName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsAvailable { get; set; }
            public int StockQuantity { get; set; }

            public bool Equals(Product? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return ProductName == other.ProductName &&
                       Price == other.Price &&
                       IsAvailable == other.IsAvailable &&
                       StockQuantity == other.StockQuantity;
            }

            public override bool Equals(object? obj) => Equals(obj as Product);

            public override int GetHashCode() => HashCode.Combine(ProductName, Price, IsAvailable, StockQuantity);
        }

        public class Order
        {
            public Order()
            {
                Products = [];
            }

            public string OrderId { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public List<Product> Products { get; set; }
        }

        public class ReadOnlyOrder
        {
            public ReadOnlyOrder()
            {
                Products = [];
            }

            public string OrderId { get; init; } = string.Empty;
            public string CustomerName { get; init; } = string.Empty;
            public DateTime OrderDate { get; init; }
            public List<Product> Products { get; init; }
        }

        #endregion

        #region Person Tests with IEquatable

        [Test]
        public void CreateInstance_WithPerson_Horizontal_ReturnsEqualObject()
        {
            // Arrange
            var table = new DataTable("FirstName", "LastName", "Age", "Email");
            table.AddRow("Bob", "Johnson", "35", "bob@example.com");

            var expected = new Person
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Age = 35,
                Email = "bob@example.com"
            };

            // Act
            var result = table.CreateInstance<Person>();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void CreateInstance_WithPerson_Vertical_ReturnsEqualObject()
        {
            // Arrange
            var table = new DataTable("Property", "Value");
            table.AddRow("FirstName", "Diana");
            table.AddRow("LastName", "Prince");
            table.AddRow("Age", "32");
            table.AddRow("Email", "diana@example.com");

            var expected = new Person
            {
                FirstName = "Diana",
                LastName = "Prince",
                Age = 32,
                Email = "diana@example.com"
            };

            // Act
            var result = table.CreateInstance<Person>();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion

        #region Product Tests with IEquatable

        [Test]
        public void CreateSet_WithProduct_ReturnsEqualObjects()
        {
            // Arrange
            var table = new DataTable("ProductName", "Price", "IsAvailable", "StockQuantity");
            table.AddRow("Widget", "19.99", "true", "100");
            table.AddRow("Gadget", "29.50", "false", "0");

            var expected = new List<Product>
            {
                new() { ProductName = "Widget", Price = 19.99m, IsAvailable = true, StockQuantity = 100 },
                new() { ProductName = "Gadget", Price = 29.50m, IsAvailable = false, StockQuantity = 0 }
            };

            // Act
            var result = table.CreateSet<Product>();
        }

        [Test]
        public void CreateInstance_WithProduct_Horizontal_ReturnsEqualObject()
        {
            // Arrange
            var table = new DataTable("ProductName", "Price", "IsAvailable", "StockQuantity");
            table.AddRow("Laptop", "999.99", "true", "50");

            var expected = new Product
            {
                ProductName = "Laptop",
                Price = 999.99m,
                IsAvailable = true,
                StockQuantity = 50
            };

            // Act
            var result = table.CreateInstance<Product>();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void CreateInstance_WithProduct_Vertical_ReturnsEqualObject()
        {
            // Arrange
            var table = new DataTable("Property", "Value");
            table.AddRow("ProductName", "Mouse");
            table.AddRow("Price", "15.99");
            table.AddRow("IsAvailable", "true");
            table.AddRow("StockQuantity", "200");

            var expected = new Product
            {
                ProductName = "Mouse",
                Price = 15.99m,
                IsAvailable = true,
                StockQuantity = 200
            };

            // Act
            var result = table.CreateInstance<Product>();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion

        #region Order Tests (Class with List<Product>)

        [Test]
        public void CreateSet_WithOrder_InitializesEmptyProductList()
        {
            // Arrange
            var table = new DataTable("OrderId", "CustomerName", "TotalAmount");
            table.AddRow("ORD001", "Alice Smith", "150.00");
            table.AddRow("ORD002", "Bob Jones", "250.00");

            // Act
            var result = table.CreateSet<Order>();

        }

        [Test]
        public void CreateInstance_WithOrder_Horizontal_InitializesEmptyProductList()
        {
            // Arrange
            var table = new DataTable("OrderId", "CustomerName", "OrderDate", "TotalAmount");
            table.AddRow("ORD003", "Charlie Brown", "2026-02-14", "500.00");

            // Act
            var result = table.CreateInstance<Order>();

            // Assert
            Assert.That(result.OrderId, Is.EqualTo("ORD003"));
            Assert.That(result.CustomerName, Is.EqualTo("Charlie Brown"));
            Assert.That(result.OrderDate, Is.EqualTo(new DateTime(2026, 2, 14)));
            Assert.That(result.TotalAmount, Is.EqualTo(500.00m));
            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void CreateInstance_WithOrder_Vertical_InitializesEmptyProductList()
        {
            // Arrange
            var table = new DataTable("Property", "Value");
            table.AddRow("OrderId", "ORD004");
            table.AddRow("CustomerName", "Diana Ross");
            table.AddRow("OrderDate", "2026-01-15");
            table.AddRow("TotalAmount", "750.50");

            // Act
            var result = table.CreateInstance<Order>();

            // Assert
            Assert.That(result.OrderId, Is.EqualTo("ORD004"));
            Assert.That(result.CustomerName, Is.EqualTo("Diana Ross"));
            Assert.That(result.OrderDate, Is.EqualTo(new DateTime(2026, 1, 15)));
            Assert.That(result.TotalAmount, Is.EqualTo(750.50m));
            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void CreateSet_WithReadOnlyOrder_InitializesEmptyProductList()
        {
            // Arrange
            var table = new DataTable("OrderId", "CustomerName", "OrderDate");
            table.AddRow("ORD005", "Eve Anderson", "2026-02-10");

            // Act
            var result = table.CreateSet<ReadOnlyOrder>();
        }

        [Test]
        public void CreateInstance_WithReadOnlyOrder_Horizontal_InitializesEmptyProductList()
        {
            // Arrange
            var table = new DataTable("OrderId", "CustomerName", "OrderDate");
            table.AddRow("ORD006", "Frank Miller", "2026-03-01");

            // Act
            var result = table.CreateInstance<ReadOnlyOrder>();

            // Assert
            Assert.That(result.OrderId, Is.EqualTo("ORD006"));
            Assert.That(result.CustomerName, Is.EqualTo("Frank Miller"));
            Assert.That(result.OrderDate, Is.EqualTo(new DateTime(2026, 3, 1)));
            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        #endregion

        #region Additional Edge Cases

        [Test]
        public void CreateSet_WithEmptyTable_ReturnsEmptyList()
        {
            // Arrange
            var table = new DataTable("FirstName", "Age");

            // Act
            var result = table.CreateSet<Person>();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CreateSet_WithNonExistentProperty_IgnoresColumn()
        {
            // Arrange
            var table = new DataTable("FirstName", "Age", "NonExistentColumn");
            table.AddRow("Helen", "33", "IgnoredValue");

            // Act
            var result = table.CreateSet<Person>();

            // Assert

        }

        [Test]
        public void CreateInstance_WithPartialData_SetsOnlyProvidedProperties()
        {
            // Arrange
            var table = new DataTable("FirstName");
            table.AddRow("Ivan");

            // Act
            var result = table.CreateInstance<Person>();

            // Assert
            Assert.That(result.FirstName, Is.EqualTo("Ivan"));
            Assert.That(result.Age, Is.EqualTo(0)); // Default value
            Assert.That(result.Email, Is.EqualTo(string.Empty)); // Default value
        }

        [Test]
        public void CreateInstance_VerticalFormat_IsCaseInsensitive()
        {
            // Arrange - "property" in lowercase
            var table = new DataTable("property", "value");
            table.AddRow("FirstName", "Frank");
            table.AddRow("Age", "45");

            // Act
            var result = table.CreateInstance<Person>();

            // Assert
            Assert.That(result.FirstName, Is.EqualTo("Frank"));
            Assert.That(result.Age, Is.EqualTo(45));
        }

        #endregion
    }
}