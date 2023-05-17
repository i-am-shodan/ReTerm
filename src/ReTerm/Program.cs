using System;
using System.Threading;
using System.Threading.Tasks;
using Common.ANSI.ANSIParser;
using Graphite;
using Graphite.Typography;
using ReMarkable.NET.Unix.Driver;
using ReMarkable.NET.Unix.Driver.Display.EinkController;
using ReMarkable.NET.Unix.Driver.Keyboard;
using ReMarkable.NET.Util;
using Sandbox.Terminal;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace Sandbox
{
    class Program
    {
        private static ITerminalProcess TerminalProcess;
        private static VT100Converter VT100Converter;
        private static PseudoUnixTerminal PseudoTerminal;
        private static TerminalWindow TerminalWindow;

        static async Task Main(string[] args)
        {

            var screen = OutputDevices.Display;
            InputDevices.Keyboard.Released += Keyboard_Released;

            var buildCacheTask = TerminalFont.BuildGlyphCache();

            const string splashText = "ReTerm (alpha)";
            var splashFont = Fonts.SegoeUi.CreateFont(72);
            var strSize = TextMeasurer.Measure(splashText, new RendererOptions(splashFont));

            using (var blankScreen = new Image<Rgb24>(screen.VisibleWidth, screen.VisibleHeight))
            using (var splashImg = new Image<Rgb24>((int)strSize.Width * 2, (int)strSize.Height * 2))
            {
                blankScreen.Mutate(context => context
                    .SetGraphicsOptions(options => options.Antialias = false)
                    .Clear(Color.White));

                splashImg.Mutate(context => context
                    .SetGraphicsOptions(options => options.Antialias = false)
                    .Clear(Color.White));

                splashImg.Mutate(g => g.DrawText(splashText, Fonts.SegoeUi.CreateFont(72), Color.Black, new PointF(0, 0)));
                splashImg.Mutate(x => x.Rotate(90));
                blankScreen.Mutate(x => x.DrawImage(splashImg, new Point(screen.VisibleWidth / 2, (screen.VisibleHeight / 2) - 300), 1));

                OutputDevices.Display.Draw(blankScreen, blankScreen.Bounds(), Point.Empty, waveformMode: WaveformMode.Auto);
            }

            // calculate the width of the terminal in chars based on the size of the font
            var terminalWidthInChars = (int)Math.Floor(screen.VisibleHeight / (double)TerminalFont.GetWidth());
            // calculate the height of the terminal in chars based on the height of the font
            var terminalHeightInChars = (int)Math.Floor(screen.VisibleWidth / (double)TerminalFont.GetHeight());

            Console.WriteLine("Terminal width = " + terminalWidthInChars);
            Console.WriteLine("Terminal height = " + terminalHeightInChars);

            using (var cts = new CancellationTokenSource())
            {
                TerminalWindow = new TerminalWindow(terminalWidthInChars, terminalHeightInChars, () => cts.Cancel());
                PseudoTerminal = new PseudoUnixTerminal(TerminalWindow);
                VT100Converter = new(PseudoTerminal);
                await using (TerminalProcess = (DeviceType.GetDevice() == Device.Emulator) ? new WindowsTerminalProcess(VT100Converter, () => cts.Cancel()) : new LinuxTerminalProcess(VT100Converter, () => cts.Cancel()))
                {
                    await buildCacheTask;

                    await Task.Delay(-1, cts.Token);
                }
            }
        }

        private static void Keyboard_Released(object sender, KeyEventArgs e)
        {
            char key = '\0';

            if (e.Key == KeyboardKey.LeftCtrl || 
                e.Key == KeyboardKey.LeftShift || 
                e.Key == KeyboardKey.RightShift ||
                e.Key == KeyboardKey.LeftAlt ||
                e.Key == KeyboardKey.RightAlt)
            {
                return;
            }

            bool isShiftHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.LeftShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed |
                   InputDevices.Keyboard.KeyStates[KeyboardKey.RightShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

            switch (e.Key)
            {
                case KeyboardKey.Enter:
                    key = '\n';
                    break;
                case KeyboardKey.Backspace:
                    key = '\b';
                    break;
                case KeyboardKey.Space:
                    key = ' ';
                    break;
                case KeyboardKey.Tab:
                    key = '\t';
                    break;
                case KeyboardKey.CapsLock:
                    key = '|';
                    break;
                case KeyboardKey.Grave:
                    key = isShiftHeld ? '"' : '`';
                    break;
                case KeyboardKey.Semicolon:
                    key = isShiftHeld ? ':' : ';';
                    break;
                case KeyboardKey.Apostrophe:
                    key = isShiftHeld ? '@' : '\'';
                    break;
                case KeyboardKey.Comma:
                    key = isShiftHeld ? '<' : ',';
                    break;
                case KeyboardKey.Period:
                    key = isShiftHeld ? '>' : '.';
                    break;
                case KeyboardKey.Slash:
                    key = isShiftHeld ? '?' : '/';
                    break;
                case KeyboardKey.Equal:
                    key = isShiftHeld ? '-' : '=';
                    break;
                case KeyboardKey.NumberRow1:
                case KeyboardKey.NumberRow2:
                case KeyboardKey.NumberRow3:
                case KeyboardKey.NumberRow4:
                case KeyboardKey.NumberRow5:
                case KeyboardKey.NumberRow6:
                case KeyboardKey.NumberRow7:
                case KeyboardKey.NumberRow8:
                case KeyboardKey.NumberRow9:
                case KeyboardKey.NumberRow0:
                    var enumValue = e.Key.ToString();
                    key = enumValue[enumValue.Length - 1];

                    if (isShiftHeld)
                    {
                        switch (key)
                        {
                            case '1':
                                key = '!';
                                break;
                            case '2':
                                key = '"';
                                break;
                            case '3':
                                key = '£';
                                break;
                            case '4':
                                key = '$';
                                break;
                            case '5':
                                key = '%';
                                break;
                            case '6':
                                key = '^';
                                break;
                            case '7':
                                key = '&';
                                break;
                            case '8':
                                key = '*';
                                break;
                            case '9':
                                key = '(';
                                break;
                            case '0':
                                key = ')';
                                break;
                        }
                    }
                    break;
                default:
                    var keyStr = e.Key.ToString();
                    if (isShiftHeld)
                    {
                        keyStr = keyStr.ToUpperInvariant();
                    }
                    else
                    {
                        keyStr = keyStr.ToLowerInvariant();
                    }

                    if (keyStr.Length != 1)
                    {
                        Console.WriteLine("Invalid keyboard mapping: " + keyStr);
                        key = '\0';
                    }
                    key = keyStr[0];
                    break;
            }

            if (InputDevices.Keyboard.KeyStates[KeyboardKey.LeftCtrl] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed)
            {
                //https://en.wikipedia.org/wiki/ASCII#ASCII_control_code_chart
                switch (e.Key)
                {
                    case KeyboardKey.C:
                        TerminalProcess.WriteToStdIn("\x03");
                        break;
                    case KeyboardKey.X:
                        TerminalProcess.WriteToStdIn("\x18");
                        break;
                    case KeyboardKey.G:
                        TerminalProcess.WriteToStdIn("\x07");
                        break;
                    case KeyboardKey.O:
                        TerminalProcess.WriteToStdIn("\x0f");
                        break;
                    case KeyboardKey.W:
                        TerminalProcess.WriteToStdIn("\x17");
                        break;
                    case KeyboardKey.K:
                        TerminalProcess.WriteToStdIn("\x0b");
                        break;
                    case KeyboardKey.J:
                        TerminalProcess.WriteToStdIn("\x0a");
                        break;
                    case KeyboardKey.R:
                        TerminalProcess.WriteToStdIn("\x12");
                        break;
                    case KeyboardKey.Slash:
                        TerminalProcess.WriteToStdIn("\x1c");
                        break;
                    case KeyboardKey.U:
                        TerminalProcess.WriteToStdIn("\x15");
                        break;
                    case KeyboardKey.T:
                        TerminalProcess.WriteToStdIn("\x14");
                        break;
                }
            }
            else if (key != '\0')
            {
                TerminalProcess.WriteToStdIn(key.ToString());
            }
        }

        public static void WindowUpdate(object sender, WindowUpdateEventArgs e)
        {
            _ = Task.Run(() =>
            {
                OutputDevices.Display.Draw(e.Buffer, e.UpdatedArea, e.UpdatedArea.Location, displayTemp: DisplayTemp.RemarkableDraw);
            });      
        }
    }
}
