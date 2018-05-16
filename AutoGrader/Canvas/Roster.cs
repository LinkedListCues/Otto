using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace AutoGrader
{
    public class Roster : IManager
    {
        private const string INFO_PATH = "submissions.json";

        private string _tempurl =
            @"https://canvas.northwestern.edu/api/v1/courses/72859/assignments/458956/submissions?access_token=1876~nSmP6pGTi0LsIdPe8h19TLVL9zAP5tHTgvfMd08cjLZAdarU0HF5KQSyss8JGcdp&per_page=99&page=1";

        public List<Submission> Submissions { get; private set; }

        public void Initialize () {
            PrepareRoster();
        }

        public void PrepareRoster () {
            if (Serializer.FileExists(INFO_PATH)) { LoadRoster(); }
            else { FetchAndSaveRoster(); }
        }

        private void LoadRoster () {
            Logger.Log("Loading serialized roster...");
            Submissions = Serializer.ReadObjectFromPath<List<Submission>>(INFO_PATH);
            foreach (var submission in Submissions) {
                Logger.Log(submission.SubmissionID);
            }
        }

        // todo make sure to get all 200 (only doing 99 atm)
        private void FetchAndSaveRoster () {
            Logger.Log("Downloading all submissions...");
            Submissions = new List<Submission>();
            var json = LoadJsonArrayFromURL(_tempurl);
            foreach (var child in json.Children<JObject>()) {
                Submissions.Add(new Submission(child));
            }
            Logger.Log("Serializing submissions...");
            Serializer.WriteObjectToPath(Submissions, INFO_PATH);
        }

        // todo error catching
        private static JArray LoadJsonArrayFromURL (string url) {
            using (var webClient = new WebClient()) {
                string json = webClient.DownloadString(url);
                return JArray.Parse(json);
            }
        }

        // todo error catching
        private static JObject LoadJsonFromURL (string url) {
            using (var webClient = new WebClient()) {
                string json = webClient.DownloadString(url);
                return JObject.Parse(json);
            }
        }
    }
}
