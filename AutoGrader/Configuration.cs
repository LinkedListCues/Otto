using System;
using System.IO;
using AutoGrader.Canvas;
using Newtonsoft.Json.Linq;

namespace AutoGrader
{
    public class Configuration
    {
        public const int PER_PAGE = 99;
        private const string AUTHORIZATION = "Authorization", BEARER = "Bearer";

        public readonly string APIKey, BaseURL;

        public readonly string MSBuildPath, VSTestPath;

        public readonly string CourseID, AssignmentID, TestExecutablePath, TestbedAdapterPath, TestbedDLLPath;
        public readonly bool Exe;

        public readonly int TotalTests;

        public Configuration (string path) {
            if (!File.Exists(path)) { throw new FileNotFoundException($"Missing {path}."); }

            string text = File.ReadAllText(path);
            var json = JObject.Parse(text);

            APIKey = LoadAPIKey((string)json["secret_file"]);
            BaseURL = (string)json["base_url"];

            CourseID = (string)json["course_id"];
            AssignmentID = (string)json["assignment_id"];


            string adapter = (string)json["testbed_adapter_path"];
            if (adapter.Length > 0) { TestbedAdapterPath = adapter; }
            TestExecutablePath = (string)json["testbed_exe_path"];
            TestbedDLLPath = (string)json["testbed_dll_path"];
            Exe = (bool)json["exe"];

            MSBuildPath = (string)json["msbuild_path"];
            VSTestPath = (string)json["vstest_path"];

            TotalTests = (int)json["total_tests"];

        }

        private static string LoadAPIKey (string path) {
            if (!File.Exists(path)) { throw new FileNotFoundException($"Missing {path}."); }

            var lines = File.ReadAllLines(path);
            if (lines.Length != 1) { throw new Exception("Invalid file contents."); }

            return lines[0];
        }

        //
        // public API

        // todo hide the hard-coding
        public Uri GetSubmissionsURL () {
            return new Uri($"{BaseURL}/{CourseID}/assignments/{AssignmentID}/submissions?per_page={PER_PAGE}");
        }

        //private string MakeMagicUploadURI () {
        //    // todo PARAM jesus
        //    const string baseuri = "https://canvas.northwestern.edu/api/v1/courses/72859/assignments/463053/submissions/";
        //    return $"{baseuri}{UserID}?submission[posted_grade]={_feedback.Grade}"
        //           + $"&comment[text_comment]={ConstructFeedbackString()}";
        //}

        public Uri GetUploadURL (Submission sub, Submission.Feedback feedback, out string headername, out string headervalue) {
            headername = $"{AUTHORIZATION}";
            headervalue = $"{BEARER} {APIKey}";

            string url = $"{BaseURL}/{CourseID}/assignments/{AssignmentID}/submissions/{sub.UserID}"
                         + $"?submission[posted_grade]={feedback.Grade}"
            + $"&comment[text_comment]={ConstructFeedbackString(sub, feedback)}";
            return new Uri(url);
        }

        private static string ConstructFeedbackString (Submission sub, Submission.Feedback feedback) {
            if (!sub.Valid) { return $"Grade: {feedback.Grade} due to {feedback.InvalidReason}"; }

            string latepenalty = sub.LatePenalty > 0f ? $"Late penalty: {sub.LatePenalty}\n\n" : "";
            string information =
                $"Grade : {feedback.Grade}\n" +
                $"{feedback.Correct} correct\n" +
                $"{feedback.Incorrect} incorrect\n" +
                $"{feedback.Ambigious} ambiguous\n";

            string generaloutput = feedback.GeneralOutput + "\n";

            string result = latepenalty + information + generaloutput;
            return result.Trim();
        }

    }
}
