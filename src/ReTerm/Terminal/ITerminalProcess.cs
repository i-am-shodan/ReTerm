using System;

namespace Sandbox.Terminal
{
    interface ITerminalProcess : IAsyncDisposable
    {
        event Action<byte[]> OnNewData;

        void WriteToStdIn(string txt);
        void SendSIGINT();
    }
}
