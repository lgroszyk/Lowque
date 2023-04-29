using System.Linq;

namespace Lowque.BusinessLogic.Utils
{
    public static class StringExtensions
    {
        public static bool IsCamelCase(this string str)
        {
            var asCamelCase = str.ToCamelCase();
            return str == asCamelCase;
        }

        public static bool IsPascalCase(this string str)
        {
            var asPascalCase = str.ToPascalCase();
            return str == asPascalCase;
        }

        public static string ToCamelCase(this string str)
        {
            var preformatted = Preformat(str);
            return char.ToLowerInvariant(preformatted[0]) + preformatted.Substring(1);
        }

        public static string ToPascalCase(this string str)
        {
            var preformatted = Preformat(str);
            return char.ToUpperInvariant(preformatted[0]) + preformatted.Substring(1);
        }

        private static string Preformat(string str)
        {
            return string.Join("", str.Split(' ').Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }
    }
}
