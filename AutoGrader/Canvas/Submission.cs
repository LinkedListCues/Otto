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

        public void PrepareSubmissionFiles (int index, int count) {
            if (!Submitted) {
                Invalidate("Nothing submitted.");
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

        //
        // public API

        public void Invalidate (string reason) {
            if (!Valid) { throw new Exception("double invalidation?"); }

            _feedback = new Feedback { Grade = 0f, InvalidReason = reason };
            Valid = false;
            Logger.Log($"{SubmissionID} invalid: {reason}");
        }

        // todo yikes.gov
        public void GiveFeedback (float grade, int correct, int incorrect, string general, string error) {
            if (grade < 0f || grade > 100.0f) { throw new Exception($"Grade {grade} unreasonable."); }

            int unknown = Grader.TOTAL_TESTS - (correct + incorrect);
            float finalgrade = (1 - LatePenalty) * grade;

            Logger.Log($"{SubmissionID} grade : {finalgrade}, {correct} correct, {incorrect} incorrect, {unknown} ambiguous.");

            _feedback = new Feedback {
                Grade = finalgrade,
                Correct = correct,
                Incorrect = incorrect,
                Ambigious = unknown,
                GeneralOutput = general,
                ErrorOutput = error
            };
        }

        public void UploadResults () {
            Logger.Log($"Uploading results for {SubmissionID}");

            try {
                string uri = MakeMagicUploadURI();
                using (var client = new WebClient()) {
                    client.Headers.Add("Authorization", "Bearer 1876~nSmP6pGTi0LsIdPe8h19TLVL9zAP5tHTgvfMd08cjLZAdarU0HF5KQSyss8JGcdp");
                    client.UploadString(uri, "PUT", "");
                }
            }
            catch (Exception) {
                // ignored
            }
        }

        private string MakeMagicUploadURI () {
            const string baseuri = "https://canvas.northwestern.edu/api/v1/courses/72859/assignments/460601/submissions/";
            return $"{baseuri}{UserID}?submission[posted_grade]={_feedback.Grade}"
            + $"&comment[text_comment]={ConstructFeedbackString()}";
        }

        private string ConstructFeedbackString () {
            if (!Valid) { return $"Grade: {_feedback.Grade} due to {_feedback.InvalidReason}"; }

            string latepenalty = LatePenalty > 0f ? $"Late penalty: {LatePenalty}\n\n" : "";
            string information =
                $"Grade : {_feedback.Grade}\n{_feedback.Correct} correct\n{_feedback.Incorrect} incorrect\n {_feedback.Ambigious} ambiguous\n";

            string generaloutput = _feedback.GeneralOutput + "\n";
            string erroroutput = _feedback.ErrorOutput + "\n";

            string result = latepenalty + information + generaloutput + erroroutput;
            return result.Trim();
        }

        private struct Feedback
        {
            public float Grade;
            public int Correct, Incorrect, Ambigious;
            public string GeneralOutput, ErrorOutput, InvalidReason;
        }
    }
}
