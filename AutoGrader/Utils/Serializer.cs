using System;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace AutoGrader
{
    public static class Serializer
    {
        private const string BASE_PATH = "214AutoGrader";
        private const string SUBMISSION_PATH = "Submissions";

        private static string GetPathName (string path) {
            string datapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(datapath, BASE_PATH, path);
        }

        //
        // public API

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
    }
}
