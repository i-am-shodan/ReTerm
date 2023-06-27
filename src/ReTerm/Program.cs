using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReMarkable.NET.Unix.Driver;
using ReMarkable.NET.Unix.Driver.Display.EinkController;
using ReMarkable.NET.Unix.Driver.Keyboard;
using ReMarkable.NET.Util;
using ReTerm.Fonts;
using ReTerm.Settings;
using Sandbox.Terminal;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VtNetCore.VirtualTerminal;
using VtNetCore.VirtualTerminal.Enums;
using VtNetCore.XTermParser;
using PointF = SixLabors.ImageSharp.PointF;

namespace Sandbox
{
    class Program
    {
        private static ITerminalProcess TerminalProcess;
        private static TerminalWindow TerminalWindow;
        private static VirtualTerminalController TerminalController = new VirtualTerminalController();
        private static DataConsumer VT100DataConsumer = new DataConsumer(TerminalController);

        static async Task Main(string[] args)
        {
            var screen = OutputDevices.Display;
            if (InputDevices.Keyboard != null)
            {
                InputDevices.Keyboard.Released += Keyboard_Released;
            }

            var buildCacheTask = TerminalFont.BuildGlyphCache();

            string splashText = "ReTerm";

            if (InputDevices.Keyboard == null)
            {
                splashText += " - no kdb detected";
                Console.WriteLine("Error: no keyboard attached");
            }

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

            var settings = TerminalSettings.Get();

            // calculate the width of the terminal in chars based on the size of the font
            var terminalWidthInChars = (int)Math.Floor(screen.VisibleHeight / (double)TerminalFont.GetWidth());
            // calculate the height of the terminal in chars based on the height of the font
            var terminalHeightInChars = (int)Math.Floor(screen.VisibleWidth / (double)TerminalFont.GetHeight());

            Console.WriteLine("Terminal width = " + terminalWidthInChars);
            Console.WriteLine("Terminal height = " + terminalHeightInChars);

            using (var cts = new CancellationTokenSource())
            {
                TerminalWindow = new TerminalWindow(terminalWidthInChars, terminalHeightInChars, () => cts.Cancel());
                SetTerminalDefaults(terminalWidthInChars, terminalHeightInChars);

                //await using (TerminalProcess = new ReplayTerminalProcess(() => cts.Cancel()))
                await using (TerminalProcess = (DeviceType.GetDevice() == Device.Emulator) ? new WindowsTerminalProcess(() => cts.Cancel()) : new LinuxTerminalProcess(() => cts.Cancel()))
                {
                    ConcurrentDictionary<Tuple<DateTime, int, int>, byte> marks = new();

                    int lastStylusXPos = 0;
                    int lastStylusYPos = 0;

                    Action<Point> HandleStylusFingerPosition = (point) =>
                    {
                        if (settings.DisableMouseAndTouch)
                        {
                            return;
                        }

                        // We need to convert state.DevicePosition.x and state.DevicePosition.y to the which character
                        // and row the stylus is over. We can then use that to set the cursor position.
                        int x = (screen.VisibleWidth - point.X) / TerminalFont.GetHeight();
                        int y = point.Y / TerminalFont.GetWidth();

                        bool isShiftHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.LeftShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed | InputDevices.Keyboard.KeyStates[KeyboardKey.RightShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;
                        bool isCtrlHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.LeftCtrl] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed | InputDevices.Keyboard.KeyStates[KeyboardKey.RightCtrl] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

                        if (x > 0 && y > 0 && x < terminalHeightInChars && y < terminalWidthInChars)
                        {
                            if (lastStylusXPos != x || lastStylusYPos != y)
                            {
                                TerminalController.MouseMove(x, y, 3, isCtrlHeld, isShiftHeld);
                                TerminalWindow.SetCursor(x, y, '*');
                                marks.TryAdd(new Tuple<DateTime, int, int>(DateTime.Now, x, y), 0);
                                lastStylusXPos = x;
                                lastStylusYPos = y;
                            }
                        }
                    };

                    InputDevices.Touchscreen.Moved += (sender, finger) =>
                    {
                        HandleStylusFingerPosition(finger.DevicePosition);
                    };

                    InputDevices.Digitizer.StylusUpdate += (sender, state) =>
                    {
                        if (state.Pressure > 0)
                        {
                            HandleStylusFingerPosition(new Point((int)state.DevicePosition.X, (int)state.DevicePosition.Y));
                        }
                    };

                    bool alreadyResetting = false;
                    TerminalController.OnScreenBufferChanged += () =>
                    {
                        // resetting the terminal will cause a screen buffer change, so we need to ignore that
                        // We reset once the screen buffer has changed to ensure that the terminal is in a known state
                        // otherwise it seems to get a bit confused.

                        if (alreadyResetting) return;
                        alreadyResetting = true;
              
                        TerminalWindow.Clear();

                        if (TerminalController.IsActiveBufferNormal)
                        {
                            TerminalController.FullReset();
                        }
                        SetTerminalDefaults(terminalWidthInChars, terminalHeightInChars);

                        alreadyResetting = false;
                    };

                    TerminalController.OnCharacterChanged += (row, col, attrib, c) => {
                        if (row >= terminalHeightInChars && !TerminalController.InEraseState)
                        {
                            TerminalController.SetAbsoluteRow(0);
                            TerminalWindow.AddPageAndSetAsCurrent();
                            row = 0;
                        }

                        if (c == '\0')
                        {
                            c = ' ';
                        }

                        TerminalWindow.ScreenUpdate(
                            row, 
                            col, 
                            c.ToString(),
                            ConvertColor(attrib.ForegroundColor), ConvertColor(attrib.BackgroundColor));
                    };

                    TerminalProcess.OnNewData += (data) => {
                        VT100DataConsumer.Push(data);
                    };

                    // Turn on cursor blinking if configured
                    var cursorAndMarksActivity = Task.Run(async () =>
                    {
                        while (!cts.IsCancellationRequested)
                        {
                            var cursor = TerminalController.CursorState;
                            int currentRow = cursor.CurrentRow;
                            int currentCol = cursor.CurrentColumn;

                            foreach (var markToRemove in marks.Where(x => DateTime.Now.Subtract(x.Key.Item1) > settings.TouchMarksClearTime).ToArray())
                            {
                                TerminalWindow.UnsetCursor(markToRemove.Key.Item2, markToRemove.Key.Item3);
                                marks.TryRemove(markToRemove);
                            }

                            if (cursor.BlinkingCursor)
                            {
                                char cursorChar = '_';

                                switch (cursor.CursorShape)
                                {
                                    case ECursorShape.Underline:
                                        cursorChar = '_';
                                        break;
                                    case ECursorShape.Bar:
                                        cursorChar = '|';
                                        break;
                                    case ECursorShape.Block:
                                        cursorChar = '|';
                                        break;
                                    default:
                                        throw new Exception("Unknown cursor");
                                }

                                // if don't blink is set we update the cursor position once 250ms after an update has occured
                                if (!settings.DontBlinkCursor)
                                {
                                    TerminalWindow.SetCursor(currentRow, currentCol, cursorChar);
                                    await Task.Delay(settings.CursorBlinkInterval);
                                    TerminalWindow.UnsetCursor(currentRow, currentCol);
                                }
                            }
                            await Task.Delay(500, cts.Token);
                        }
                    }, cts.Token);

                    await buildCacheTask;

                    await Task.Delay(-1, cts.Token);
                }
            }
        }

