namespace Reqnroll.Helpers.Tests
{
    [TestFixture]
    public class DataTableExtensionsCoreLogicTests
    {
        #region Test Models for Core Logic

        public class TypeConversionModel
        {
            public int IntValue { get; set; }
            public decimal DecimalValue { get; set; }
            public double DoubleValue { get; set; }
            public bool BoolValue { get; set; }
            public DateTime DateTimeValue { get; set; }
            public Guid GuidValue { get; set; }
            public string StringValue { get; set; } = string.Empty;
        }

        public class NullableTypeModel
        {
            public int? NullableInt { get; set; }
            public bool? NullableBool { get; set; }
            public DateTime? NullableDateTime { get; set; }
        }

        public class MixedAccessibilityModel
        {
            public string PublicProperty { get; set; } = string.Empty;
            internal string InternalProperty { get; set; } = string.Empty;
            protected string ProtectedProperty { get; set; } = string.Empty;
            private string PrivateProperty { get; set; } = string.Empty;

            public string GetPrivateProperty() => PrivateProperty;
        }

        public class ComputedPropertyModel
        {
            public string FirstName { get; init; } = string.Empty;
            public string LastName { get; init; } = string.Empty;
            public string FullName => $"{FirstName} {LastName}".Trim();
            public int Age { get; init; }
            public bool IsAdult => Age >= 18;
            public bool IsMinor => Age < 18;
        }

        #endregion

        #region Type Conversion Tests

        [Test]
        public void CreateSet_WithVariousDataTypes_ConvertsCorrectly()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            var testDate = new DateTime(2026, 3, 4);

            var table = new DataTable("IntValue", "DecimalValue", "DoubleValue", "BoolValue", "DateTimeValue", "GuidValue", "StringValue");
            table.AddRow("42", "19.99", "3.14159", "true", "2026-03-04", testGuid.ToString(), "Test String");

