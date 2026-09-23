using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Yogi.UniGSC.Editor.Parsers
{
    [ParserType("flat")]
    public class SpreadSheetsParserFlat : ISpreadsheetParser
    {
        public string Parse(int sheetId, IList<IList<object>> sheetData)
        {
            var headers = sheetData[0];
            var result = new JObject();

            for (var i = 1; i < sheetData.Count; i++)
            {
                var row = sheetData[i];
                var item = new JObject();

                for (var j = 0; j < row.Count; j++)
                {
                    if (j >= headers.Count)
                        break;

                    var rawHeader = headers[j].ToString();
                    if (string.IsNullOrEmpty(rawHeader))
                        continue;

                    var cell = row[j]?.ToString();
                    if (string.IsNullOrEmpty(cell))
                        continue;

                    var (header, forceArray) = SpreadSheetsParserUtils.ParseHeader(rawHeader);
                    var values = SpreadSheetsParserUtils.ParseCell(cell);
                    var token = SpreadSheetsParserUtils.ToJsonToken(values, forceArray);
                    if (token != null)
                        item[header] = token;
                }

                result[row[0].ToString()] = item;
            }

            return result.ToString();
        }
    }
}
