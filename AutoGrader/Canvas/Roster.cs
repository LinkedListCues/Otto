using System.Collections.Generic;
using AutoGrader.Utils;
using Newtonsoft.Json.Linq;

namespace AutoGrader.Canvas
{
    public class Roster
    {
        private const string INFO_PATH = "submissions.json";

        public List<Submission> Submissions { get; private set; }

        public Roster () {
            PrepareRoster();
        }

        public void PrepareRoster () {
            if (Serializer.FileExists(INFO_PATH)) { LoadRoster(); }
            else { FetchAndSaveRoster(); }
        }

        private void LoadRoster () {
            Logger.Log("Loading serialized roster...");
            Submissions = Serializer.ReadObjectFromPath<List<Submission>>(INFO_PATH);
        }

        private void FetchAndSaveRoster () {
            Logger.Log("Downloading all submissions...");
            Submissions = new List<Submission>();

            for (int i = 1; i < 99; i++) { // TODO this is a magic constant fuck
                var json = AutoGrader.Fetcher.FetchSubmissions(i, out bool full);
                foreach (var child in json.Children<JObject>()) { Submissions.Add(new Submission(child)); }
                if (!full) { break; }
            }

            Logger.Log("Serializing submissions...");
            Serializer.WriteObjectToPath(Submissions, INFO_PATH);
        }
    }
}
