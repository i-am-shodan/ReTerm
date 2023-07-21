using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReMarkable.NET.Unix.Driver;
using ReMarkable.NET.Unix.Driver.Display.EinkController;
using ReMarkable.NET.Unix.Driver.Keyboard;
using ReMarkable.NET.Util;
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
using static ReTerm.Settings.TerminalSettings;
using PointF = SixLabors.ImageSharp.PointF;

namespace ReTerm
{
    class Program
    {
        private static ITerminalProcess TerminalProcess;
        private static TerminalWindow TerminalWindow;
        private static VirtualTerminalController TerminalController = new VirtualTerminalController();
        private static DataConsumer VT100DataConsumer = new DataConsumer(TerminalController);
        private static TerminalSettings Settings;

        private const string ReTerm = "ReTerm";

        static async Task Main(string[] args)
        {
            var screen = OutputDevices.Display;

            string splashMessage = ReTerm;
            try
            {
                Settings = await Get();
                Fonts.FontHelper.Init(Settings.FontPath, Settings.FontName);
                TerminalFont.Init(Settings.FontSize);
                InputDevices.Keyboard.Pressed += Keyboard_Released;
            }
            catch (Exception ex) {

                var errorLocation = Settings == null ? "Could not load settings" :
                            Fonts.FontHelper.Default == null ? "Could not load fonts" :
                            InputDevices.Keyboard == null ? "No keyboard attached" : "Could not use fonts";

                splashMessage += "\nError - " + errorLocation + "\n" + ex.GetType().Name + "\n " + ex.Message;
            }

            var buildCacheTask = TerminalFont.BuildGlyphCache();

            var splashFont = Fonts.FontHelper.SegoeUi.CreateFont(72);
            var strSize = TextMeasurer.Measure(splashMessage, new RendererOptions(splashFont));

            using (var blankScreen = new Image<Rgb24>(screen.VisibleWidth, screen.VisibleHeight))
            using (var splashImg = new Image<Rgb24>((int)strSize.Width + 20, (int)strSize.Height + 20))
            {
                blankScreen.Mutate(context => context
                    .SetGraphicsOptions(options => options.Antialias = false)
                    .Clear(Color.White));

                splashImg.Mutate(context => context
                    .SetGraphicsOptions(options => options.Antialias = false)
                    .Clear(Color.White));

                splashImg.Mutate(g => g.DrawText(splashMessage, Fonts.FontHelper.SegoeUi.CreateFont(72), Color.Black, new PointF(0, 0)));
                splashImg.Mutate(x => x.Rotate(90));

                var landscapeScreenWidth = screen.VisibleHeight;
                var landscapeScreenHeight = screen.VisibleWidth;

                var splashWidth = splashImg.Height;
                var splashHeight = splashImg.Width;

                var splashX = (landscapeScreenWidth / 2) - (splashWidth / 2);
                var splashY = (landscapeScreenHeight / 2) - (splashHeight / 2);

                blankScreen.Mutate(x => x.DrawImage(splashImg, new Point(splashY, splashX), 1));

                OutputDevices.Display.Draw(blankScreen, blankScreen.Bounds(), Point.Empty, waveformMode: WaveformMode.Auto);
            }

            if (splashMessage != ReTerm)
            {
                // to get here we added some messages to the splash messages
                // we should stop here as we can't proceed
                return;
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
                SetTerminalDefaults(terminalWidthInChars, terminalHeightInChars);

                //await using (TerminalProcess = new ReplayTerminalProcess(() => cts.Cancel()))
                await using (TerminalProcess = (DeviceType.GetDevice() == Device.Emulator) ? new WindowsTerminalProcess(() => cts.Cancel()) : new LinuxTerminalProcess(() => cts.Cancel()))
                {
                    ConcurrentDictionary<Tuple<DateTime, int, int>, byte> marks = new();

                    int lastStylusXPos = 0;
                    int lastStylusYPos = 0;

                    Action<Point> HandleStylusFingerPosition = (point) =>
                    {
                        if (Settings.DisableMouseAndTouch)
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
                            GetColor(Settings.ForceForegroundHexColor, attrib.ForegroundColor), 
                            GetColor(Settings.ForceBackgroundHexColor, attrib.BackgroundColor));
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

                            foreach (var markToRemove in marks.Where(x => DateTime.Now.Subtract(x.Key.Item1) > Settings.TouchMarksClearTime).ToArray())
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
                                if (!Settings.DontBlinkCursor)
                                {
                                    TerminalWindow.SetCursor(currentRow, currentCol, cursorChar);
                                    await Task.Delay(Settings.CursorBlinkInterval);
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

        private static Rgb24 GetColor(string hexValue, ETerminalColor color)
        {
            if (string.IsNullOrWhiteSpace(hexValue))
            {
                return ConvertColor(color);
            }
            else
            {
                return Color.ParseHex(hexValue);
            }
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

        private static void Keyboard_Released(object sender, KeyEventArgs e)
        {
            if (e.Key == KeyboardKey.RightAlt)
            {
                Console.WriteLine("Forcing screen refresh");
                TerminalWindow.ForceRefresh();
            }

            if (e.Key == KeyboardKey.LeftCtrl || 
                e.Key == KeyboardKey.LeftShift || 
                e.Key == KeyboardKey.RightShift ||
                e.Key == KeyboardKey.LeftAlt ||
                e.Key == KeyboardKey.RightAlt || 
                e.Key == KeyboardKey.End) // opt
            {
                return;
            }

            bool isCtrlHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.LeftCtrl] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed |
                   InputDevices.Keyboard.KeyStates[KeyboardKey.RightCtrl] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

            bool isShiftHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.LeftShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed |
                   InputDevices.Keyboard.KeyStates[KeyboardKey.RightShift] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

            bool isOptHeld = InputDevices.Keyboard.KeyStates[KeyboardKey.End] == ReMarkable.NET.Unix.Driver.Generic.ButtonState.Pressed;

            if (Settings.KeyboardKeyLookup.ContainsKey(e.Key))
            {
                List<char> charsToType = isCtrlHeld && Settings.KeyboardKeyLookup[e.Key].ContainsKey(MetaKey.Ctrl) ? Settings.KeyboardKeyLookup[e.Key][MetaKey.Ctrl] :
                                         isOptHeld && Settings.KeyboardKeyLookup[e.Key].ContainsKey(MetaKey.Opt) ? Settings.KeyboardKeyLookup[e.Key][MetaKey.Opt] :
                                         isShiftHeld && Settings.KeyboardKeyLookup[e.Key].ContainsKey(MetaKey.Shift) ? Settings.KeyboardKeyLookup[e.Key][MetaKey.Shift] :
                                         Settings.KeyboardKeyLookup[e.Key].ContainsKey(MetaKey.None) ? Settings.KeyboardKeyLookup[e.Key][MetaKey.None] :
                                         new();


                if (charsToType.Any())
                {
                    TerminalProcess.WriteToStdIn(new string(charsToType.ToArray()));
                }
            }
        }
    }
}
