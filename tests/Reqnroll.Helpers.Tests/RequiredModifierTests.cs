namespace Reqnroll.Helpers.Tests;

[TestFixture]
public class RequiredModifierTests
{
    #region Test Models with Required Members

    internal class SimpleRequiredProperty
    {
        public required string RequiredProperty { get; set; }
    }

    internal class MultipleRequiredProperties
    {
        public required string Name { get; set; }
        public required int Age { get; set; }
        public required string Email { get; set; }
        public string? OptionalField { get; set; }
    }

    internal class RequiredWithReadOnly
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }

    internal class MixedRequiredAndNormal
    {
        public required string MandatoryField { get; set; }
        public string NormalField { get; set; } = string.Empty;
        public int? NullableField { get; set; }
    }

    internal class NestedRequiredProperties
    {
        public required string OuterId { get; set; }
        public required InnerClass Inner { get; set; }

        internal class InnerClass
        {
            public required string InnerId { get; set; }
            public string? Description { get; set; }
        }
    }

    internal class RequiredFieldNotProperty
    {
        public required string RequiredField;
        public string NormalProperty { get; set; } = string.Empty;
    }

    #endregion

    #region CreateInstanceWithReadOnlySupport - Vertical Format Tests

    [Test]
    public void CreateInstance_WithSingleRequiredProperty_Vertical_SetsValueCorrectly()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("RequiredProperty", "TestValue");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<SimpleRequiredProperty>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RequiredProperty, Is.EqualTo("TestValue"));
    }

    [Test]
    public void CreateInstance_WithMultipleRequiredProperties_Vertical_SetsAllValuesCorrectly()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("Name", "John Doe");
        table.AddRow("Age", "30");
        table.AddRow("Email", "john@example.com");
        table.AddRow("OptionalField", "Optional");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<MultipleRequiredProperties>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("John Doe"));
        Assert.That(result.Age, Is.EqualTo(30));
        Assert.That(result.Email, Is.EqualTo("john@example.com"));
        Assert.That(result.OptionalField, Is.EqualTo("Optional"));
    }

    [Test]
    public void CreateInstance_WithRequiredAndReadOnlyInit_Vertical_SetsValuesCorrectly()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("Id", "ID123");
        table.AddRow("Name", "Test Item");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<RequiredWithReadOnly>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("ID123"));
        Assert.That(result.Name, Is.EqualTo("Test Item"));
        // When using RuntimeHelpers.GetUninitializedObject, property initializers don't execute
        // So CreatedAt will be default(DateTime) unless explicitly set in the table
        Assert.That(result.CreatedAt, Is.EqualTo(default(DateTime)));
    }

    [Test]
    public void CreateInstance_WithMixedRequiredAndNormal_Vertical_SetsAllValuesCorrectly()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("MandatoryField", "MustHave");
        table.AddRow("NormalField", "CanBeSet");
        table.AddRow("NullableField", "42");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<MixedRequiredAndNormal>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MandatoryField, Is.EqualTo("MustHave"));
        Assert.That(result.NormalField, Is.EqualTo("CanBeSet"));
        Assert.That(result.NullableField, Is.EqualTo(42));
    }

    #endregion

    #region CreateInstanceWithReadOnlySupport - Horizontal Format Tests

    [Test]
    public void CreateInstance_WithSingleRequiredProperty_Horizontal_SetsValueCorrectly()
    {
        // Arrange
        var table = new DataTable("RequiredProperty");
        table.AddRow("HorizontalValue");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<SimpleRequiredProperty>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RequiredProperty, Is.EqualTo("HorizontalValue"));
    }

    [Test]
    public void CreateInstance_WithMultipleRequiredProperties_Horizontal_SetsAllValuesCorrectly()
    {
        // Arrange
        var table = new DataTable("Name", "Age", "Email");
        table.AddRow("Alice Smith", "28", "alice@example.com");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<MultipleRequiredProperties>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Alice Smith"));
        Assert.That(result.Age, Is.EqualTo(28));
        Assert.That(result.Email, Is.EqualTo("alice@example.com"));
    }

    [Test]
    public void CreateInstance_WithRequiredAndReadOnlyInit_Horizontal_SetsValuesCorrectly()
    {
        // Arrange
        var table = new DataTable("Id", "Name");
        table.AddRow("H-ID456", "Horizontal Item");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<RequiredWithReadOnly>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("H-ID456"));
        Assert.That(result.Name, Is.EqualTo("Horizontal Item"));
        // Property initializers don't run when bypassing constructor
        Assert.That(result.CreatedAt, Is.EqualTo(default(DateTime)));
    }

    [Test]
    public void CreateInstance_WithMixedRequiredAndNormal_Horizontal_SetsAllValuesCorrectly()
    {
        // Arrange
        var table = new DataTable("MandatoryField", "NormalField", "NullableField");
        table.AddRow("Required", "Normal", "99");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<MixedRequiredAndNormal>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MandatoryField, Is.EqualTo("Required"));
        Assert.That(result.NormalField, Is.EqualTo("Normal"));
        Assert.That(result.NullableField, Is.EqualTo(99));
    }

    #endregion

    #region CreateSetWithReadOnlySupport with Required Members Tests

    [Test]
    public void CreateSet_WithRequiredProperties_UsingFactory_CreatesMultipleInstances()
    {
        // Arrange
        var table = new DataTable("Name", "Age", "Email");
        table.AddRow("John Doe", "30", "john@example.com");
        table.AddRow("Jane Smith", "25", "jane@example.com");
        table.AddRow("Bob Johnson", "35", "bob@example.com");

        // Act - Using factory as workaround
        var result = table.CreateSetWithReadOnlySupport(() => new MultipleRequiredProperties
        {
            Name = string.Empty,
            Age = 0,
            Email = string.Empty
        });

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));

        Assert.That(result[0].Name, Is.EqualTo("John Doe"));
        Assert.That(result[0].Age, Is.EqualTo(30));
        Assert.That(result[0].Email, Is.EqualTo("john@example.com"));

        Assert.That(result[1].Name, Is.EqualTo("Jane Smith"));
        Assert.That(result[1].Age, Is.EqualTo(25));
        Assert.That(result[1].Email, Is.EqualTo("jane@example.com"));

        Assert.That(result[2].Name, Is.EqualTo("Bob Johnson"));
        Assert.That(result[2].Age, Is.EqualTo(35));
        Assert.That(result[2].Email, Is.EqualTo("bob@example.com"));
    }

    #endregion

    #region Edge Cases and Special Scenarios

    [Test]
    public void CreateInstance_WithPartialRequiredProperties_OnlySetsProvidedValues()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("Name", "Partial Test");
        // Age and Email are required but not provided in table

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<MultipleRequiredProperties>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Partial Test"));
        // Age and Email will have default values since RuntimeHelpers.GetUninitializedObject is used
        Assert.That(result.Age, Is.EqualTo(0));
        Assert.That(result.Email, Is.Null);
    }

    [Test]
    public void CreateInstance_WithRequiredField_SetsFieldValue()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("RequiredField", "FieldValue");
        table.AddRow("NormalProperty", "PropertyValue");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<RequiredFieldNotProperty>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RequiredField, Is.EqualTo("FieldValue"));
        Assert.That(result.NormalProperty, Is.EqualTo("PropertyValue"));
    }

    [Test]
    public void CreateInstance_WithFactoryMethod_StillWorksForRequiredTypes()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("RequiredProperty", "FactoryValue");

        // Act - Using factory even though not needed anymore
        var result = table.CreateInstanceWithReadOnlySupport(() => new SimpleRequiredProperty
        {
            RequiredProperty = "Initial"
        });

        // Assert - Table values should override factory values
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RequiredProperty, Is.EqualTo("FactoryValue"));
    }

    [Test]
    public void CreateInstance_CompareFactoryVsAutomatic_BothProduceSameResult()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("name", "Test Name");
        table.AddRow("Age", "42");
        table.AddRow("Email", "test@example.com");

        // Act
        var resultAuto = table.CreateInstanceWithReadOnlySupport<MultipleRequiredProperties>();
        var resultFactory = table.CreateInstanceWithReadOnlySupport(() => new MultipleRequiredProperties
        {
            Name = string.Empty,
            Age = 0,
            Email = string.Empty
        });

        // Assert
        Assert.That(resultAuto.Name, Is.EqualTo(resultFactory.Name));
        Assert.That(resultAuto.Age, Is.EqualTo(resultFactory.Age));
        Assert.That(resultAuto.Email, Is.EqualTo(resultFactory.Email));
    }

    #endregion

    #region Normal Types Still Work (Regression Tests)

    internal class NormalClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Test]
    public void CreateInstance_WithNormalClassNoRequiredMembers_StillWorks()
    {
        // Arrange
        var table = new DataTable("Property", "Value");
        table.AddRow("Name", "Normal User");
        table.AddRow("Age", "25");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<NormalClass>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Normal User"));
        Assert.That(result.Age, Is.EqualTo(25));
    }

    [Test]
    public void CreateInstance_WithNormalClassNoRequiredMembers_Horizontal_StillWorks()
    {
        // Arrange
        var table = new DataTable("Name", "Age");
        table.AddRow("Normal User 2", "30");

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<NormalClass>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Normal User 2"));
        Assert.That(result.Age, Is.EqualTo(30));
    }

    #endregion

    #region CreateInstanceWithReadOnlySupport - Additional Tests

    [Test]
    public void CreateInstance_WithRequiredAndReadOnlyInit_WithExplicitDateTime_SetsValueFromTable()
    {
        // Arrange
        var expectedDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var table = new DataTable("Property", "Value");
        table.AddRow("Id", "ID789");
        table.AddRow("Name", "Test Item with Date");
        table.AddRow("CreatedAt", expectedDate.ToString("O")); // ISO 8601 format

        // Act
        var result = table.CreateInstanceWithReadOnlySupport<RequiredWithReadOnly>();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("ID789"));
        Assert.That(result.Name, Is.EqualTo("Test Item with Date"));
        Assert.That(result.CreatedAt, Is.EqualTo(expectedDate));
    }

    #endregion
}