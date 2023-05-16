using System;

namespace Sandbox.Terminal
{
    interface ITerminalProcess : IAsyncDisposable
    {
        void WriteToStdIn(string txt);
        void SendSIGINT();
    }
}
