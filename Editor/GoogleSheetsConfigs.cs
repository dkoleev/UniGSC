using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yogi.GoogleSheetsConfig.Editor {
    [CreateAssetMenu(fileName = "Google Sheets Configs", menuName = "Yogi/Google Sheets Configs/Configs", order = 2)]
    internal class GoogleSheetsConfigs : ScriptableObject {
        [field: SerializeField] internal GoogleSheetsServiceProvider Provider { get; private set; }
        [field: SerializeField] internal List<GoogleSpreadSheetConfigData> Configs { get; private set; }

        internal void PullAllConfigs() {
            Provider.PullAllSheets(Configs);
        }
    }

    [Serializable]
    internal class GoogleSpreadSheetConfigData {
        [SerializeField] private string name;
        [field: SerializeField] public string SpreadSheet { get; private set; }
        [field: SerializeField] public List<GoogleSheetConfigData> Sheets { get; private set; }
    }

    [Serializable]
    internal class GoogleSheetConfigData {
        [field: SerializeField] public string ConfigName { get; private set; }
        [field: SerializeField] public int SheetId { get; private set; }
        [field: SerializeField] public string Range { get; private set; }
        [field: SerializeField] public string Parser { get; private set; }
    }
}
