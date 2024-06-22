namespace Backend.ExtensionMethods
{
    public static class PrimitiveExtensions
    {
        public static string FirstLetterCapital(this string str) 
        {
            char FirstLetter = str[0];
            string restOf = str.Substring(1);
            return FirstLetter.ToString().ToUpper() + restOf;
        }

        public static string ReplaceCharAt(this string input, int position, char newChar)
        {
            if (position < 0 || position >= input.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of range.");
            }

            return input.Substring(0, position) + newChar + input.Substring(position + 1);
        }
    }
}
