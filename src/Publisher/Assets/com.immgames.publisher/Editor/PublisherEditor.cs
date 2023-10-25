using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ImmGames.Publisher.Editor.Uploaders;
using ImmGames.Publisher.Editor.Utils;

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

            settings.UploadType = (UploadType)EditorGUILayout.EnumPopup("Upload type:", settings.UploadType);

            if (settings.UploadType == UploadType.ToGitRemote)
            {
                settings.TargetRepo = EditorGUILayout.TextField("Target repository:", settings.TargetRepo);
            }
            
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
                    Uploader.GetUploader(settings.UploadType)
                        .UploadFolderContents(settings, settings.PathFolder);
                }
                else
                {
                    Debug.LogError("Path is empty!");
                }
            }
            ShowLinkAtTheEnd();
        }

        private void ShowLinkAtTheEnd()
        {
            if (!string.IsNullOrEmpty(settings.Url) && settings.UploadType == UploadType.ToIGBackend)
            {
                if (GUILayout.Button(settings.Url, EditorStyles.linkLabel))
                    Application.OpenURL(settings.Url);
            }
            else if (!string.IsNullOrEmpty(settings.TargetRepo) && settings.UploadType == UploadType.ToGitRemote)
            {
                var githubPage = Github.MakePagesLinkFromRepo(settings.TargetRepo);
                if (GUILayout.Button(githubPage, EditorStyles.linkLabel))
                    Application.OpenURL(githubPage);
            }
        }

        private void OnDisable()
        {
            settings.Save();
        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var settings = PublisherSettings.GetSettings();

            if (target == BuildTarget.WebGL && settings.AutoStart)
            {
                Uploader.GetUploader(settings.UploadType)
                    .UploadFolderContents(settings, pathToBuiltProject);
            }
        }
    }
}