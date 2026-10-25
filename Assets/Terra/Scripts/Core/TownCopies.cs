using System;
using System.Linq;
using System.Reflection;

namespace Terra.Core
{
    // DTOs contain only primitive/string/enum values, arrays and public-field DTOs.
    // Copy at the boundary so callers/providers cannot mutate authoritative state.
    internal static class TownCopies
    {
        public static T Copy<T>(T value) => (T)CopyObject(value);
        static object CopyObject(object value)
        {
            if (value == null) return null;
            var type = value.GetType();
            if (type.IsValueType || type == typeof(string)) return value;
            if (value is Array array)
            {
                var copy = Array.CreateInstance(type.GetElementType(), array.Length);
                for (int i = 0; i < array.Length; i++) copy.SetValue(CopyObject(array.GetValue(i)), i);
                return copy;
            }
            var result = Activator.CreateInstance(type);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                field.SetValue(result, CopyObject(field.GetValue(value)));
            return result;
        }

        // Stable across runtimes, independent of reflection ordering and JSON formatting.
        public static string Hash(object value)
        {
            var text = new System.Text.StringBuilder();
            Append(text, value);
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return string.Concat(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text.ToString())).Select(b => b.ToString("x2")));
        }
        static void Append(System.Text.StringBuilder text, object value)
        {
            if (value == null) { text.Append("null;"); return; }
            var type = value.GetType();
            if (value is string s) { text.Append(s.Length).Append(':').Append(s).Append(';'); return; }
            if (value is double d) { text.Append(d.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(';'); return; }
            if (value is float f) { text.Append(f.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append(';'); return; }
            if (type.IsValueType) { text.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)).Append(';'); return; }
            if (value is Array a) { text.Append('[').Append(a.Length).Append(':'); foreach (var item in a) Append(text, item); text.Append(']'); return; }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f.Name, StringComparer.Ordinal))
            { Append(text, field.Name); Append(text, field.GetValue(value) ?? (field.FieldType == typeof(string) ? "" : null)); }
        }
    }
}
