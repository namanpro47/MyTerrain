using System;
using System.Linq;
using System.Reflection;
using Terra.Contracts;

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
            {
                var fieldValue = field.GetValue(value);
                field.SetValue(result, (field.Name == "simulation" || field.Name == "playerRules") && EmptyExtension(fieldValue) ? null : CopyObject(fieldValue));
            }
            return result;
        }

        // Unity's inline serializer can expand an absent optional DTO into zero/empty
        // fields. Only a wholly empty extension is equivalent to absence; partial or
        // nonzero content remains fingerprinted and must pass normal validation.
        internal static bool EmptyExtension(object value)
        {
            if(value == null) return true;
            if(value is string text) return string.IsNullOrEmpty(text);
            if(value is Array array) return array.Length == 0;
            var type=value.GetType();
            if(type.IsValueType) return value.Equals(Activator.CreateInstance(type));
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance).All(f=>
                // JsonUtility expands an absent driver using these declared initializers.
                // Other identity/data fields must still be entirely empty.
                value is TownSimulationDriver && (f.Name=="solarFactor"||f.Name=="windFactor") && (double)f.GetValue(value)==1
                || EmptyExtension(f.GetValue(value)));
        }

        // Stable across runtimes, independent of reflection ordering and JSON formatting.
        public static string Hash(object value)
        {
            var text = new System.Text.StringBuilder();
            Append(text, value);
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return string.Concat(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text.ToString())).Select(b => b.ToString("x2")));
        }
        static void Append(System.Text.StringBuilder text, object value, bool legacyActions = false)
        {
            if (value == null) { text.Append("null;"); return; }
            var type = value.GetType();
            if (value is string s) { text.Append(s.Length).Append(':').Append(s).Append(';'); return; }
            if (value is double d) { text.Append(RoundTrip(d)).Append(';'); return; }
            if (value is float f) { text.Append(RoundTrip(f)).Append(';'); return; }
            if (type.IsValueType) { text.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)).Append(';'); return; }
            if (value is Array a) { text.Append('[').Append(a.Length).Append(':'); foreach (var item in a) Append(text, item, legacyActions); text.Append(']'); return; }
            if (value is AnnualTownDefinition rules) legacyActions = rules.version == "1.0.0";
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                var fieldValue = field.GetValue(value);
                if (value is AnnualTownSnapshot && field.Name == "planningYears" && (int)fieldValue == 0) continue;
                if (value is TownAsset && ((field.Name == "previousActionId" && string.IsNullOrEmpty((string)fieldValue))
                    || (field.Name == "conversionDelayYears" && (int)fieldValue == 0)
                    || (field.Name == "conversionOpeningVegetation" && (double)fieldValue == 0))) continue;
                // Numeric-v4 additions are absent on all previously published definitions,
                // requests/reports and snapshots. Preserve their exact legacy fingerprints.
                if ((field.Name == "simulation" || field.Name == "playerRules") && EmptyExtension(fieldValue)) continue;
                if (value is TownYearDriver && field.Name == "regionalEvents" && (fieldValue == null || ((Array)fieldValue).Length == 0)) continue;
                if (value is TownAsset && ((field.Name == "cancelledYear" && (int)fieldValue == 0) || (field.Name == "clearedVegetatedFraction" && (double)fieldValue == 0))) continue;
                if (value is TownYearLedger && field.Name == "capitalRefundCents" && (long)fieldValue == 0) continue;
                // Optional schema-2 DTOs do not change any legacy definition/report fingerprints.
                if (fieldValue == null && (field.Name == "economy" || field.Name == "services")) continue;
                // Optional ending targets were added after schema-2 3.0 saves shipped.
                if(value is TownEconomyDefinition && (field.Name=="finalEconomicTarget"||field.Name=="finalSocialTarget"||field.Name=="finalEnvironmentalTarget") && (double)fieldValue==0)continue;
                // v1 actions always coupled services to production. Preserve their saved hashes
                // only for that default; altered behavior must still change the rules hash.
                if (legacyActions && value is TownActionDefinition && field.Name == "servicesFollowProduction" && (bool)fieldValue) continue;
                // An absent and an empty redo stack have identical meaning. Older chapter
                // cache fingerprints predate this field; nonempty redo is still fingerprinted.
                if (value is AnnualTownSnapshot && field.Name == "redo" && (fieldValue == null || ((Array)fieldValue).Length == 0)) continue;
                Append(text, field.Name);
                Append(text, fieldValue ?? (field.FieldType == typeof(string) ? "" : null), legacyActions);
            }
        }

        // Preserve Unity/Mono's existing v1 hashes on the .NET server. Modern .NET's
        // "R" uses shortest formatting; Mono first tries 15/7 significant digits.
        // Changing Unity's format would invalidate existing accepted saves.
        static string RoundTrip(double value)
        {
#if NET8_0_OR_GREATER
            if (value == 0) return "0";
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var shortText = LegacyGeneral(value,15);
            return double.Parse(shortText, culture) == value ? shortText : LegacyGeneral(value,17);
#else
            return value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
#endif
        }
        static string RoundTrip(float value)
        {
#if NET8_0_OR_GREATER
            if (value == 0) return "0";
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var shortText = LegacyGeneral(value,7);
            return float.Parse(shortText, culture) == value ? shortText : LegacyGeneral(value,9);
#else
            return value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
#endif
        }
#if NET8_0_OR_GREATER
        // Mono rounds decimal midpoint ties away from zero; modern .NET rounds to even.
        // Float-derived schema-2 quantities frequently hit those exact binary midpoints.
        static string LegacyGeneral(double value,int digits)
        {
            var culture=System.Globalization.CultureInfo.InvariantCulture;
            string formatted=value.ToString("G"+digits,culture);
            if(value==0||double.IsNaN(value)||double.IsInfinity(value))return formatted;
            double magnitude=Math.Abs(value);
            int decimals=digits-1-(int)Math.Floor(Math.Log10(magnitude));
            long bits=BitConverter.DoubleToInt64Bits(magnitude);
            int exponent=(int)((bits>>52)&2047);
            var numerator=new System.Numerics.BigInteger((bits&0xfffffffffffffL)|(exponent==0?0:0x10000000000000L));
            var denominator=System.Numerics.BigInteger.One;
            int binaryPower=(exponent==0?-1074:exponent-1075);
            if(binaryPower>=0)numerator<<=binaryPower;else denominator<<=-binaryPower;
            if(decimals>=0)numerator*=System.Numerics.BigInteger.Pow(10,decimals);
            else denominator*=System.Numerics.BigInteger.Pow(10,-decimals);
            var integral=System.Numerics.BigInteger.DivRem(numerator,denominator,out var remainder);
            if(remainder*2!=denominator||!integral.IsEven)return formatted;
            string coefficient=(integral+1).ToString(culture);
            int decimalExponent=coefficient.Length-1-decimals;
            string result;
            if(decimalExponent < -4 || decimalExponent>=digits)
            {
                string tail=coefficient.Substring(1).TrimEnd('0');
                result=coefficient[0]+(tail.Length==0?"":"."+tail)+"E"+(decimalExponent<0?"-":"+")+Math.Abs(decimalExponent).ToString("D2",culture);
            }
            else if(decimals<=0)result=coefficient+new string('0',-decimals);
            else
            {
                coefficient=coefficient.PadLeft(decimals+1,'0');
                result=coefficient.Insert(coefficient.Length-decimals,".").TrimEnd('0').TrimEnd('.');
            }
            return value<0?"-"+result:result;
        }
#endif
    }
}
