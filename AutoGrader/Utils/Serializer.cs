using System;
using System.Data;
using System.IO;
using System.Linq;
using AutoGrader.Canvas;
using Newtonsoft.Json;

namespace AutoGrader
{
    public static class Serializer
    {
        private const string BASE_PATH = "214AutoGrader";
        private const string SUBMISSION_PATH = "Submissions";
        private const string TESTBED_PATH = "Testbed";

        private static string GetPathName (params string[] paths) {
            string datapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string[] args = new[] { datapath, BASE_PATH }.Concat(paths).ToArray(); // todo this seems nasty
            return Path.Combine(args);
        }

        //
        // public API

        public static void InitializeDirectories () {
            Directory.CreateDirectory(GetPathName(SUBMISSION_PATH));
            Directory.CreateDirectory(GetPathName(TESTBED_PATH));
        }

        public static string GetAPIKey () {
            string path = GetPathName(".secret");
            return File.ReadAllText(path);
        }

        public static bool FileExists (string path) { return File.Exists(GetPathName(path)); }

        public static void WriteObjectToPath (object payload, string path) {
            string serialized = JsonConvert.SerializeObject(payload);
            string fullpath = GetPathName(path);
            File.WriteAllText(fullpath, serialized);
        }

        public static T ReadObjectFromPath<T> (string path) {
            if (path == null) { throw new NoNullAllowedException("Passed in null for path."); }
            if (!FileExists(path)) { throw new FileNotFoundException("Invalid path name: " + path); }

            string fullpath = GetPathName(path);
            string text = File.ReadAllText(fullpath);

            var result = JsonConvert.DeserializeObject<T>(text);
            return result;
        }

        public static string GetSubmissionFileName (Submission submission) {
            return GetPathName(SUBMISSION_PATH, Path.ChangeExtension(submission.SubmissionID, ".zip"));
        }
    }
}
