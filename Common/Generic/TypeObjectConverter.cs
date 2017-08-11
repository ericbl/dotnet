using Common.Strings;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Common.Generic
{
    /// <summary>
    /// Utilities to convert type, i.e. from object or string to any value type.
    /// </summary>
    public static class TypeObjectConverter
    {
        #region convert type to given type -- 
        /// <summary>
        /// Converts the sourceDate to a TDest object.
        /// </summary>
        /// <typeparam name="TDest">The type of the dest.</typeparam>
        /// <param name="sourceData">The source data.</param>
        /// <param name="errorMsg"><c>null</c> if the conversion succeeded, the error message otherwise.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// The converted object or the default value
        /// </returns>
        public static TDest ConvertType<TDest>(object sourceData, out string errorMsg, IFormatProvider formatProvider)
        {
            object converted = ConvertType(sourceData, typeof(TDest), out errorMsg, formatProvider);
            return (converted != null) ? (TDest)converted : default(TDest);
        }

        /// <summary>
        /// Converts the sourceDate to a destType object. To uses typically with reflection. Not generic to be able to work easier with SetValue(null)        
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="originalType">Type of the destination.</param>
        /// <param name="errorMsg"><c>null</c> if the conversion succeeded, the error message otherwise.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>The converted object or null</returns>
        /// <remarks>Eric2016: This method seems over complicated, see the simple ConvertTypeChangeTypeNullable which would be sufficient for most cases!</remarks>
        public static object ConvertType(object sourceData, Type originalType, out string errorMsg, IFormatProvider formatProvider)
        {
            Type destType = originalType;
            var typedValue = sourceData;
            errorMsg = null;
            try
            {
                Type srcType = sourceData.GetType();
                if (srcType == typeof(DBNull))
                {
                    typedValue = null;
                }
                else
                {
                    if (srcType != destType)
                    {
                        if (srcType == typeof(DateTime) && destType == typeof(TimeSpan))
                        {
                            DateTime srcTime = (DateTime)sourceData;
                            typedValue = new TimeSpan(srcTime.Hour, srcTime.Minute, srcTime.Second);
                        }
                        else
                        {
                            if (Helper.IsNullableType(destType))
                            {
                                if (sourceData == null)
                                {
                                    return null;
                                }
                                // Define the type behind the nullable one
                                destType = Helper.GetNullableUnderlyingType(destType);
                            }

                            if (destType == typeof(Boolean) && srcType == typeof(string))
                            {
                                typedValue = Strings.Helper.ParseBoolean((string)sourceData);
                            }
                            else
                            {
                                if (sourceData.GetType() == typeof(String))
                                {
                                    string sourceString = sourceData.ToString().Trim();
                                    if (destType == typeof(TimeSpan))
                                    {
                                        var dateTime = DateTime.Parse(sourceString);
                                        typedValue = new TimeSpan(dateTime.Hour, dateTime.Minute, dateTime.Second);
                                    }
                                    else
                                    {
                                        if (destType == typeof(DateTime))
                                        {
                                            typedValue = DateTime.Parse(sourceString);
                                        }
                                        else
                                        {
                                            if (destType.IsPrimitive)
                                            {
                                                if (string.IsNullOrEmpty(sourceString))
                                                {
                                                    typedValue = Helper.IsNullableType(originalType) ? null : Activator.CreateInstance(destType);
                                                }
                                                else
                                                {
                                                    typedValue = ((IConvertible)sourceString).ToType(destType, formatProvider);
                                                }
                                            }
                                            else
                                            {
                                                //typedValue = Convert.ChangeType(sourceData, destType);
                                                typedValue = TypeDescriptor.GetConverter(destType).ConvertFromString(sourceString);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    typedValue = Convert.ChangeType(sourceData, destType);
                                }
                            }
                        }
                    }
                }

                if (destType == typeof(string) && (string)typedValue == string.Empty)
                {
                    typedValue = null;
                }
            }
            catch (Exception)
            {
                errorMsg = string.Format("Die Eingangsdaten '{0}' konnten nicht in {1} umgewandeln werden!", sourceData.ToString(), destType.ToString());
                return null;
            }

            return typedValue;
        }

        /// <summary>
        /// Converts the propertyValue via a simple call of ChangeType with destination type is typeof(T) or the underlying type if T is nullable.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="propertyValue">The property string.</param>
        /// <param name="formatProvider">The format provider. CurrentCulture per default when let null</param>
        /// <returns>
        /// The converted property to T or the default value of T
        /// </returns>
        public static T ConvertTypeChangeTypeNullable<T>(string propertyValue, IFormatProvider formatProvider = null)
        {
            object result = ConvertTypeChangeTypeNullable(typeof(T), propertyValue, formatProvider);
            if (result == null)
                return default(T);

            return (T)result;
        }

        /// <summary>
        /// Converts the propertyValue via a simple call of ChangeType with destination type is typeof(T) or the underlying type if T is nullable.
        /// </summary>
        /// <param name="destinationType">Type of the property.</param>
        /// <param name="propertyValue">The property string.</param>
        /// <param name="formatProvider">The format provider. CurrentCulture per default when let null</param>
        /// <returns>
        /// The converted property or null.
        /// </returns>
        public static object ConvertTypeChangeTypeNullable(Type destinationType, string propertyValue, IFormatProvider formatProvider = null)
        {
            if (formatProvider == null)
            {
                formatProvider = System.Threading.Thread.CurrentThread.CurrentCulture; // default value of ChangeType when formatProvider is not set!
            }
            if (!string.IsNullOrEmpty(propertyValue))
            {
                Type underlyingType = Nullable.GetUnderlyingType(destinationType);
                Type targetType = underlyingType != null ? underlyingType : destinationType;
                return Convert.ChangeType(propertyValue, targetType, formatProvider);
            }

            return null;
        }

        #endregion

        #region String Converter: from string to given type -- similar job as above but from another work

        /// <summary>
        /// Convert object record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>
        /// The object converted record.
        /// </returns>
        public static object ConvertObjectRecord(string record, Type targetType, string cultureInfo)
        {
            object convertedRecord;
            if (targetType == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(record)) // skip empty record!
                    return null;
                else
                    convertedRecord = record;
            }
            if (targetType == typeof(DateTime?))
            {
                convertedRecord = record.ToDateTimeNullable(cultureInfo);
            }
            else if (targetType == typeof(DateTime))
            {
                convertedRecord = record.ToDateTime(cultureInfo);
            }
            else if (targetType == typeof(bool?))
            {
                convertedRecord = record.ToBooleanNullable();
            }
            else if (targetType == typeof(bool))
            {
                convertedRecord = record.ToBoolean();
            }
            else if (targetType == typeof(int?))
            {
                convertedRecord = record.ToInt32Nullable();
            }
            else if (targetType == typeof(double?))
            {
                convertedRecord = record.ToDoubleNullable();
            }
            else if (targetType.IsEnum)
            {
                convertedRecord = Enum.Parse(targetType, record);
            }
            else
            {
                convertedRecord = Convert.ChangeType(record, targetType); // convert also string, applying the current culture info!
            }

            return convertedRecord;
        }

        /// <summary>
        /// A string extension method that converts a source to an int 32.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as an int.
        /// </returns>
        public static int ToInt32(this string source)
        {
            int outNum;
            return !string.IsNullOrEmpty(source) && int.TryParse(source, out outNum) ? outNum : 0;
        }

        /// <summary>
        /// A string extension method that converts a source to an int 32 nullable.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as an int?
        /// </returns>
        public static int? ToInt32Nullable(this string source)
        {
            int outNum;
            return !string.IsNullOrEmpty(source) && int.TryParse(source, out outNum) ? outNum : (int?)null;
        }

        /// <summary>
        /// A string extension method that converts a source to an int 64. Remove fist the white space!
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as an long.
        /// </returns>
        public static long ToInt64(this string source)
        {
            source = source.RemoveWhitespace();
            long outNum;
            return !string.IsNullOrEmpty(source) && long.TryParse(source, out outNum) ? outNum : 0;
        }

        /// <summary>
        /// A string extension method that converts a source to an int 64 nullable. Remove fist the white space!
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as an long?
        /// </returns>
        public static long? ToInt64Nullable(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            source = source.RemoveWhitespace();
            long outNum;
            return !string.IsNullOrEmpty(source) && long.TryParse(source, out outNum) ? outNum : (long?)null;
        }

        /// <summary>
        /// A string extension method that converts a source to a decimal.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a decimal.
        /// </returns>
        public static decimal ToDecimal(this string source)
        {
            decimal outNum;
            return !string.IsNullOrEmpty(source) && decimal.TryParse(source, out outNum) ? outNum : 0;
        }

        /// <summary>
        /// A string extension method that converts a source to a decimal nullable.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a decimal?
        /// </returns>
        public static decimal? ToDecimalNullable(this string source)
        {
            decimal outNum;
            return !string.IsNullOrEmpty(source) && decimal.TryParse(source, out outNum) ? outNum : (decimal?)null;
        }

        /// <summary>
        /// A string extension method that converts a source to a double.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a double.
        /// </returns>
        public static double ToDouble(this string source)
        {
            double outNum;
            return !string.IsNullOrEmpty(source) && double.TryParse(source, out outNum) ? outNum : 0;
        }

        /// <summary>
        /// A string extension method that converts a source to a double nullable.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a double?
        /// </returns>
        public static double? ToDoubleNullable(this string source)
        {
            double outNum;
            return !string.IsNullOrEmpty(source) && double.TryParse(source, out outNum) ? outNum : (double?)null;
        }

        /// <summary>
        /// A string extension method that converts a source to an enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The converted enum</returns>
        public static TEnum ToEnum<TEnum>(this string source)
            where TEnum : struct
        {
            TEnum outEnum;
            Enum.TryParse(source, out outEnum);
            return outEnum;
        }

        /// <summary>
        /// A string extension method that converts a source to a date time.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>
        /// Source as a DateTime.
        /// </returns>
        public static DateTime ToDateTime(this string source, string cultureInfo)
        {
            return ToDateTime(source, false, cultureInfo).Value;
        }

        /// <summary>
        /// A string extension method that converts a source to a date time nullable.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>
        /// Source as a DateTime?
        /// </returns>
        public static DateTime? ToDateTimeNullable(this string source, string cultureInfo)
        {
            if (string.IsNullOrEmpty(source))
                return null;
            else
                return ToDateTime(source, true, cultureInfo);
        }

        /// <summary>
        /// A string extension method that converts a source to a date time.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <param name="isNullable">True if this object is nullable.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>
        /// Source as a DateTime.
        /// </returns>
        private static DateTime? ToDateTime(string source, bool isNullable, string cultureInfo)
        {
            DateTime outDt;
            IFormatProvider provider = string.IsNullOrEmpty(cultureInfo) ? null : Strings.Helper.GetProviderFromCultureInfo(cultureInfo);
            bool parsed = provider == null ? DateTime.TryParse(source, out outDt) : DateTime.TryParse(source, provider, System.Globalization.DateTimeStyles.None, out outDt);
            if (parsed)
            {
                return outDt;
            }
            else
            {
                //Check OLE Automation date time
                if (IsNumeric(source))
                {
                    return DateTime.FromOADate(source.ToDouble());
                }
                if (isNullable)
                    return null;
                else return DateTime.Now;
            }
        }

        /// <summary>
        /// A string extension method that converts a source to a boolean.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a bool.
        /// </returns>
        public static bool ToBoolean(this string source)
        {
            if (!string.IsNullOrEmpty(source))
                if (source.ToLower() == "true" || source == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A string extension method that converts a source to a boolean nullable.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a bool?
        /// </returns>
        public static bool? ToBooleanNullable(this string source)
        {
            if (!string.IsNullOrEmpty(source))
                if (source.ToLower() == "true" || source == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// A string extension method that converts a source to a unique identifier.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a GUID.
        /// </returns>
        public static Guid ToGuid(this string source)
        {
            Guid outGuid;
            return Guid.TryParse(source, out outGuid) ? outGuid : Guid.Empty;
        }

        /// <summary>
        /// A string extension method that converts a source to a unique identifier nullable.
        /// </summary>
        /// <param name="source">The source to act on.</param>
        /// <returns>
        /// Source as a Guid?
        /// </returns>
        public static Guid? ToGuidNullable(this string source)
        {
            Guid outGuid;
            return Guid.TryParse(source, out outGuid) ? outGuid : (Guid?)null;
        }
        #endregion

        #region Utils
        private static bool IsNumeric(string value)
        {
            return _isNumericRegex.IsMatch(value);
        }

        private static readonly Regex _isNumericRegex = new Regex("^(" +
            /*Hex*/ @"0x[0-9a-f]+" + "|" +
            /*Bin*/ @"0b[01]+" + "|" +
            /*Oct*/ @"0[0-7]*" + "|" +
            /*Dec*/ @"((?!0)|[-+]|(?=0+\.))(\d*\.)?\d+(e\d+)?" +
            ")$");

        #endregion
    }
}
