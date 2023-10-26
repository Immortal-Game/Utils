using System;

namespace ImmGames.Publisher.Editor.Uploaders
{
    public interface IUploader
    {
        void UploadFolderContents(PublisherSettings settings, string pathToFolder);
    }

    public static class Uploader
    {
        public static IUploader GetUploader(UploadType type)
        {
            switch (type)
            {
                case UploadType.ToGitRemote:
                    return new RemoteGItUploader();
                case UploadType.ToIGBackend:
                    return new ImmortalsGamesBackendUploader();
                default:
                    throw new Exception("Unexpected exception");
            }
        }
    }
}