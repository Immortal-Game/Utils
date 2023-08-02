using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using System.IO.Compression;
using UnityEditor.Callbacks;
using System;

namespace ImmGames.Publisher.Editor
{
    public class PublisherEditor : EditorWindow
    {
        private PublisherSettings settings;

        [MenuItem("IG/Upload")]
        private static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(PublisherEditor));
            window.titleContent = new GUIContent("IG Publisher");
            window.Show();
        }
        [MenuItem("IG/Clear Editor Player Prefs")]
        private static void ClearSettings()
        {
            PublisherSettings.Clear();
        }
        private void OnEnable()
        {
            settings = PublisherSettings.GetSettings();
        }

        private void OnGUI()
        {
            GUILayout.Label("Upload Folder Contents to Server", EditorStyles.boldLabel);
            GUILayout.Label("ProjectId: " + settings.ProjectId);
            EditorGUILayout.BeginHorizontal();
            settings.PathFolder = EditorGUILayout.TextField("Folder Path:", settings.PathFolder);

            if (GUILayout.Button("Browse", GUILayout.Width(100)))
            {
                settings.PathFolder = EditorUtility.OpenFolderPanel("Select Folder", "", "");
            }
            EditorGUILayout.EndHorizontal();
            settings.ChatId = EditorGUILayout.TextField("ChatId", settings.ChatId);

            settings.AutoStart = EditorGUILayout.Toggle("Upload after build", settings.AutoStart);

            settings.RunAfterUpload = EditorGUILayout.Toggle("Run after upload", settings.RunAfterUpload);

            if (GUILayout.Button("Upload", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(settings.PathFolder))
                {
                    settings.Save();
                    UploadFolderContents(settings);
                }
                else
                {
                    Debug.LogError("Path is empty!");
                }
            }
            if(!string.IsNullOrEmpty(settings.Url))
            {
                if (GUILayout.Button(settings.Url, EditorStyles.linkLabel))
                    Application.OpenURL(settings.Url);
            }
            
        }

        private void OnDisable()
        {
            settings.Save();
        }

        private static void UploadFolderContents(PublisherSettings settings)
        {
            string url = settings.ServerUrl;
            string folderPath = settings.PathFolder;


            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("Please select a folder first!");
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Selected folder path does not exist!");
                return;
            }

            if (!File.Exists(Path.Combine(folderPath, "index.html")))
            {
                Debug.LogError("The folder does not contain index.html!");
                return;
            }

            string zipFileName = UnityEngine.Random.Range(1000, 9999) + "unity.zip";
            string zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

            try
            {
                ZipFile.CreateFromDirectory(folderPath, zipFilePath);

                // Upload the ZIP file to the server
                string gameUrl = UploadFile(url, zipFilePath, settings.ProjectId,settings.ChatId);

                if (!string.IsNullOrEmpty(gameUrl))
                {
                    settings.Url = gameUrl;
                    settings.Save();
                    if(settings.RunAfterUpload)
                        Application.OpenURL(settings.Url);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create or upload the ZIP file. Error: {ex.Message}");
            }
            finally
            {
                File.Delete(zipFilePath);
            }
        }

        private static string UploadFile(string serverURL, string filePath, string guid,string ChatId)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);

            using (UnityWebRequest www = new UnityWebRequest(serverURL, UnityWebRequest.kHttpVerbPOST))
            {

                www.uploadHandler = new UploadHandlerRaw(fileData);
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/zip");
                www.SetRequestHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                www.SetRequestHeader("pid", guid);
                if(!string.IsNullOrEmpty(ChatId))www.SetRequestHeader("Chat-Id", ChatId);
                www.SetRequestHeader("Product-Name", PlayerSettings.productName);
                www.SendWebRequest();

                while (!www.isDone) { }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to upload {fileName}. Error: {www.error}");
                    return "";
                }
                
                var fileURL = www.downloadHandler.text;
                Debug.Log($"{fileName} successfully uploaded to the server!");
                Debug.Log("URL: " + fileURL);

                return fileURL;
                
            }

        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var settings = PublisherSettings.GetSettings();

            if (target == BuildTarget.WebGL && settings.AutoStart)
            {
                UploadFolderContents(settings);
            }
        }
    }
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
        public string ServerUrl => serverURL;
        public static PublisherSettings GetSettings()
        {
            var json = EditorPrefs.GetString(settingsKey, "");

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
                settingsKey,
                JsonUtility.ToJson(this)
                );
        }
        public static void Clear()
        {
            EditorPrefs.DeleteKey(settingsKey);
        }
    }
}