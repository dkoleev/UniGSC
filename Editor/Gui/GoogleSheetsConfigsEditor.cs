using UnityEditor;
using UnityEngine;

namespace Yogi.UniGSC.Editor.Gui {
    [CustomEditor(typeof(GoogleSheetsConfigs))]
    public class GoogleSheetsConfigsEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var configs = (GoogleSheetsConfigs)target;
            if (GUILayout.Button("Pull Configs from Google Drive", GUILayout.Height(50))) {
                configs.PullAllConfigs();
            }
        }
    }
}
