using System.Collections.Generic;

namespace Yogi.GoogleSheetsConfig.Editor.Parsers {
    public interface ISpreadsheetParser {
        string Parse(int sheetId, IList<IList<object>> sheetData);
    }
}