using System.IO;
using UnityEngine;

namespace ImmGames.Publisher.Editor.Utils
{
    public class Git
    {
        private readonly string _folder;

        public Git(string folder)
        {
            _folder = folder;
        }

        public void Init()
        {
            var shell = new Shell(_folder);
            Debug.Log(shell.RunAndGetStdout("git", "init"));
        }

        public void SetOriginRemote(string remoteUrl)
        {
            var shell = new Shell(_folder);
            Debug.Log(shell.RunAndGetStdout("git", "remote", "add", "origin", remoteUrl));
        }

        public void PullRebase(string branch = "master")
        {
            var shell = new Shell(_folder);
            Debug.Log(shell.RunAndGetStdout("git", "pull", "origin", branch, "--rebase"));
        }
        
        public void AddAll()
        {
            var shell = new Shell(_folder);
            Debug.Log(shell.RunAndGetStdout("git", "add", "."));
        }
        
        public void Commit(string message)
        {
            var shell = new Shell(_folder);
            Debug.Log(shell.RunAndGetStdout("git", "commit", "-m", message));
        }

        public void PushOrigin(string branchName = "master")
        {
            var shell = new Shell(_folder);
            Debug.Log(shell.RunAndGetStdout("git", "push", "origin", branchName));
        }
    }
}