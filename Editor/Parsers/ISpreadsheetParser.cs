using System.Collections.Generic;
namespace Yogi.UniGSC.Editor.Parsers {
    public interface ISpreadsheetParser {
        string Parse(int sheetId, IList<IList<object>> sheetData);
    }
}