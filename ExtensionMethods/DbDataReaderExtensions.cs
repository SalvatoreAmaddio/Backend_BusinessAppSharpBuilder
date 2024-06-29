using Backend.Utils;
using System.Data.Common;

namespace Backend.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="DbDataReader"/> to handle nullable and exception-prone data fetch operations.
    /// </summary>
    public static class DbDataReaderExtensions
    {
        /// <summary>
        /// Tries to fetch a <see cref="string"/> from a <see cref="DbDataReader"/>. 
        /// Handles exceptions in case the field is null or an error occurs.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> instance.</param>
        /// <param name="index">The ordinal position of the column.</param>
        /// <param name="onEmpty">The string to return if the fetched string is empty.</param>
        /// <param name="onNull">The string to return if an exception occurs.</param>
        /// <returns>A <see cref="string"/> representing the fetched value or a default value in case of an exception.</returns>
        public static string TryFetchString(this DbDataReader reader, int index, string onEmpty = "", string onNull = "")
        {
            try
            {
                string str = reader.GetString(index);
                if (string.IsNullOrEmpty(str))
                    return onEmpty;
                return str;
            }
            catch
            {
                return onNull;
            }
        }

        /// <summary>
        /// Tries to fetch a <see cref="DateTime"/> object from a <see cref="DbDataReader"/>. 
        /// Handles exceptions in case the field is null or an error occurs.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> instance.</param>
        /// <param name="index">The ordinal position of the column.</param>
        /// <returns>A nullable <see cref="DateTime"/> object representing the fetched value or null in case of an exception.</returns>
        public static DateTime? TryFetchDate(this DbDataReader reader, int index)
        {
            try
            {
                return reader.GetDateTime(index);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to fetch an <see cref="int"/> from a <see cref="DbDataReader"/>. 
        /// Handles exceptions in case the field is null or an error occurs.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> instance.</param>
        /// <param name="index">The ordinal position of the column.</param>
        /// <returns>An <see cref="int"/> representing the fetched value or 0 in case of an exception.</returns>
        public static int TryFetchInt32(this DbDataReader reader, int index)
        {
            try
            {
                return reader.GetInt32(index);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Tries to fetch a <see cref="double"/> from a <see cref="DbDataReader"/>. 
        /// Handles exceptions in case the field is null or an error occurs.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> instance.</param>
        /// <param name="index">The ordinal position of the column.</param>
        /// <returns>A <see cref="double"/> representing the fetched value or 0 in case of an exception.</returns>
        public static double TryFetchDouble(this DbDataReader reader, int index)
        {
            try
            {
                return reader.GetDouble(index);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Tries to fetch a <see cref="TimeSpan"/> from a <see cref="DateTime"/> in a <see cref="DbDataReader"/>. 
        /// Handles exceptions in case the field is null or an error occurs.
        /// </summary>
        /// <param name="reader">The <see cref="DbDataReader"/> instance.</param>
        /// <param name="index">The ordinal position of the column.</param>
        /// <returns>A nullable <see cref="TimeSpan"/> representing the fetched value or null in case of an exception.</returns>
        public static TimeSpan? TryFetchTime(this DbDataReader reader, int index)
        {
            try
            {
                DateTime? date = reader.GetDateTime(index);
                return Sys.GetTime(date);
            }
            catch
            {
                return null;
            }
        }
    }

}
