using Reqnroll.Assist;
using System.Globalization;
using System.Reflection;

namespace Reqnroll.Helpers
{
    public static class DataTableExtensions
    {
        private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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
                    // row[0] is the Property Name, row[1] is the Value
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
        /// Centralized logic to set a property, handling ReadOnly backing fields automatically.
        /// </summary>
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

        private static object GetValueFromTracker(string propertyName, string valueString, Type propertyType)
        {
            var keyValuePair = new KeyValuePair<string, string>(propertyName, valueString);

            // Iterate through all registered retrievers (Standard + Custom)
            foreach (var retriever in Service.Instance.ValueRetrievers)
            {
                if (retriever.CanRetrieve(keyValuePair, propertyType, propertyType))
                {
                    return retriever.Retrieve(keyValuePair, propertyType, propertyType);
                }
            }

            // Fallback: If no retriever claims it, use default Convert.ChangeType
            return Convert.ChangeType(value: valueString, propertyType, CultureInfo.InvariantCulture);
        }
    }
}
