using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoGraderTests
{

    [TestClass]
    public class SerializerTests
    {
        [TestMethod]
        public void SerializeNullObject () { }

        [TestMethod]
        public void SerializeToInvalidPath () { }

        [TestMethod]
        public void DeserializeOutInvalidPath () { }

        [TestMethod]
        public void DeserializeOutBadFormat () { }

        [TestMethod]
        public void SerializeValid () { }

        [TestMethod]
        public void ReadObjectFromPath () { }


    }
}
