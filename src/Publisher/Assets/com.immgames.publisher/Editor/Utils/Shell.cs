using System.Diagnostics;
using System.Linq;

namespace ImmGames.Publisher.Editor.Utils
{
    public class Shell
    {
        private readonly string _dir;

        public Shell(string dir)
        {
            _dir = dir;
        }
        
        public string RunAndGetStdout(string program, params string[] args)
        {
            ProcessStartInfo gitInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = program,
                UseShellExecute = false
            };
            gitInfo.Arguments = args.Aggregate((res, x) => res + " " + x);
            gitInfo.WorkingDirectory = _dir;

            var gitProcess = new Process();
            gitProcess.StartInfo = gitInfo;
            gitProcess.Start();
            
            string stderr_str = gitProcess.StandardError.ReadToEnd();  // pick up STDERR
            string stdout_str = gitProcess.StandardOutput.ReadToEnd(); // pick up STDOUT
            
            gitProcess.WaitForExit();
            gitProcess.Close();

            return stderr_str + "\n" + stdout_str;
        }
    }
}