            // Act
            var result = table.CreateSetWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IntValue, Is.EqualTo(42));
            Assert.That(result[0].DecimalValue, Is.EqualTo(19.99m));
            Assert.That(result[0].DoubleValue, Is.EqualTo(3.14159).Within(0.00001));
            Assert.That(result[0].BoolValue, Is.True);
            Assert.That(result[0].DateTimeValue, Is.EqualTo(testDate));
            Assert.That(result[0].GuidValue, Is.EqualTo(testGuid));
            Assert.That(result[0].StringValue, Is.EqualTo("Test String"));
        }

        [Test]
        public void CreateInstance_Horizontal_WithVariousDataTypes_ConvertsCorrectly()
        {
            // Arrange
            var testGuid = Guid.NewGuid();

            var table = new DataTable("IntValue", "DecimalValue", "BoolValue", "GuidValue");
            table.AddRow("100", "99.95", "false", testGuid.ToString());

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(100));
            Assert.That(result.DecimalValue, Is.EqualTo(99.95m));
            Assert.That(result.BoolValue, Is.False);
            Assert.That(result.GuidValue, Is.EqualTo(testGuid));
        }

        [Test]
        public void CreateInstance_Vertical_WithVariousDataTypes_ConvertsCorrectly()
        {
            // Arrange
            var table = new DataTable("Property", "Value");
            table.AddRow("IntValue", "999");
            table.AddRow("DecimalValue", "123.45");
            table.AddRow("BoolValue", "true");
            table.AddRow("StringValue", "Vertical Test");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(999));
            Assert.That(result.DecimalValue, Is.EqualTo(123.45m));
            Assert.That(result.BoolValue, Is.True);
            Assert.That(result.StringValue, Is.EqualTo("Vertical Test"));
        }

        #endregion

        #region Computed Properties Tests

        [Test]
        public void CreateSet_WithComputedProperties_OnlySetsInitProperties()
        {
            // Arrange
            var table = new DataTable("FirstName", "LastName", "Age", "IsAdult");
            table.AddRow("John", "Doe", "25", "true");
            table.AddRow("Jane", "Smith", "17", "false");

            // Act
            var result = table.CreateSetWithReadOnlySupport<ComputedPropertyModel>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));

            Assert.That(result[0].FirstName, Is.EqualTo("John"));
            Assert.That(result[0].LastName, Is.EqualTo("Doe"));
            Assert.That(result[0].Age, Is.EqualTo(25));
            Assert.That(result[0].FullName, Is.EqualTo("John Doe"));
            Assert.That(result[0].IsAdult, Is.True);
            Assert.That(result[0].IsMinor, Is.False);

            Assert.That(result[1].FirstName, Is.EqualTo("Jane"));
            Assert.That(result[1].LastName, Is.EqualTo("Smith"));
            Assert.That(result[1].Age, Is.EqualTo(17));
            Assert.That(result[1].FullName, Is.EqualTo("Jane Smith"));
            Assert.That(result[1].IsAdult, Is.False);
            Assert.That(result[1].IsMinor, Is.True);
        }

        #endregion

        #region Property Accessibility Tests

        [Test]
        public void CreateSet_WithMixedAccessibility_SetsAllAccessibleProperties()
        {
            // Arrange
            var table = new DataTable("PublicProperty", "InternalProperty", "ProtectedProperty", "PrivateProperty");
            table.AddRow("Public", "Internal", "Protected", "Private");

            // Act
            var result = table.CreateSetWithReadOnlySupport<MixedAccessibilityModel>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].PublicProperty, Is.EqualTo("Public"));
            Assert.That(result[0].InternalProperty, Is.EqualTo("Internal"));
            // Protected and Private properties are set via reflection
            Assert.That(result[0].GetPrivateProperty(), Is.EqualTo("Private"));
        }

        #endregion

        #region Vertical Table Detection Tests

        [Test]
        public void CreateInstance_WithPropertyColumn_DetectsVerticalFormat()
        {
            // Arrange
            var table = new DataTable("Property", "Value");
            table.AddRow("IntValue", "42");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(42));
        }

        [Test]
        public void CreateInstance_WithPropertyColumnLowercase_DetectsVerticalFormat()
        {
            // Arrange
            var table = new DataTable("property", "value");
            table.AddRow("IntValue", "42");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(42));
        }

        [Test]
        public void CreateInstance_WithPropertyColumnUppercase_DetectsVerticalFormat()
        {
            // Arrange
            var table = new DataTable("PROPERTY", "VALUE");
            table.AddRow("IntValue", "42");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(42));
        }

        [Test]
        public void CreateInstance_WithPropertyColumnMixedCase_DetectsVerticalFormat()
        {
            // Arrange
            var table = new DataTable("ProPeRtY", "VaLuE");
            table.AddRow("IntValue", "42");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(42));
        }

        [Test]
        public void CreateInstance_WithoutPropertyColumn_DetectsHorizontalFormat()
        {
            // Arrange
            var table = new DataTable("IntValue", "StringValue");
            table.AddRow("42", "Test");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result.IntValue, Is.EqualTo(42));
            Assert.That(result.StringValue, Is.EqualTo("Test"));
        }

        #endregion

        #region Empty and Null Value Tests

        [Test]
        public void CreateSet_WithEmptyStringValues_SetsDefaultValues()
        {
            // Arrange
            var table = new DataTable("IntValue", "StringValue");
            table.AddRow("0", "");

            // Act
            var result = table.CreateSetWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IntValue, Is.EqualTo(0));
            Assert.That(result[0].StringValue, Is.EqualTo(string.Empty));
        }

        #endregion

        #region Multiple Rows Tests

        [Test]
        public void CreateSet_WithMultipleRows_CreatesMultipleInstances()
        {
            // Arrange
            var table = new DataTable("IntValue", "BoolValue");
            table.AddRow("1", "true");
            table.AddRow("2", "false");
            table.AddRow("3", "true");
            table.AddRow("4", "false");
            table.AddRow("5", "true");

            // Act
            var result = table.CreateSetWithReadOnlySupport<TypeConversionModel>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].IntValue, Is.EqualTo(1));
            Assert.That(result[0].BoolValue, Is.True);
            Assert.That(result[1].IntValue, Is.EqualTo(2));
            Assert.That(result[1].BoolValue, Is.False);
            Assert.That(result[4].IntValue, Is.EqualTo(5));
            Assert.That(result[4].BoolValue, Is.True);
        }

        #endregion
    }
}