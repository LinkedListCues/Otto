using System.Diagnostics;
using System.IO;
using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public class Builder
    {
        private readonly string _msBuildPath = Path.Combine("C:\\\\", "Program Files (x86)", "Microsoft Visual Studio", "2017", "Community", "MSBuild", "15.0", "Bin", "MSBuild.exe");

        public bool Build (Submission submission, string solutionpath) {
            Logger.Log($"Building {submission.SubmissionID}");

            var startinfo = new ProcessStartInfo {
                FileName = _msBuildPath,
                // n.b. the quotes are here because Ian has a love of spaces in directory names
                Arguments = "/nologo \"" + solutionpath + "\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var build = Process.Start(startinfo);
            if (build == null) { return false; }
            string output = build.StandardOutput.ReadToEnd();
            build.WaitForExit();

            return output.Contains("0 Error");
        }
    }
}
