using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Anno.Api.Core.Utility
{
    public static class DataUtility
    {
        #region DataTable/DataSets

        public static bool HasRecord(DataSet dataSet)
        {
            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool HasRecord(DataTable dataTable)
        {
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region SQL Formatters

        /// <summary>
        /// Converts a generic typed object into SQL string. Suports SQL Null.
        /// </summary>
        /// <param name="value">Generic object to parse into SQL string.</param>
        /// <returns>SQL string of the parsed object.</returns>
        //public static string ToSqlParam(object value)
        //{
        //    if (value != null)
        //    {
        //        if (value.GetType() == typeof(DateTime))
        //        {
        //            DateTime dateField = (DateTime)value;
        //            return "'" + ToSqlDateTime(dateField) + "'";
        //        }
        //        else if (value.GetType() == typeof(bool))
        //        {
        //            return ToSqlBoolean((bool)value);
        //        }
        //        else
        //        {
        //            return "'" + ToSqlString(value.ToString()) + "'";
        //        }
        //    }
        //    else
        //    {
        //        return "null";
        //    }
        //}
        //public static string ToSqlParamWithoutApos(object value)
        //{
        //    return ToSqlParam(value).Replace("'", "");
        //}

        public static string ToMySqlParam(object value)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(DateTime))
                {
                    DateTime dateField = (DateTime)value;
                    return "'" + ToSqlDateTime(dateField) + "'";
                }
                else if (value.GetType() == typeof(bool))
                {
                    return ToMySqlBoolean((bool)value);
                }
                else if (value.GetType() == typeof(int) ||
                    value.GetType() == typeof(double) ||
                    value.GetType() == typeof(decimal)||
                    value.GetType() == typeof(float))
                {
                    return value.ToString();
                }
                else
                {
                    return "'" + ToSqlString(value.ToString()) + "'";
                }
            }
            else
            {
                return "null";
            }
        }
        public static string ToMySqlParamWithoutApos(object value)
        {
            return ToMySqlParam(value).Replace("'", "");
        }

        //public static string ToOraParam(object value)
        //{
        //    if (value != null)
        //    {
        //        if (value.GetType() == typeof(DateTime))
        //        {
        //            DateTime dateField = (DateTime)value;
        //            return "TO_DATE('" + ToSqlDateTime(dateField) + "','yyyy-mm-dd hh:mi:ss am')";
        //        }
        //        else if (value.GetType() == typeof(bool))
        //        {
        //            return ToSqlBoolean((bool)value);
        //        }
        //        else
        //        {
        //            return "'" + ToSqlString(value.ToString()) + "'";
        //        }
        //    }
        //    else
        //    {
        //        return "null";
        //    }
        //}

        /// <summary>
        /// Replaces ' characters to '' in a string for SQL query.
        /// </summary>
        /// <param name="value">String to replace.</param>
        /// <returns>String with replaced ' characters.</returns>
        private static string ToSqlString(string value)
        {
            if (value == null)
                value = "";
            //value = value.Replace("'", "''");
            value = value.Replace("\'", "''");
            return value.Trim();
        }

        /// <summary>
        /// Formats a DateTime object into yyyy-MM-dd hh:mm:ss tt string.
        /// For SQL insert/update into a DateTime data field.
        /// </summary>
        /// <param name="value">DateTime object to format.</param>
        /// <returns>String of the formatted DateTime object as yyyy-MM-dd hh:mm:ss tt.</returns>
        private static string ToSqlDateTime(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Converts strongly typed boolean value into '1' or '0' for SQL query.
        /// </summary>
        /// <param name="value">Boolean object to format.</param>
        /// <returns>String with either '1' or '0'.</returns>
        private static string ToSqlBoolean(bool value)
        {
            if ((bool)value)
            {
                return "'1'";
            }
            else
            {
                return "'0'";
            }
        }
        private static string ToMySqlBoolean(bool value)
        {
            if ((bool)value)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        #endregion
    }
}