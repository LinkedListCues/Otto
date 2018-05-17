using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public static class Grader
    {
        private const string DLL_PATH = @"C:\Users\Ethan\AppData\Roaming\214AutoGrader\Testbed\Tests\bin\Debug\Tests.dll";
        private const string ADAPTER_PATH =
            @"/testadapterpath:C:\Users\Ethan\AppData\Roaming\214AutoGrader\Testbed\packages\MSTest.TestAdapter.1.2.1";
        private const string VSTEST_PATH = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe";
        private const string EXE_PATH =
            @"C:\Users\Ethan\AppData\Roaming\214AutoGrader\Testbed\Tests\bin\Debug\HappyFunBlobZero.exe";

        public const int TOTAL_TESTS = 27;

        public static void Grade (Submission submission, int index, int count) {
            Logger.Log($"Grading {submission.SubmissionID} ({index} of {count})");
            if (!submission.Submitted || !submission.Valid) {
                Logger.Log("Skipping invalid submission");
                return;
            }

            // todo figure this bad boy out
            int tries = 10;
            while (true) {
                try {
                    File.Copy(submission.ResultPath, EXE_PATH, true);
                    File.SetAttributes(EXE_PATH, FileAttributes.Normal);
                    break;
                }
                catch (UnauthorizedAccessException) {
                    Thread.Sleep(1000);
                }
                catch (IOException) {
                    Thread.Sleep(1000);
                }

                if (tries-- != 0) { continue; }
                submission.Invalidate("Failed to run unit tests. Reason unclear.");
                return;
            }

            var startinfo = new ProcessStartInfo {
                FileName = VSTEST_PATH,
                Arguments = "/inisolation " + ADAPTER_PATH + " " + DLL_PATH,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var test = Process.Start(startinfo);
            if (test == null) { return; }
            ProcessResults(submission, test.StandardOutput, test.StandardError);
            test.WaitForExit();
        }

        private static void ProcessResults (Submission submission, StreamReader outstream, StreamReader errorstream) {
            string output = outstream.ReadToEnd();
            int correct = 0, incorrect = 0;
            foreach (string line in output.Split("\n")) {
                if (line.StartsWith("Correct")) { correct++; }
                else if (line.StartsWith("Mensaje de error")) { incorrect++; }
            }

            string error = errorstream.ReadToEnd();

            float grade = MathF.Truncate(1000f * correct / TOTAL_TESTS) / 10f;
            submission.GiveFeedback(grade, correct, incorrect, TrimOutput(output), error);
        }

        private static string TrimOutput (string output) {
            var trimmedOutput = new StringBuilder();
            var lines = output.Split("\n");

            // skip the first five lines, skip the last 4 lines
            for (int i = 5; i < lines.Length - 4; i++) { trimmedOutput.AppendLine(lines[i]); }
            return trimmedOutput.ToString();
        }

        // todo explain stackoverflow exception
        /*
         * pruebas: 0.7135 Segundos
/m/c/U/E/A/R/2/Testbed ❯❯❯ "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "/testadapterpath:./packages/MSTest.TestAdapter.1.2.1" ./Tests/bin/debug/Tests.dll

         */
    }
}
