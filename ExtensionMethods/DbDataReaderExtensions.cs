using Backend.Utils;
using System.Data.Common;

namespace Backend.ExtensionMethods
{
    public static class DbDataReaderExtensions
    {

        /// <summary>
        /// Try to fetch a <see cref="string"/>. <para/>
        /// This extension method hanldes Exceptions in case the field is null in the Database.
        /// </summary>
        /// <param name="index">the ordinal position of the column</param>
        /// <param name="onEmpty">the string to return if the <see cref="DbDataReader.GetString(int)"/> returns an empty string</param>
        /// <param name="onNull">the string to return if the <see cref="DbDataReader.GetString(int)"/> fails</param>
        /// <returns>A <see cref="string"/></returns>
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
        /// Try to fetch a <see cref="DateTime"/> object from a <see cref="DbDataReader"/>. <para/>
        /// This extension method hanldes Exceptions in case the field is null in the Database.
        /// </summary>
        /// <param name="index">the ordinal position of the column</param>
        /// <returns>A <see cref="DateTime"/> object</returns>
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
        /// Try to fetch the <see cref="TimeSpan"/> from a <see cref="DateTime"/>. <para/>
        /// This extension method hanldes Exceptions in case the field is null in the Database.
        /// </summary>
        /// <param name="index">the ordinal position of the column</param>
        /// <returns>A <see cref="TimeSpan"/></returns>
        public static TimeSpan? TryFetchTime(this DbDataReader reader, int index)
        {
            DateTime? date = null;
            try
            {
                date = reader.GetDateTime(index);
                return Sys.GetTime(date);
            }
            catch
            {
                return null;
            }
        }
    }
}
