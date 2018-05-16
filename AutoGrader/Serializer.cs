using System;
using System.IO;
using Newtonsoft.Json;

namespace AutoGrader
{
    public class Serializer : IManager
    {
        private const string BASE_PATH = "214AutoGrader";
        private const string INFORMATION_PATH = "Info";

        public void Initialize() {
            CheckAndMakeDirectory(INFORMATION_PATH);
        }

        private static string GetPathName(string path) {
            string datapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(datapath, BASE_PATH, path);
        }

        private DirectoryInfo CheckAndMakeDirectory(string path) {
            string fullpath = GetPathName(path);
            if (Directory.Exists(fullpath)) { return new DirectoryInfo(fullpath); }
            Logger.Log("Creating directory at: " + fullpath);
            return Directory.CreateDirectory(fullpath);
        }


        public static bool WriteObjectToPath(object paylod, string path) {
            return false;
        }

        public static T ReadObjectFromPath<T>(string path) {
            if (path == null || !File.Exists(path)) {
                throw new FileNotFoundException("No file at path: " + path);
            }

            string text = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<T>(text);
            return result;
        }
    }
}
