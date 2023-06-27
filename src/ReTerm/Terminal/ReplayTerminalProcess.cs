using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Terminal
{
    class ReplayTerminalProcess : ITerminalProcess
    {
        private readonly Action Quit;
        private readonly CancellationTokenSource cts = new();
        public event Action<byte[]> OnNewData;

        public ReplayTerminalProcess(Action quit) {

            this.Quit = quit;

            var filename = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output.dat");

            var t = Task.Run(() => ProcessStream(filename, cts.Token), cts.Token);
        }

        internal async Task ProcessStream(string filename, CancellationToken token = default)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fs))
            {
                var line = await sr.ReadLineAsync();

                var rawBytes = Convert.FromBase64String(line);

                OnNewData?.Invoke(rawBytes);

                await Task.Delay(1000);
            }
        }

        public void WriteToStdIn(string txt)
        {
            // do nothing
        }

        public void SendSIGINT()
        {
            // do nothing
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async ValueTask DisposeAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            cts.Cancel();
        }
    }
}
