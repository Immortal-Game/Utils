using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ImmGames.Publisher.Editor.Uploaders
{
    public class ImmortalsGamesBackendUploader : IUploader
    {
        public void UploadFolderContents(PublisherSettings settings, string pathToFolder)
        {
            string url = settings.ServerUrl;

            if (string.IsNullOrEmpty(pathToFolder))
            {
                Debug.LogError("Please select a folder first!");
                return;
            }

            if (!Directory.Exists(pathToFolder))
            {
                Debug.LogError("Selected folder path does not exist!");
                return;
            }

            if (!File.Exists(Path.Combine(pathToFolder, "index.html")))
            {
                Debug.LogError("The folder does not contain index.html!");
                return;
            }

            string zipFileName = UnityEngine.Random.Range(1000, 9999) + "unity.zip";
            string zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

            try
            {
                ZipFile.CreateFromDirectory(pathToFolder, zipFilePath);

                // Upload the ZIP file to the server
                string gameUrl = UploadFile(url, zipFilePath, settings.ProjectId, settings.ChatId);

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

        private string UploadFile(string serverURL, string filePath, string guid, string ChatId)
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
                if(!string.IsNullOrEmpty(ChatId)) www.SetRequestHeader("Chat-Id", ChatId);
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
    }
}