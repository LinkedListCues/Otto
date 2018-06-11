using System.Diagnostics;
using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public static class Builder
    {
        public static bool Build (Submission submission, string solutionpath) {
            Logger.Log($"Building {submission.SubmissionID}");

            var startinfo = new ProcessStartInfo {
                FileName = AutoGrader.Config.MSBuildPath,
                // n.b. the quotes are here because Ian has a love of spaces in directory names
                // N.B.: /t:restore says, "please reinstall the missing NUGet packages."
                // Should this be necessary? No, but fuck you.

                // todo maybe rebuild?
                Arguments = "/nologo /t:restore /t:rebuild \"" + solutionpath + "\"",
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
