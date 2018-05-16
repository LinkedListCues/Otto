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
                Logger.Log($"No submission for {SubmissionID}");
                return;
            }

            string path = Serializer.GetSubmissionFileName(this);
            string dir = Path.ChangeExtension(path, "");

            Logger.Log($"{SubmissionID} \t({index} of {count})");
            if (Directory.Exists(dir)) { return; }
            DownloadAndUnzip(path, dir);
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
                Logger.Log($"Invalid .zip for {SubmissionID}");
            }
        }
    }
}
