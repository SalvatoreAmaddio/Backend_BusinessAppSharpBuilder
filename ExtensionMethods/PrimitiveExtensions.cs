namespace Backend.ExtensionMethods
{
    /// <summary>
    /// Extension methods for primitive types.
    /// </summary>
    public static class PrimitiveExtensions
    {
        /// <summary>
        /// Capitalizes the first letter of the given string.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>A new string with the first letter capitalized.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the input string is empty.</exception>
        public static string FirstLetterCapital(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str), "Input string cannot be null.");
            if (str.Length == 0) throw new ArgumentException("Input string cannot be empty.", nameof(str));

            char firstLetter = str[0];
            string restOf = str.Substring(1);
            return firstLetter.ToString().ToUpper() + restOf;
        }

        /// <summary>
        /// Replaces the character at the specified position in the input string with a new character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="position">The zero-based position of the character to replace.</param>
        /// <param name="newChar">The new character to replace the old character with.</param>
        /// <returns>A new string with the character at the specified position replaced by <paramref name="newChar"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is out of range.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
        public static string ReplaceCharAt(this string input, int position, char newChar)
        {
            if (input == null) throw new ArgumentNullException(nameof(input), "Input string cannot be null.");
            if (position < 0 || position >= input.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of range.");

            return input.Substring(0, position) + newChar + input.Substring(position + 1);
        }
    }
}