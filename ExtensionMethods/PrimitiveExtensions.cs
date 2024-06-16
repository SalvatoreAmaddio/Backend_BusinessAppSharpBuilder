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
    }
}
