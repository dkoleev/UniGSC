using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Yogi.UniGSC.Editor.Parsers {
    [ParserType("default")]
    public class SpreadsheetParserDefault : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            JObject dicJson = new JObject();
            for (int i = 1; i < sheetData.Count; i++) {
                var item = new JObject();
                for (int j = 0; j < sheetData[i].Count; j++) {
                    if (string.IsNullOrEmpty(sheetData[0][j].ToString())) {
                        continue;
                    }
                    
                    item.Add(new JProperty(sheetData[0][j].ToString(), SpreadSheetsParserUtils.GetParseValue(sheetData[i][j])));
                }

                dicJson[sheetData[i][0].ToString()] = item;
            }

            return dicJson.ToString();
        }
    }
}