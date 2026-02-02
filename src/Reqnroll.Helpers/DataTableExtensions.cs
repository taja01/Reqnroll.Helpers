using Reqnroll.Assist;
using System.Globalization;
using System.Reflection;

namespace Reqnroll.Helpers
{
    public static class DataTableExtensions
    {
        private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Creates a list of objects from a Reqnroll DataTable. 
        /// Supports mapping to properties with private setters or read-only backing fields.
        /// </summary>
        /// <typeparam name="T">The type of object to create. Must have a parameterless constructor.</typeparam>
        /// <param name="table">The Reqnroll DataTable containing the data rows.</param>
        /// <returns>A list of instances of type <typeparamref name="T"/> populated with data from the table.</returns>
        public static List<T> CreateSetWithReadOnlySupport<T>(this DataTable table) where T : new()
        {
            var items = new List<T>();

            foreach (var row in table.Rows)
            {
                var instance = new T();
                foreach (var header in table.Header)
                {
                    SetProperty(instance, header, row[header]);
                }

                items.Add(instance);
            }

            return items;
        }

        /// <summary>
        /// Creates a single instance of an object from a Reqnroll DataTable.
        /// Automatically detects if the table is horizontal (headers as properties) or vertical (Property/Value columns).
        /// </summary>
        /// <typeparam name="T">The type of object to create. Must have a parameterless constructor.</typeparam>
        /// <param name="table">The Reqnroll DataTable containing the data.</param>
        /// <returns>A single instance of type <typeparamref name="T"/>.</returns>
        public static T CreateInstanceWithReadOnlySupport<T>(this DataTable table) where T : new()
        {
            var instance = new T();

            // SCENARIO A: Vertical Table (Key/Value pairs)
            // | Property | Value |
            // | Name     | John  |
            // | Age      | 44    |
            if (table.Header.Any(h => h.Equals("property", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var row in table.Rows)
                {
                    SetProperty(instance, row[0], row[1]);
                }
            }
            // SCENARIO B: Horizontal Table (Single row of data)
            // | Name | Age |
            // | John | 30  |
            else
            {
                var row = table.Rows[0];
                foreach (var header in table.Header)
                {
                    SetProperty(instance, header, row[header]);
                }
            }

            return instance;
        }

        /// <summary>
        /// Sets a property value on an instance using reflection. 
        /// If the property is read-only, it attempts to set the value via the C# compiler-generated backing field.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The object instance to modify.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="valueString">The string value from the DataTable to be converted and assigned.</param>
        /// <exception cref="InvalidOperationException">Thrown if the property cannot be set or conversion fails.</exception>
        private static void SetProperty<T>(T instance, string propertyName, string valueString)
        {
            var prop = typeof(T).GetProperty(propertyName, PropertyBindingFlags);
            if (prop == null)
            {
                return;
            }

            try
            {
                var value = GetValueFromTracker(propertyName, valueString, prop.PropertyType);

                if (prop.CanWrite)
                {
                    prop.SetValue(instance, value);
                }
                else
                {
                    var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    backingField?.SetValue(instance, value);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set property '{propertyName}' with value '{valueString}'.", ex);
            }
        }

        /// <summary>
        /// Converts a string value to the target type using Reqnroll's registered Value Retrievers.
        /// Falls back to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> if no retriever is found.
        /// </summary>
        /// <param name="propertyName">Name of the property (used for mapping).</param>
        /// <param name="valueString">The raw string value from the table.</param>
        /// <param name="propertyType">The target Type to convert the string into.</param>
        /// <returns>The converted object value.</returns>
        private static object GetValueFromTracker(string propertyName, string valueString, Type propertyType)
        {
            var keyValuePair = new KeyValuePair<string, string>(propertyName, valueString);

            foreach (var retriever in Service.Instance.ValueRetrievers)
            {
                if (retriever.CanRetrieve(keyValuePair, propertyType, propertyType))
                {
                    return retriever.Retrieve(keyValuePair, propertyType, propertyType);
                }
            }

            return Convert.ChangeType(value: valueString, propertyType, CultureInfo.InvariantCulture);
        }
    }
}
