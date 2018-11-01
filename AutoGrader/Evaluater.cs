using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AutoGrader.Canvas;
using AutoGrader.Utils;

namespace AutoGrader
{
    public static class Evaluater
    {
        private const string UNKNOWN_INVALID = "Failed to run unit tests. Reason unclear.";

        private static readonly Dictionary<float, int> GradeCounts =
            new Dictionary<float, int>();
        private static readonly Dictionary<string, int> InvalidCounts =
            new Dictionary<string, int>();

        public static void PrintGradeCounts () {
            foreach (var gradeCount in GradeCounts.OrderByDescending(x => x.Key)) {
                Logger.Log($"{gradeCount.Key}:\t{gradeCount.Value}");
            }
        }

        public static void PrintInvalidCounts () {
            foreach (var invalidCount in InvalidCounts.OrderByDescending(x => x.Value)) {
                Logger.Log($"{invalidCount.Key}:\t{invalidCount.Value}");
            }
        }

        public static void IncrementInvalidCount (string reason) {
            if (InvalidCounts.TryGetValue(reason, out int count)) {
                InvalidCounts.Remove(reason);
                InvalidCounts.Add(reason, count + 1);
            }
            else {
                InvalidCounts.Add(reason, 1);
            }
        }

        public static void Grade (Submission submission) {
            if (!submission.Submitted || !submission.Valid) {
                Logger.Log("Skipping invalid submission");
                IncrementGradeCount(0f);
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
                catch (UnauthorizedAccessException e) {
                    Logger.Log("Unauthorized: " + e.Message);
                    Thread.Sleep(1000);
                }
                catch (IOException e) {
                    Logger.Log("IO: " + e.Message);
                    Thread.Sleep(1000);
                }

                if (tries-- != 0) { continue; }
                submission.Invalidate(UNKNOWN_INVALID);
                return;
            }

            string arguments = "/inisolation "
                            + (AutoGrader.Config.TestbedAdapterPath ?? " ")
                            + $" {AutoGrader.Config.TestbedDLLPath}";

            var startinfo = new ProcessStartInfo {
                FileName = AutoGrader.Config.VSTestPath,
                Arguments = arguments,
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
            float grade = MathF.Truncate(1000f * correct / AutoGrader.Config.TotalTests) / 10f;
            IncrementGradeCount(grade);
            submission.GiveFeedback(correct, incorrect, trimmed);
        }

        private static void IncrementGradeCount (float grade) {
            if (GradeCounts.TryGetValue(grade, out int count)) {
                GradeCounts.Remove(grade);
                GradeCounts.Add(grade, count + 1);
            }
            else {
                GradeCounts.Add(grade, 1);
            }
        }

        private static string MakeFeedbackLine (string line, bool correct) {
            // this is a hack; correctas and con error have the same number of letters lol
            string prefix = correct ? "Correct  " : "Incorrect";
            return prefix + line.Substring(10);
        }
    }
}
