using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yogi.UniGSC.Editor {
    [CreateAssetMenu(fileName = "Google Sheets Configs", menuName = "Yogi/Google Sheets Configs/Configs", order = 2)]
    public class GoogleSheetsConfigs : ScriptableObject {
        [field: SerializeField] public GoogleSheetsServiceProvider Provider { get; private set; }
        [field: SerializeField] public List<GoogleSpreadSheetConfigData> Configs { get; private set; }

        public void PullAllConfigs() {
            Provider.PullAllSheets(Configs);
        }

        public void PullConfig(GoogleSpreadSheetConfigData data) {
            Provider.PullSheet(data);
        }

        public void OpenSpreadsheet(GoogleSpreadSheetConfigData data) {
            Provider.OpenSheetInBrowser(data);
        }
    }

    [Serializable]
    public class GoogleSpreadSheetConfigData {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string SpreadSheet { get; private set; }
        [field: SerializeField] public List<GoogleSheetConfigData> Sheets { get; private set; }
    }

    [Serializable]
    public class GoogleSheetConfigData {
        [field: SerializeField] public string ConfigName { get; private set; }
        [field: SerializeField] public int SheetId { get; private set; }
        [field: SerializeField] public string Range { get; private set; }
        [field: SerializeField] public string Parser { get; private set; }
    }
}
