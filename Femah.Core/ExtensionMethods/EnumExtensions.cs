using System;

namespace Femah.Core.ExtensionMethods
{
    static public class EnumExtensions
    {
        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible
        {
            string valueLower = value.ToLower();
            var retValue = !string.IsNullOrEmpty(valueLower) && Enum.IsDefined(typeof(TEnum), valueLower);
            result = retValue ? (TEnum)Enum.Parse(typeof(TEnum), valueLower) : default(TEnum);
            return retValue;
        }
    }
}
