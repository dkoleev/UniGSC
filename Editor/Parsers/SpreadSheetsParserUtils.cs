using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

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
            
            return value.ToString();
        }
        
        public static (string name, bool isArray) ParseHeader(string rawHeader) {
            if (rawHeader.EndsWith("[]"))
                return (rawHeader.Substring(0, rawHeader.Length - 2), true);
            return (rawHeader, false);
        }

        public static List<object> ParseCell(string cell) {
            var values = new List<object>();
            if (cell.Contains(", ")) {
                foreach (var part in cell.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                    values.Add(SpreadSheetsParserUtils.GetParseValue(part.Trim()));
            } else {
                values.Add(SpreadSheetsParserUtils.GetParseValue(cell));
            }
            return values;
        }

        public static JToken ToJsonToken(List<object> values, bool forceArray) {
            if (values.Count == 0)
                return null;
            if (forceArray || values.Count > 1)
                return new JArray(values.ToArray());
            return JToken.FromObject(values[0]);
        }
    }
}