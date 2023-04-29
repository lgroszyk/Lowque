using System.Linq;

namespace Lowque.BusinessLogic.Types
{
    public static class SystemBasicTypes
    {
        public const string Binary = "Binary";
        public const string IntegralNumber = "IntegralNumber";
        public const string RealNumber = "RealNumber";
        public const string TextPhrase = "TextPhrase";
        public const string DateAndTime = "DateAndTime";
        public const string File = "File";
        public const string List = "List";

        public static bool IsSimpleType(string type)
        {
            var simpleTypes = new[] { Binary, IntegralNumber, RealNumber, TextPhrase, DateAndTime, File };
            var isSimpleType = simpleTypes.Contains(type);
            return isSimpleType;
        }

        public static bool IsList(string type)
        {
            return type == List;
        }

        public static bool IsSortable(string type)
        {
            var sortableTypes = new[] { IntegralNumber, RealNumber, TextPhrase, DateAndTime };
            var isSortableType = sortableTypes.Contains(type);
            return isSortableType;
        }

        public static string AsCSharpType(string systemType)
        {
            switch (systemType)
            {
                case Binary: return "bool";
                case IntegralNumber: return "int";
                case RealNumber: return "double";
                case TextPhrase: return "string";
                case DateAndTime: return "DateTime";
                case File: return "IFormFile";
                default: return systemType;
            }
        }

        public static string AsSystemType(string csharpType)
        {
            switch (csharpType)
            {
                case "bool": return Binary;
                case "int": return IntegralNumber;
                case "double": return RealNumber;
                case "string": return TextPhrase;
                case "DateTime": return DateAndTime;
                case "IFormFile": return File;
                default: return csharpType;
            }
        }
    }
}
