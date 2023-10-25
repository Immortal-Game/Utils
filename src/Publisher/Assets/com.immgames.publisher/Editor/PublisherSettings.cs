using System;
using UnityEditor;
using UnityEngine;

namespace ImmGames.Publisher.Editor
{
    [Serializable]
    public class PublisherSettings
    {
        private const string serverURL = "https://yandex.immgames.ru/api/Upload";
        private const string settingsKey = "IG.PublisherSettings";
        
        public string ProjectId;
        public bool AutoStart;
        public string PathFolder;
        public string Url;
        public string ChatId;
        public bool RunAfterUpload;
        public string ProductName;
        public UploadType UploadType = UploadType.ToIGBackend;
        public string TargetRepo = "https://github.com/username/repo-name";
        
        public string ServerUrl => serverURL;
        public static string ProjectKey => $"{PlayerSettings.productName}.{settingsKey}";
        
        public static PublisherSettings GetSettings()
        {
            var json = EditorPrefs.GetString(ProjectKey, "");

            PublisherSettings settings;

            if (string.IsNullOrEmpty(json))
            {
                string guid = GUID.
                    Generate()
                    .ToString()
                    .Replace("-", "");

                settings = new PublisherSettings()
                {
                    AutoStart = false,
                    PathFolder = "",
                    ProjectId = guid,
                    RunAfterUpload = false,
                    Url = "",
                    ChatId = ""
                };
            }
            else
            { 
                settings = JsonUtility.FromJson<PublisherSettings>(json); 
            }

            return settings;
        }

        public void Save()
        {
            EditorPrefs.SetString(
                ProjectKey,
                JsonUtility.ToJson(this)
            );
        }
        
        public static void Clear()
        {
            EditorPrefs.DeleteKey(ProjectKey);
        }
    }
}