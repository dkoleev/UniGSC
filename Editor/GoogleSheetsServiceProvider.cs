using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using UnityEditor;
using UnityEngine;
using Yogi.UniGSC.Editor.Parsers;

namespace Yogi.UniGSC.Editor {
    public enum AuthenticationType {
        None,
        OAuth,
        APIKey
    }

    /// <summary>
    /// The Sheets service provider performs the authentication to Google and keeps track of the authentication tokens
    /// so that you do not need to authenticate each time.
    /// </summary>
    [CreateAssetMenu(fileName = "Google Sheets Provider", menuName = "Yogi/Google Sheets Configs/Provider", order = 1)]
    [HelpURL("https://developers.google.com/sheets/api/guides/authorizing#AboutAuthorization")]
    public class GoogleSheetsServiceProvider : ScriptableObject {
        [SerializeField] [HideInInspector] private string apiKey;
        [SerializeField] [HideInInspector] private string clientId;
        [SerializeField] [HideInInspector] private string clientSecret;
        [SerializeField] [HideInInspector] private AuthenticationType authenticationType;
        [SerializeField] [HideInInspector] private string applicationName;

        private SheetsService _sheetsService;
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };

        /// <summary>
        /// Used to make sure the access and refresh tokens persist. Uses a FileDataStore by default with "Library/Google/{name}" as the path.
        /// </summary>
        internal IDataStore DataStore { get; private set; }

        /// <summary>
        /// The Google Sheet service that will be created using the Authorization API.
        /// </summary>
        internal virtual SheetsService Service => _sheetsService ??= Connect();

        /// <summary>
        /// The authorization methodology to use.
        /// See <see href="https://developers.google.com/sheets/api/guides/authorizing"/>
        /// </summary>
        internal AuthenticationType Authentication => authenticationType;

        /// <summary>
        /// The API Key to use when using <see cref="AuthenticationType.APIKey"/> authentication.
        /// </summary>
        internal string ApiKey => apiKey;

        /// <summary>
        /// <para>Client Id when using OAuth authentication.</para>
        /// See also <seealso cref="SetOAuthCredentials"/>
        /// </summary>
        internal string ClientId => clientId;

        /// <summary>
        /// <para>Client secret when using OAuth authentication.</para>
        /// See also <seealso cref="SetOAuthCredentials"/>
        /// </summary>
        internal string ClientSecret => clientSecret;

        /// <summary>
        /// The name of the application that will be sent when connecting.
        /// </summary>
        internal string ApplicationName {
            get => applicationName;
            set => applicationName = value;
        }

        private readonly Dictionary<string, IList<Sheet>> _spreadsheetSheets = new Dictionary<string, IList<Sheet>>();

        internal void OnEnable() {
            if (string.IsNullOrEmpty(ApplicationName))
                ApplicationName = PlayerSettings.productName;
        }

        /// <summary>
        /// Set the API Key. An API key can only be used for reading from a public Google Spreadsheet.
        /// </summary>
        /// <param name="apiKey"></param>
        internal void SetApiKey(string apiKey) {
            this.apiKey = apiKey;
            authenticationType = AuthenticationType.APIKey;
        }

        /// <summary>
        /// Enable OAuth 2.0 authentication and extract the <see cref="ClientId"/> and <see cref="ClientSecret"/> from the supplied json.
        /// </summary>
        /// <param name="credentialsJson"></param>
        internal void SetOAuthCredentials(string credentialsJson) {
            var secrets = LoadSecrets(credentialsJson);
            clientId = secrets.ClientId;
            clientSecret = secrets.ClientSecret;
            authenticationType = AuthenticationType.OAuth;
        }

