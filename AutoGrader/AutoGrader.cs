namespace AutoGrader
{
    public class AutoGrader : IManager
    {
        private Serializer _serializer;
        private Roster _roster;

        public void Initialize() {
            _serializer = new Serializer();
            _serializer.Initialize();

            _roster = new Roster();
            _roster.Initialize();
        }
    }
}
