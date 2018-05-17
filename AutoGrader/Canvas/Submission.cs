using System;
using System.IO;
using System.IO.Compression;
using System.Net;
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

        public string ResultPath { get; private set; }

        private bool _valid = true;

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

        public void PrepareSubmissionFiles (int index, int count) {
            if (!Submitted) {
                Invalidate("Nothing submitted");
                return;
            }

            string dir = Serializer.GetSubmissionDirectory(this);
            string path = Path.ChangeExtension(dir, ".zip");
            ResultPath = Path.ChangeExtension(path, ".exe"); //todo parameter

            Logger.Log($"{SubmissionID} \t({index} of {count})");
            if (File.Exists(ResultPath)) { return; }
            if (!Directory.Exists(dir)) { DownloadAndUnzip(path, dir); }
            if (Directory.Exists(dir)) { BuildAndCopyResult(dir); } // we may not have succeeded
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
            catch (InvalidDataException) { // the .zip is invalid, presumably
                Invalidate("Submission (.zip file) corrupted.");
            }
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
            bool success = AutoGrader.Builder.Build(this, target);
            if (!success) {
                Invalidate("Build completed with errors.");
                return;
            }

            var exes = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories);
            Logger.Log($"Wrote exe to {ResultPath}");
            File.Copy(exes[0], ResultPath);
            try {
                Directory.Delete(directory, true);
            }
            catch (IOException) { }
        }


        private void Invalidate (string reason) {
            if (_valid) {

            }
            _valid = false;
            Logger.Log($"{SubmissionID} invalid: {reason}");
        }
    }
}
