using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Sandbox.Terminal
{
    internal class LinuxTerminalProcess : ITerminalProcess
    {
        private readonly Task ReadStdOut;
        private readonly Action Quit;
        private readonly int masterFileDescription;
        private readonly int pid;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        #region PInvoke libc
#nullable enable
        [DllImport("libc.so.6")]
        private static extern int close(int fd);

        [DllImport("libc.so.6")]
        private static extern int read(int fd, byte[] buf, int count);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern int write(int fd, byte[] buf, int count);

        [DllImport("libc.so.6")]
        private static extern int errno();

        [DllImport("libutil.so.1", SetLastError = true)]
        internal static extern int forkpty(ref int master, StringBuilder? name, IntPtr termp, IntPtr winsize);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern int execvp(
            [MarshalAs(UnmanagedType.LPStr)] string file,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string?[] args);

        [DllImport("libc.so.6")]
        private static extern int kill(int pid, int sig);

        [DllImport("libc.so.6")]
        private static extern int fcntl(int fd, int cmd, int arg);

        [DllImport("libc.so.6")]
        private static extern int setenv(string name, string value, int overwrite);

#nullable disable
        const int F_SETFL = 4;
        const int O_NONBLOCK = 00004000;
        const int SIGINT = 2;
        #endregion

        public LinuxTerminalProcess(VT100Converter converter, Action quit)
        {
            Quit = quit;

            // Create a new process
            pid = forkpty(ref masterFileDescription, null, IntPtr.Zero, IntPtr.Zero);

            if (pid == 0)
            {
                // Child process
                setenv("TERM", "vt100", 1);

                if (execvp("/bin/bash", new string[] { "/bin/bash", null, "--login" }) == -1)
                {
                    throw new Exception("Error executing bash: " + errno());
                }
                throw new Exception("This should not fail!");
            }
            else if (pid > 0)
            {
                // Parent process
                Console.WriteLine("PID = " + pid);

                // Set the master file description to non-blocking
                fcntl(masterFileDescription, F_SETFL, O_NONBLOCK);

                // kick off a task to read from the shell
                ReadStdOut = Task.Run(async () => {
                    // Read from the child process's stdout
                    while (!cts.IsCancellationRequested)
                    {
                        byte[] rawBuffer = new byte[64];
                        int dataLen = read(masterFileDescription, rawBuffer, rawBuffer.Length);

                        if (dataLen <= 0)
                        {
                            await Task.Delay(100);
                            continue;
                        }

                        byte[] buffer = new byte[dataLen];
                        Buffer.BlockCopy(rawBuffer, 0, buffer, 0, buffer.Length);

                        converter.Process(buffer);
                    }
                    Quit();
                }, cts.Token);
            }
            else
            {
                // Error
                throw new Exception("Error calling forkpty");
            }
        }

        public void WriteToStdIn(string txt)
        {
            // Encoding.UTF8.GetBytes(txt); is not availiable
            byte[] buffer = new byte[txt.Length];
            for (int x = 0; x < txt.Length; x++)
            {
                buffer[x] = (byte)txt[x];
            }

            if (write(masterFileDescription, buffer, buffer.Length) != buffer.Length)
            {
                Console.WriteLine("Error writing to stdin: " + errno());
                return;
            }
        }

        public void SendSIGINT()
        {
            Console.WriteLine("Sending SIGINT to " + pid);
            kill(pid, SIGINT);
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();
            await ReadStdOut;
        }
    }
}
