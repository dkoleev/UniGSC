using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using UnityEngine;

namespace GoogleSheetsConfig.Editor {
    public class SpreadSheetLoader {
         private static readonly string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};
        private static string ApplicationName = "Google Sheets API .NET";
        private readonly SheetsService _sheetsService;
        private readonly Dictionary<string, IList<Sheet>> _spreadsheetSheets = new Dictionary<string, IList<Sheet>>();

        public SpreadSheetLoader() {
            _sheetsService = new SheetsService(new BaseClientService.Initializer() {
                HttpClientInitializer = GetCredentials(),
                ApplicationName = ApplicationName,
            });
        }

        public string Load(ISpreadsheetParser parser,
            string spreadSheetId,
            int sheetId,
            string range,
            SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum majorDimensionEnum =
                SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS) {
            var sheetData = LoadSheet(spreadSheetId, sheetId, range, majorDimensionEnum);
            var jsonString = parser.Parse(sheetId, sheetData);

            return jsonString;
        }

        private IList<IList<object>> LoadSheet(string spreadsheetId,
            int sheetId,
            string sheetRange = "",
            SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum majorDimensionEnum =
                SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS) {
            var sheetName = GetSheetNameById(spreadsheetId, sheetId, _sheetsService);

            var range = sheetName;
            if (!string.IsNullOrEmpty(sheetRange)) {
                range += $"!{sheetRange}";
            }
            var valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum) 0;
            var dateTimeRenderOption =
                (SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum)
                0; // TODO: Update placeholder value.

            var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
            request.ValueRenderOption = valueRenderOption;
            request.DateTimeRenderOption = dateTimeRenderOption;
            request.MajorDimension = majorDimensionEnum;

            var response = request.Execute();
            
            return response.Values;
        }

        private UserCredential GetCredentials() {
            UserCredential credential;

            using (var stream =
                new FileStream(Application.dataPath + "/../configs_credentials.json", FileMode.Open, FileAccess.Read)) {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = Application.dataPath + "/../token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return credential;
        }

        private string GetSheetNameById(string spreadSheetId, int sheetId, SheetsService service) {
            if (_spreadsheetSheets.ContainsKey(spreadSheetId)) {
                return GetName(sheetId, _spreadsheetSheets[spreadSheetId]);
            } 
            
            List<string> ranges = new List<string>();
            bool includeGridData = false;

            SpreadsheetsResource.GetRequest request = service.Spreadsheets.Get(spreadSheetId);
            request.Ranges = ranges;
            request.IncludeGridData = includeGridData;

            Spreadsheet response = request.Execute();
            
            _spreadsheetSheets.Add(spreadSheetId, response.Sheets);
            
            return GetName(sheetId, _spreadsheetSheets[spreadSheetId]);
            
            string GetName(int sheetId, IList<Sheet> sheets) {
                foreach (var sheet in sheets) {
                    if (sheet.Properties.SheetId == sheetId) {
                        return sheet.Properties.Title;
                    }
                }

                return null;
            }
        }
    }
}