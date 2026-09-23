using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Yogi.UniGSC.Editor.Parsers
{
    [ParserType("const")]
    public class SpreadsheetParserConsts : ISpreadsheetParser
    {
        public string Parse(int sheetId, IList<IList<object>> sheetData)
        {
            var result = new JObject();

            for (var i = 0; i < sheetData.Count; i++)
            {
                var rawKey = sheetData[i][0].ToString();
                var cell = sheetData[i][1]?.ToString();
                if (string.IsNullOrEmpty(cell))
                    continue;

                var (key, forceArray) = SpreadSheetsParserUtils.ParseHeader(rawKey);
                var values = SpreadSheetsParserUtils.ParseCell(cell);
                var token = SpreadSheetsParserUtils.ToJsonToken(values, forceArray);
                if (token != null)
                    SetNestedValue(result, key, token);
            }

            return result.ToString();
        }

        private static void SetNestedValue(JObject root, string key, JToken value)
        {
            var parts = key.Split('.');

            var current = root;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (current[parts[i]] is not JObject child)
                {
                    child = new JObject();
                    current[parts[i]] = child;
                }

                current = child;
            }

            current[parts[^1]] = value;
        }
    }
}
