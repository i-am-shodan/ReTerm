using Common.ANSI.ANSIParser;
using libvt100;
using Sandbox.Terminal;

namespace UnitTests
{
    [TestClass]
    public class VT100Handling
    {
        [TestMethod]
        public void TestVT100Handling()
        {
            var ansi = new byte[] { 27, 91, 49, 48, 59, 49, 48, 72, 104, 101, 108, 108, 111, 13, 10, 27, 91, 48, 48, 59, 51, 55, 109, 114, 101, 77, 97, 114, 107, 97, 98, 108, 101, 27, 91, 48, 49, 59, 51, 49, 109, 58, 32, 27, 91, 48, 49, 59, 51, 55, 109, 126, 47, 27, 91, 48, 48, 109, 32 };

            IAnsiDecoder vt100 = new VT100Decoder();
            vt100.Input(ansi);
        }
    }
}