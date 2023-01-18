using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEditor;
using UnityEngine;

namespace Yogi.UniGSC.Editor {
    [CustomEditor(typeof(GoogleSheetsServiceProvider))]
    public class GoogleSheetsServiceProviderEditor : UnityEditor.Editor {
        private SerializedProperty _clientId;
        private SerializedProperty _clientSecret;
        private SerializedProperty _authenticationType;
        private SerializedProperty _applicationName;
        private SerializedProperty _apiKey;

        private static Task<UserCredential> _authorizeTask;
        private static CancellationTokenSource _cancellationToken;

        private ClientSecrets _secrets;
        private AuthenticationType _prevAuthenticationType = AuthenticationType.None;

        private void OnEnable() {
            _clientId = serializedObject.FindProperty("clientId");
            _clientSecret = serializedObject.FindProperty("clientSecret");
            _authenticationType = serializedObject.FindProperty("authenticationType");
            _apiKey = serializedObject.FindProperty("apiKey");
            _applicationName = serializedObject.FindProperty("applicationName");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            base.OnInspectorGUI();

            var provider = (GoogleSheetsServiceProvider)target;
            
            EditorGUILayout.PropertyField(_applicationName);
            EditorGUILayout.PropertyField(_authenticationType, EditorGUIUtility.TrTextContent("Authentication"));
            
            var auth = (AuthenticationType)_authenticationType.intValue;
            if (auth != _prevAuthenticationType) {
                provider.ResetService();
                _prevAuthenticationType = auth;
            }
            
            if (auth == AuthenticationType.APIKey) {
                EditorGUILayout.HelpBox("API Key can be used for reading from public sheets only.", MessageType.Info);
                EditorGUILayout.PropertyField(_apiKey, EditorGUIUtility.TrTextContent("API Key"));
            } else if (auth == AuthenticationType.OAuth) {
                EditorGUILayout.HelpBox("OAuth 2.0 authorization allows reading and writing to both public and private sheets.", MessageType.Info);
                EditorGUILayout.PropertyField(_clientId);
                EditorGUILayout.PropertyField(_clientSecret);
                
                if (GUILayout.Button("Load Credentials", GUILayout.Height(30))) {
                    _secrets = LoadCredentials();
                    _clientId.stringValue = _secrets.ClientId;
                    _clientSecret.stringValue = _secrets.ClientSecret;
                }

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_clientId.stringValue) || string.IsNullOrEmpty(_clientSecret.stringValue));

                if (_authorizeTask != null) {
                    if (GUILayout.Button("Cancel")) {
                        _cancellationToken.Cancel();
                    }

                    if (_authorizeTask.IsCompleted) {
                        if (_authorizeTask.Status == TaskStatus.RanToCompletion) {
                            Debug.Log($"Authorized: {_authorizeTask.Result.Token.IssuedUtc}", target);
                        }
                        else if (_authorizeTask.Exception != null) {
                            Debug.LogException(_authorizeTask.Exception, target);
                        }
                        
                        _authorizeTask = null;
                        _cancellationToken = null;
                    }
                }
                else {
                    if (GUILayout.Button("Authorize", GUILayout.Height(30))) {
                        _cancellationToken = new CancellationTokenSource();
                        _authorizeTask = provider.AuthorizeOAuthAsync(_cancellationToken.Token);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        internal ClientSecrets LoadCredentials() {
            var file = EditorUtility.OpenFilePanel("Load Credentials", "", "json");
            if (!string.IsNullOrEmpty(file)) {
                var json = File.ReadAllText(file);
                var secrets = GoogleSheetsServiceProvider.LoadSecrets(json);

                return secrets;
            }

            return null;
        }
    }
}