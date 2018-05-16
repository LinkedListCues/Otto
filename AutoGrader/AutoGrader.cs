namespace AutoGrader
{
    public class AutoGrader : IManager
    {
        private Roster _roster;

        public void Initialize () {
            Config.Initialize();

            _roster = new Roster();
            _roster.Initialize();
        }
    }

    public static class Config
    {
        public static CanvasJsonFetcher Fetcher;

        public static void Initialize () {
            Fetcher = new CanvasJsonFetcher();
        }
    }
}
