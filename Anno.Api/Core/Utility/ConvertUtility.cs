using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Anno.Api.Core.Utility
{
    public static class ConvertUtility
    {
        public static byte[] HexToBytes(string value)
        {
            if (value == null || value.Length == 0)
                return new byte[0];
            if (value.Length % 2 == 1)
                throw new FormatException();
            byte[] result = new byte[value.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public static bool IsBoolean(object expression)
        {
            if (expression.ToString().ToLower() == "true" ||
                expression.ToString().ToLower() == "false")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsNumeric(object expression)
        {
            if (expression == null)
                return false;

            if (expression is Int16 || expression is Int32 || expression is Int64)
                return true;

            if (expression is string)
            {
                try
                {
                    double.Parse(expression as string);
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        public static decimal? ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return (decimal)value;
            }
        }

        public static double? ToDouble(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return Convert.ToDouble(value);
            }
        }

        public static bool? ToBoolean(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return Convert.ToBoolean(value);
            }
        }

        public static bool ToDefaultBoolean(object value, bool defaultValue)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            else
            {
                return Convert.ToBoolean(value);
            }
        }
        
        public static string ToString(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return value.ToString();
            }
        }
        
        public static int? ToInt32(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(value);
            }
        }
        
        public static long? ToInt64(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(value);
            }
        }
        
        public static DateTime? ToDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return value as DateTime?;
            }
        }
    }
}