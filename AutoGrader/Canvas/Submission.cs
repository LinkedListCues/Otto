using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using AutoGrader.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoGrader.Canvas
{
    public class Submission
    {
        [JsonProperty("SubmissionID")] public readonly string SubmissionID;
        [JsonProperty("UserID")] public readonly string UserID;
        [JsonProperty("LatePenalty")] public readonly float LatePenalty;
        [JsonProperty("AttachmentURL")] public readonly string AttachmentURL;

        [JsonProperty("Submitted")] public readonly bool Submitted;

        public bool Valid = true;
        public string ResultPath { get; private set; }

        private Feedback _feedback;

        public Submission () { }

        public Submission (JObject json) {
            SubmissionID = json["id"].ToString();
            UserID = json["user_id"].ToString();
            LatePenalty = CalculateLatePenalty((int)json["seconds_late"]);

            Submitted = json.ContainsKey("attachments");
            if (Submitted) { AttachmentURL = json["attachments"][0]["url"].ToString(); }
        }

        private static float CalculateLatePenalty (int secondslate) {
            if (secondslate == 0) { return 0f; }
            int hourslate = secondslate / 3600;
            if (hourslate < 50) { return 0f; }
            return hourslate < 220 ? 0.3f : 1f;
        }

        private void DownloadAndUnzip (string zippath, string directory) {
            if (!File.Exists(zippath)) {
                using (var client = new WebClient()) {
                    client.DownloadFile(AttachmentURL, zippath);
                    Logger.Log("Wrote to " + zippath);
                }
            }

            Logger.Log($"Unzipping {zippath}");
            try {
                ZipFile.ExtractToDirectory(zippath, directory);
                File.Delete(zippath);
            }
            // The .zip is invalid, presumably.
            catch (InvalidDataException) { Invalidate("Submission (.zip file) corrupted."); }

            CleanUpMacDirectory(directory);
        }

        private static void CleanUpMacDirectory (string directory) {
            string osx = Path.Combine(directory, "_MACOSX");
            if (!Directory.Exists(osx)) { return; }
            Logger.Log($"Removing mac directory: {osx}");
            Directory.Delete(osx, true);
        }

        private void BuildAndCopyResult (string directory) {
            var directories = Directory.GetDirectories(directory, "_*");
            foreach (string dir in directories) { Directory.Delete(dir, true); }

            var solutions = Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories);
            if (solutions.Length == 0) {
                Invalidate("No .sln file found.");
                return;
            }
            if (solutions.Length > 1) {
                Invalidate("Multiple solution files submitted.");
                return;
            }

            string target = solutions[0];
            bool success = Builder.Build(this, target);
            if (!success) {
                Invalidate("Build completed with errors.");
                return;
            }

            var exes = Directory.GetFiles(directory, AutoGrader.Config.Exe ? "*.exe" : "*.dll", SearchOption.AllDirectories);
            if (exes.Length < 1) {
                Invalidate("Produced wrong kind of output on build:" +
                           $"{(AutoGrader.Config.Exe ? ".dll" : ".exe")}");
                return;
            }
            Logger.Log($"Wrote result to {ResultPath}");
            File.Copy(exes[0], ResultPath);
            try {
                Directory.Delete(directory, true);
            }
            catch (IOException) {
                // todo wtf are you doing
            }
        }

        //
        // public API

        public void PrepareSubmissionFiles (int index, int count) {
            if (!Submitted) {
                Invalidate("Nothing submitted.");
                return;
            }

            string dir = Serializer.GetSubmissionDirectory(this);
            string path = Path.ChangeExtension(dir, ".zip");
            ResultPath = Path.ChangeExtension(path, AutoGrader.Config.Exe ? ".exe" : ".dll");

            if (File.Exists(ResultPath)) { return; }
            if (!Directory.Exists(dir)) { DownloadAndUnzip(path, dir); }
            if (Directory.Exists(dir)) { BuildAndCopyResult(dir); } // we may not have succeeded
        }

        public void Invalidate (string reason) {
            if (!Valid) { throw new Exception("double invalidation?"); }

            _feedback = new Feedback { Grade = 0f, InvalidReason = reason };
            Valid = false;
            Logger.Log($"{SubmissionID} invalid: {reason}");
        }

        // todo yikes.gov
        public void GiveFeedback (int correct, int incorrect, string general) {
            float grade = MathF.Truncate(1000f * correct / AutoGrader.Config.TotalTests) / 10f;
            if (grade < 0f || grade > 100.0f) { throw new Exception($"Grade {grade} unreasonable."); }

            int unknown = AutoGrader.Config.TotalTests - (correct + incorrect);
            float finalgrade = (1 - LatePenalty) * grade;

            Logger.Log($"{SubmissionID} grade : {finalgrade}, {correct} correct, {incorrect} incorrect, {unknown} ambiguous.");

            _feedback = new Feedback {
                Grade = finalgrade,
                Correct = correct,
                Incorrect = incorrect,
                Ambigious = unknown,
                GeneralOutput = general,
            };
        }

        public void UploadResults () {
            Logger.Log($"Uploading results for {SubmissionID}");
            try {
                var uri = AutoGrader.Config.GetUploadURL(this, _feedback, out string headername, out string headervalue);
                using (var client = new WebClient()) {
                    client.Headers.Add(headername, headervalue);
                    client.UploadString(uri, "PUT", "");
                }
            }
            catch (Exception) {
                // ignored
                // todo bad bad
            }
        }

        public struct Feedback
        {
            public float Grade;
            public int Correct, Incorrect, Ambigious;
            public string GeneralOutput, InvalidReason;
        }
    }
}
