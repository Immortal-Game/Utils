using ImmGames.Publisher.Editor.Utils;
using UnityEngine;

namespace ImmGames.Publisher.Editor.Uploaders
{
    public class RemoteGItUploader : IUploader
    {
        public void UploadFolderContents(PublisherSettings settings, string pathToFolder)
        {
            var git = new Git(pathToFolder);
            git.Init();
            git.SetOriginRemote(settings.TargetRepo);
            git.PullRebase();
            git.AddAll();
            git.Commit("Publishing");
            git.PushOrigin();
            if (settings.RunAfterUpload) 
                Application.OpenURL(settings.TargetRepo);
        }
    }
}