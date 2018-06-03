using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public static class Evaluater
    {
        public static void Grade (Submission submission) {
            if (!submission.Submitted || !submission.Valid) {
                Logger.Log("Skipping invalid submission");
                return;
            }

            // todo figure this bad boy out
            int tries = 10;
            while (true) {
                try {
                    string exe = AutoGrader.Config.TestExecutablePath;
                    File.Copy(submission.ResultPath, exe, true);
                    File.SetAttributes(exe, FileAttributes.Normal);
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
                FileName = AutoGrader.Config.VSTestPath,
                Arguments = "/inisolation /testadapterpath:" + AutoGrader.Config.TestbedAdapterPath + " " + AutoGrader.Config.TestbedDLLPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var test = Process.Start(startinfo);
            if (test == null) { return; }
            ProcessResults(submission, test.StandardOutput);
            test.WaitForExit();
        }

        private static void ProcessResults (Submission submission, TextReader outstream) {
            string output = outstream.ReadToEnd();
            var sb = new StringBuilder();
            int correct = 0, incorrect = 0;
            foreach (string line in output.Split("\n")) {
                if (line.StartsWith("Correct")) {
                    correct++;
                    sb.AppendLine(MakeFeedbackLine(line, true));
                }
                else if (line.StartsWith("Con error")) {
                    incorrect++;
                    sb.AppendLine(MakeFeedbackLine(line, false));
                }
            }

            string trimmed = sb.ToString();
            submission.GiveFeedback(correct, incorrect, trimmed);
        }

        private static string MakeFeedbackLine (string line, bool correct) {
            // this is a hack; correctas and con error have the same number of letters lol
            string prefix = correct ? "Correct  " : "Incorrect";
            return prefix + line.Substring(10);
        }
    }
}
