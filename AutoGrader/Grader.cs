using AutoGrader.Canvas;

namespace AutoGrader
{
    public class Grader
    {
        public Grader () {

        }

        public void Grade (Submission submission, int index, int count) {
            Logger.Log($"Grading {submission.SubmissionID} ({index} of {count})");
        }
    }
}
