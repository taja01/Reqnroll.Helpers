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

        public class PersonForAssertion
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public int Age { get; set; }
            public string FullName => $"{FirstName} {LastName}".Trim();
            public bool IsAdult => Age >= 18;
            public string Status => IsAdult ? "Adult" : "Minor";
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
            var table = new DataTable("FirstName", "LastName", "Age");
            table.AddRow("John", "Doe", "25");
            table.AddRow("Jane", "Smith", "17");

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

        #region Skip Computed Properties in Table Tests (Reqnroll Assertion Scenario)

        [Test]
        public void CreateSet_WithComputedPropertyInTable_SkipsComputedAndCalculatesCorrectly()
        {
            // Arrange - Table includes "IsAdult" column which is a computed property
            var table = new DataTable("FirstName", "LastName", "Age", "IsAdult");
            table.AddRow("John", "Doe", "25", "true");
            table.AddRow("Jane", "Smith", "17", "false");

            // Act
            var result = table.CreateSetWithReadOnlySupport<PersonForAssertion>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));

            // First person - IsAdult column value "true" should be ignored, calculated from Age
            Assert.That(result[0].FirstName, Is.EqualTo("John"));
            Assert.That(result[0].Age, Is.EqualTo(25));
            Assert.That(result[0].IsAdult, Is.True); // Calculated from Age >= 18, not from table

            // Second person - IsAdult column value "false" should be ignored, calculated from Age
            Assert.That(result[1].FirstName, Is.EqualTo("Jane"));
            Assert.That(result[1].Age, Is.EqualTo(17));
            Assert.That(result[1].IsAdult, Is.False); // Calculated from Age < 18, not from table
        }

        [Test]
        public void CreateSet_WithMultipleComputedPropertiesInTable_SkipsAllComputedProperties()
        {
            // Arrange - Table includes multiple computed properties
            var table = new DataTable("FirstName", "LastName", "Age", "FullName", "IsAdult", "Status");
            table.AddRow("Alice", "Johnson", "30", "Wrong Name", "false", "Wrong Status");
            table.AddRow("Bob", "Williams", "16", "Another Wrong", "true", "Invalid");

            // Act
            var result = table.CreateSetWithReadOnlySupport<PersonForAssertion>();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));

            // First person - All computed values should come from calculation, not table
            Assert.That(result[0].FirstName, Is.EqualTo("Alice"));
            Assert.That(result[0].LastName, Is.EqualTo("Johnson"));
            Assert.That(result[0].Age, Is.EqualTo(30));
            Assert.That(result[0].FullName, Is.EqualTo("Alice Johnson")); // Computed, ignores "Wrong Name"
            Assert.That(result[0].IsAdult, Is.True); // Computed from Age, ignores "false" from table
            Assert.That(result[0].Status, Is.EqualTo("Adult")); // Computed, ignores "Wrong Status"

            // Second person
            Assert.That(result[1].FirstName, Is.EqualTo("Bob"));
            Assert.That(result[1].LastName, Is.EqualTo("Williams"));
            Assert.That(result[1].Age, Is.EqualTo(16));
            Assert.That(result[1].FullName, Is.EqualTo("Bob Williams")); // Computed
            Assert.That(result[1].IsAdult, Is.False); // Computed from Age, ignores "true" from table
            Assert.That(result[1].Status, Is.EqualTo("Minor")); // Computed
        }

        [Test]
        public void CreateInstance_Horizontal_WithComputedPropertyInTable_SkipsComputedProperty()
        {
            // Arrange
            var table = new DataTable("FirstName", "LastName", "Age", "IsAdult", "Status");
            table.AddRow("Charlie", "Brown", "20", "false", "Child");

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<PersonForAssertion>();

            // Assert
            Assert.That(result.FirstName, Is.EqualTo("Charlie"));
            Assert.That(result.LastName, Is.EqualTo("Brown"));
            Assert.That(result.Age, Is.EqualTo(20));
            Assert.That(result.FullName, Is.EqualTo("Charlie Brown")); // Computed
            Assert.That(result.IsAdult, Is.True); // Computed from Age=20, ignores "false" from table
            Assert.That(result.Status, Is.EqualTo("Adult")); // Computed, ignores "Child" from table
        }

        [Test]
        public void CreateInstance_Vertical_WithComputedPropertyInTable_SkipsComputedProperty()
        {
            // Arrange
            var table = new DataTable("Property", "Value");
            table.AddRow("FirstName", "Diana");
            table.AddRow("LastName", "Prince");
            table.AddRow("Age", "15");
            table.AddRow("IsAdult", "true"); // This should be ignored
            table.AddRow("FullName", "Wrong Full Name"); // This should be ignored
            table.AddRow("Status", "Adult"); // This should be ignored

            // Act
            var result = table.CreateInstanceWithReadOnlySupport<PersonForAssertion>();

            // Assert
            Assert.That(result.FirstName, Is.EqualTo("Diana"));
            Assert.That(result.LastName, Is.EqualTo("Prince"));
            Assert.That(result.Age, Is.EqualTo(15));
            Assert.That(result.FullName, Is.EqualTo("Diana Prince")); // Computed, not from table
            Assert.That(result.IsAdult, Is.False); // Computed from Age=15, not "true" from table
            Assert.That(result.Status, Is.EqualTo("Minor")); // Computed, not "Adult" from table
        }

        [Test]
        public void CreateSet_ReqnrollAssertionStyle_WithExpectedComputedValues()
        {
            // This mimics a real Reqnroll scenario where you assert with computed properties
            // Given I have the following persons:
            // | FirstName | LastName | Age | IsAdult |
            // | John      | Doe      | 25  | true    |
            // | Jane      | Smith    | 17  | false   |

            // Arrange
            var table = new DataTable("FirstName", "LastName", "Age", "IsAdult");
            table.AddRow("John", "Doe", "25", "true");
            table.AddRow("Jane", "Smith", "17", "false");

            // Act
            var actual = table.CreateSetWithReadOnlySupport<PersonForAssertion>();

            // Then the results should match
            var expected = new List<PersonForAssertion>
            {
                new() { FirstName = "John", LastName = "Doe", Age = 25 },
                new() { FirstName = "Jane", LastName = "Smith", Age = 17 }
            };

            // Assert - comparing computed properties works because they calculate correctly
            Assert.That(actual, Has.Count.EqualTo(2));

            for (int i = 0; i < actual.Count; i++)
            {
                Assert.That(actual[i].FirstName, Is.EqualTo(expected[i].FirstName));
                Assert.That(actual[i].LastName, Is.EqualTo(expected[i].LastName));
                Assert.That(actual[i].Age, Is.EqualTo(expected[i].Age));
                Assert.That(actual[i].IsAdult, Is.EqualTo(expected[i].IsAdult));
            }
        }

        [Test]
        public void CreateSet_OnlyComputedPropertiesInTable_DoesNotThrowException()
        {
            // Arrange - Table contains ONLY computed properties
            var table = new DataTable("FullName", "IsAdult", "Status");
            table.AddRow("John Doe", "true", "Adult");

            // Act & Assert - Should not throw, just create empty/default object
            Assert.DoesNotThrow(() =>
            {
                var result = table.CreateSetWithReadOnlySupport<PersonForAssertion>();
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0].FirstName, Is.EqualTo(string.Empty));
                Assert.That(result[0].Age, Is.EqualTo(0));
            });
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