using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Yogi.UniGSC.Editor.Parsers
{
    [ParserType("grouped")]
    public class SpreadsheetParserGrouped : ISpreadsheetParser
    {
        public string Parse(int sheetId, IList<IList<object>> sheetData)
        {
            var headers = sheetData[0];
            var result = new JObject();
            var groups = BuildGroups(sheetData);

            foreach (var group in groups)
            {
                var id = group[0][0].ToString();
                var item = new JObject();

                for (var col = 0; col < headers.Count; col++)
                {
                    var rawHeader = headers[col].ToString();
                    if (string.IsNullOrEmpty(rawHeader))
                        continue;

                    var (header, forceArray) = SpreadSheetsParserUtils.ParseHeader(rawHeader);
                    var values = CollectColumnValues(group, col);
                    var token = SpreadSheetsParserUtils.ToJsonToken(values, forceArray);
                    if (token != null)
                        item[header] = token;
                }

                result[id] = item;
            }

            return result.ToString();
        }

        private static List<List<IList<object>>> BuildGroups(IList<IList<object>> sheetData)
        {
            var groups = new List<List<IList<object>>>();
            List<IList<object>> current = null;

            for (var i = 1; i < sheetData.Count; i++)
            {
                var row = sheetData[i];
                var hasId = row.Count > 0 && !string.IsNullOrEmpty(row[0]?.ToString());

                if (hasId)
                {
                    current = new List<IList<object>> { row };
                    groups.Add(current);
                }
                else
                {
                    current?.Add(row);
                }
            }

            return groups;
        }

        private static List<object> CollectColumnValues(List<IList<object>> group, int col)
        {
            var values = new List<object>();

            foreach (var row in group)
            {
                if (col >= row.Count)
                    continue;

                var cell = row[col]?.ToString();
                if (string.IsNullOrEmpty(cell))
                    continue;

                values.AddRange(SpreadSheetsParserUtils.ParseCell(cell));
            }

            return values;
        }
    }
}
