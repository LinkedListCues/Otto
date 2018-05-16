using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using AutoGrader.Canvas;

namespace AutoGrader
{
    public class Roster : IManager
    {
        private const string INFO_PATH = "submissions.json";

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
        }

        // todo make sure to get all 200 (only doing 99 atm)
        private void FetchAndSaveRoster () {
            Logger.Log("Downloading all submissions...");
            Submissions = new List<Submission>();

            JArray json;
            for (int i = 1; i < 99; i++) { // TODO this is a magic constant and needs to be adjusted
                json = Config.Fetcher.FetchSubmissions(i);

                foreach (var child in json.Children<JObject>()) {
                    Submissions.Add(new Submission(child));
                }

                // we're getting 99 submissions per page, so if we have fewer, it's time to bail
                if (json.Count < 99) { break; } // todo this is a slightly different magic constant
                
            }


            Logger.Log("Serializing submissions...");
            Serializer.WriteObjectToPath(Submissions, INFO_PATH);
        }
    }
}
