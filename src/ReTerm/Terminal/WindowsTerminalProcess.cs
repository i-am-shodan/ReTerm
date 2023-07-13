using ReMarkable.NET.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Terminal
{
    class WindowsTerminalProcess : ITerminalProcess
    {
        private readonly Process process;
        private readonly Task ReadStdError;
        private readonly Task ReadStdOut;
        private readonly Action Quit;
        private readonly CancellationTokenSource cts = new();
        public event Action<byte[]> OnNewData;

        public WindowsTerminalProcess(Action quit) {

            this.Quit = quit;

            process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "wsl.exe",
                    Arguments = "-e /bin/bash --login",
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                }
            };

            if (!process.StartInfo.EnvironmentVariables.ContainsKey("TERM"))
            {
                process.StartInfo.EnvironmentVariables.Add("TERM", "xterm");
            }
            if (!process.StartInfo.EnvironmentVariables.ContainsKey("HOME"))
            {
                process.StartInfo.EnvironmentVariables.Add("HOME", process.StartInfo.WorkingDirectory);
            }
            if (!process.StartInfo.EnvironmentVariables.ContainsKey("USER"))
            {
                process.StartInfo.EnvironmentVariables.Add("USER", Environment.UserName);
            }
            if (!process.StartInfo.EnvironmentVariables.ContainsKey("USER"))
            {
                process.StartInfo.EnvironmentVariables.Add("USER", Environment.UserName);
            }

            process.Start();

            ReadStdError = Task.Run(() => ProcessStream(process.StandardError.BaseStream, cts.Token), cts.Token);
            ReadStdOut = Task.Run(() => ProcessStream(process.StandardOutput.BaseStream, cts.Token), cts.Token);
        }

        internal async Task ProcessStream(Stream s, CancellationToken token = default)
        {
            var tempBuf = new byte[100];

            while (!process.HasExited && !token.IsCancellationRequested)
            {
                var dataLen = await s.ReadAsync(tempBuf, 0, tempBuf.Length, token);

                if (dataLen == 0)
                {
                    continue;
                }

                byte[] buffer = new byte[dataLen];
                Buffer.BlockCopy(tempBuf, 0, buffer, 0, buffer.Length);

                if (DeviceType.GetDevice() == Device.Emulator)
                {
                    // answers on a postcard as to why kepresses seem to come through with line endings
                    if (buffer.Length == 3 && buffer[1] == (byte)' ' && buffer[2] == '\r')
                    {
                        byte key = buffer[0];
                        buffer = new byte[1];
                        buffer[0] = key;
                    }
                }

                OnNewData?.Invoke(buffer);
            }
            Console.WriteLine("[!] The process has exited!\n");
            Quit();
        }

        public void WriteToStdIn(string txt)
        {
            if (process.HasExited || process.StandardInput.BaseStream.CanWrite == false)
            {
                Console.WriteLine("[!] Error writing to base stream!\n");
                Quit();
            }
            else
            {
                var buffer = Encoding.UTF8.GetBytes(txt);

                OnNewData?.Invoke(buffer);

                process.StandardInput.BaseStream.Write(buffer, 0, buffer.Length);
                process.StandardInput.BaseStream.Flush();
            }
        }

        public void SendSIGINT()
        {
            // do nothing
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();
            await ReadStdError;
            await ReadStdOut;
        }
    }
}