        /// <summary>
        /// Enable OAuth 2.0 authentication with the provided client Id and client secret.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        internal void SetOAuthCredentials(string clientId, string clientSecret) {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            authenticationType = AuthenticationType.OAuth;
        }

        internal void ResetService() {
            _sheetsService = null;
        }

        private SheetsService Connect() {
            if (Authentication == AuthenticationType.None) {
                throw new Exception("No connection credentials. You must provide either OAuth2.0 credentials or an Api Key.");
            }

            if (Authentication == AuthenticationType.OAuth) {
                return ConnectWithOAuth2();
            }
            
            return ConnectWithApiKey();
        }

        /// <summary>
        /// When calling an API that does not access private user data, you can use a simple API key.
        /// This key is by Google to authenticate your application for accounting purposes.
        /// If you do need to access private user data, you must use OAuth 2.0.
        /// </summary>
        private SheetsService ConnectWithApiKey() {
            SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer {
                ApiKey = apiKey,
                ApplicationName = ApplicationName
            });
            return sheetsService;
        }

        /// <summary>
        /// Call to preauthorize when using OAuth authorization. This will cause a browser to open a Google authorization
        /// page after which the token will be stored in IDataStore so that this does not need to be done each time.
        /// If this is not called then the first time <see cref="Service"/> is called it will be performed then.
        /// </summary>
        /// <returns></returns>
        internal UserCredential AuthorizeOAuth() {
            // Prevents Unity locking up if the user canceled the auth request.
            // Auto cancel after 60 secs
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var connectTask = AuthorizeOAuthAsync(cts.Token);
            if (!connectTask.IsCompleted)
                connectTask.RunSynchronously();

            if (connectTask.Status == TaskStatus.Faulted) {
                throw new Exception($"Failed to connect to Google Sheets.\n{connectTask.Exception}");
            }
            return connectTask.Result;
        }

        /// <summary>
        /// Call to preauthorize when using OAuth authorization. This will cause a browser to open a Google authorization
        /// page after which the token will be stored in IDataStore so that this does not need to be done each time.
        /// If this is not called then the first time <see cref="Service"/> is called it will be performed then.
        /// </summary>
        /// <param name="cancellationToken">Token that can be used to cancel the task prematurely.</param>
        /// <returns>The authorization Task that can be monitored.</returns>
        internal Task<UserCredential> AuthorizeOAuthAsync(CancellationToken cancellationToken) {
            if (string.IsNullOrEmpty(ClientSecret)) {
                throw new Exception($"{nameof(ClientSecret)} is empty");
            }

            if (string.IsNullOrEmpty(ClientId)) {
                throw new Exception($"{nameof(ClientId)} is empty");
            }

            // Create a separate area for each so that multiple providers don't clash.
            var dataStore = DataStore ?? new FileDataStore($"Library/Google/{name}", true);

            var secrets = new ClientSecrets {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            // Use the client Id for the user so that we can generate a unique token file and prevent conflicts when using multiple OAuth authentications. (LOC-188)
            var user = clientId;
            var connectTask = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, _scopes, user, cancellationToken, dataStore);
            return connectTask;
        }

        /// <summary>
        /// When calling an API that will access private user data, O Auth 2.0 credentials must be used.
        /// </summary>
        private SheetsService ConnectWithOAuth2() {
            var userCredentials = AuthorizeOAuth();
            var sheetsService = new SheetsService(new BaseClientService.Initializer {
                HttpClientInitializer = userCredentials,
                ApplicationName = ApplicationName,
            });
            
            return sheetsService;
        }

        internal static ClientSecrets LoadSecrets(string credentials) {
            if (string.IsNullOrEmpty(credentials)) {
                throw new ArgumentException(nameof(credentials));
            }

            using var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(credentials));
            var gcs = GoogleClientSecrets.FromStream(stream);
            return gcs.Secrets;
        }

        internal string Load(ISpreadsheetParser parser,
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
            var sheetName = GetSheetNameById(spreadsheetId, sheetId, Service);

            var range = sheetName;
            if (!string.IsNullOrEmpty(sheetRange)) {
                range += $"!{sheetRange}";
            }
            var valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)0;
            var dateTimeRenderOption =
                (SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum)
                0; // TODO: Update placeholder value.

            var request = Service.Spreadsheets.Values.Get(spreadsheetId, range);
            request.ValueRenderOption = valueRenderOption;
            request.DateTimeRenderOption = dateTimeRenderOption;
            request.MajorDimension = majorDimensionEnum;

            var response = request.Execute();

            return response.Values;
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

        internal void OpenSheetInBrowser(GoogleSpreadSheetConfigData data) =>
            UnityEngine.Application.OpenURL($"https://docs.google.com/spreadsheets/d/{data.SpreadSheet}/#gid={data.Sheets[0].SheetId}");

        internal void SaveSheetToJsonFile(GoogleSpreadSheetConfigData data) {
            foreach (var dataSheet in data.Sheets) {
                var parser = GetSpreadsheetParser(dataSheet.Parser);
                var sheetData = Load(parser, data.SpreadSheet, dataSheet.SheetId, dataSheet.Range, SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS);
                SaveConfigFile(dataSheet.ConfigName, sheetData);
            }
        }

        private ISpreadsheetParser GetSpreadsheetParser(string configType) {
            var assemblyCollection = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblyCollection) {
                var targetType = assembly
                    .GetTypes().FirstOrDefault(x =>
                        typeof(ISpreadsheetParser).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract
                        && x.GetCustomAttribute<ParserTypeAttribute>()?.ParserType == configType);

                if (targetType != null) {
                    return Activator.CreateInstance(targetType) as ISpreadsheetParser;
                }
            }

            return null;
        }

        private void SaveConfigFile(string configPath, string configData) {
            string filePath = $"{configPath}.json";
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory)) {
                var newDirPath = Path.Combine(Application.dataPath, directory);
                Directory.CreateDirectory(newDirPath);
            }
            
            File.WriteAllText(System.IO.Path.Combine(Application.dataPath, filePath), configData);
            AssetDatabase.Refresh();
        }

        internal void PullAllSheets(List<GoogleSpreadSheetConfigData> configs) {
            foreach (var configData in configs) {
                PullSheet(configData);
            }
        }
        
        internal void PullSheet(GoogleSpreadSheetConfigData data) {
            SaveSheetToJsonFile(data);
        }
    }
}
