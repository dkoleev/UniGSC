using System.Collections.Generic;

namespace Yogi.GoogleSheetsConfig.Editor {
    public interface ISpreadsheetParser {
        string Parse(int sheetId, IList<IList<object>> sheetData);
    }
}