        private static void SetTerminalDefaults(int terminalWidthInChars, int terminalHeightInChars)
        {
            TerminalController.MaximumHistoryLines = terminalHeightInChars;
            TerminalController.ResizeView(terminalWidthInChars, terminalHeightInChars + 1);
            TerminalController.CursorState.ConfiguredColumns = terminalWidthInChars;
            TerminalController.SetAutomaticNewLine(true);
            TerminalController.SetVt52Mode(true);
            TerminalController.EnableUrxvtMouseMode(true);
            TerminalController.EnableSgrMouseMode(true);
            TerminalController.SetRgbBackgroundColor(255, 255, 255);
            TerminalController.SetRgbForegroundColor(0, 0, 0);
        }

        private static Rgb24 ConvertColor(ETerminalColor color)
        {
            switch (color)
            {
                case ETerminalColor.Black:
                    return TerminalFont.Black;
                case ETerminalColor.White:
                    return TerminalFont.White;
                case ETerminalColor.Red:
                    return new Rgb24(255, 0, 0);
                case ETerminalColor.Green:
                    return new Rgb24(0, 255, 0);
                case ETerminalColor.Blue:
                    return new Rgb24(0, 0, 255);
                case ETerminalColor.Magenta:
                    return new Rgb24(255, 0, 255);
                case ETerminalColor.Yellow:
                    return new Rgb24(255, 255, 0);
                case ETerminalColor.Cyan:
                    return new Rgb24(0, 255, 255);
                default:
                    throw new Exception("Unknown color");
            }
        }

        private static void NewDataFromVirtualTerminal(object sender, SendDataEventArgs e)
        {

        }

        private static void Keyboard_Released(object sender, KeyEventArgs e)
        {
            char key = '\0';

            if (e.Key == KeyboardKey.LeftCtrl || 
                e.Key == KeyboardKey.LeftShift || 
                e.Key == KeyboardKey.RightShift ||
                e.Key == KeyboardKey.LeftAlt ||
                e.Key == KeyboardKey.RightAlt || 
                e.Key == KeyboardKey.End) // opt
            {
                return;
            }

            bool isShiftHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.LeftShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed |
                   InputDevices.Keyboard.KeyStates[KeyboardKey.RightShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

            bool isOptHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.End] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

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
                    // shift + tab = escape
                    key = isShiftHeld ? '\x1b' : '\t';
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
                    key = isShiftHeld ? '?' : isOptHeld ? '\\' : '/';
                    break;
                case KeyboardKey.Equal:
                    key = isShiftHeld ? '_' : isOptHeld ? '=' : '-';
                    break;
                case KeyboardKey.Down:
                    key = '\0';
                    TerminalProcess.WriteToStdIn("\x1B\x5B\x42");
                    break;
                case KeyboardKey.Up:
                    key = '\0';
                    TerminalProcess.WriteToStdIn("\x1B\x5B\x41");
                    break;
                case KeyboardKey.Left:
                    key = '\0';
                    TerminalProcess.WriteToStdIn("\x1B\x5B\x44");
                    break;
                case KeyboardKey.Right:
                    key = '\0';
                    TerminalProcess.WriteToStdIn("\x1B\x5B\x43");
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

                    if (isOptHeld && key == '3')
                    {
                        key = '#';
                    }
                    else if (isOptHeld && key == '0')
                    {
                        key = '+';
                    }
                    else if (isShiftHeld)
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
    }
}
