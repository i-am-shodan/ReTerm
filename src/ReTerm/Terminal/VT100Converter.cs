using Common.ANSI.ANSIParser;
using libvt100;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]

namespace Sandbox.Terminal
{
    class VT100Converter
    {
        private readonly IAnsiDecoder vt100 = new VT100Decoder();
        private readonly PseudoUnixTerminal decoder;

        public VT100Converter(PseudoUnixTerminal term)
        {
            this.decoder = term;
            vt100.Subscribe(decoder);
        }

        public void Process(byte[] data)
        {
            vt100.Input(data);
        }
    }
}
