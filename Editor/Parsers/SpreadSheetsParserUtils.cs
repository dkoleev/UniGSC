using System.Globalization;
namespace Yogi.UniGSC.Editor.Parsers {
    public static class SpreadSheetsParserUtils {
        public static object GetParseValue(object value) {
            if (int.TryParse(value.ToString(), out var resInt)) {
                return resInt;
            }

            var resValue = value.ToString().Replace(',', '.');
            if (float.TryParse(resValue, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,
                out float floatRes)) {
                return floatRes;
            }
            
            return resValue;
        }
    }
}