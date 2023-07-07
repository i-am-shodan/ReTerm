using ReMarkable.NET.Unix.Driver.Display.EinkController;
using ReMarkable.NET.Unix.Driver;
using Sandbox.Terminal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System;

namespace Sandbox
{
    internal class WindowGlyph
    {
        public Image<Rgb24> Image { get; private set; }
        public Glyph BaseGlyph { get; private set; } = TerminalFont.Empty;

        public bool HasCursorOverlay => cursor != null;

        public bool IsEmpty => Image == TerminalFont.Empty.ImageWithRotation;

        public Rgb24 ForegroundColor = TerminalFont.Black;
        public Rgb24 BackgroundColor = TerminalFont.White;

        public bool IsDefaultFgAndBg => ForegroundColor == TerminalFont.White && BackgroundColor == TerminalFont.Black;

        private char? cursor = null;

        public void Set(Glyph glyph, Rgb24 fg, Rgb24 bg)
        {
            Image = glyph.ImageWithRotation;
            BaseGlyph = glyph;
            ForegroundColor = fg;
            BackgroundColor = bg;

            if (HasCursorOverlay)
            {
                DrawCursor();
            }
        }

        public void AddCursor(char c)
        {
            if (HasCursorOverlay)
            {
                return;
            }

            cursor = c;
            DrawCursor();
        }

        internal void DrawCursor()
        {
            if (cursor == null)
            {
                return;
            }

            var c = cursor.ToString();

            var img = BaseGlyph.ImageNoRotation.Clone();
            img.Mutate(x => x.DrawText(c, TerminalFont.CurrentFont, TerminalFont.Black, new Point(0, 0))); //todo, override settings
            img.Mutate(x => x.Rotate(90));

            Image = img;
        }

        public void RemoveCursor()
        {
            if (!HasCursorOverlay)
            {
                return;
            }

            var temp = Image;

            cursor = null;
            Image = BaseGlyph.ImageWithRotation;
            temp.Dispose();
        }

        public void Clear()
        {
            Image = TerminalFont.Empty.ImageWithRotation;
            BaseGlyph = TerminalFont.Empty;
            ForegroundColor = TerminalFont.Black;
            BackgroundColor = TerminalFont.White;
            DrawCursor();
        }
    }

    internal struct Page
    {
        public List<WindowGlyph[]> Rows = new();

        public Page(int widthInChars, int heightInChars)
        {
            for (int x = 0; x < heightInChars; x++)
            {
                var logicalLine = new WindowGlyph[widthInChars].Select(h => new WindowGlyph()).ToArray();
                Rows.Add(logicalLine);
            }
        }
    }

    internal class TerminalWindow
    {
        private object readWriteLock = new();
        private readonly List<Page> Pages = new List<Page>();
        private Page CurrentPage;
        private readonly Action Quit;
        public int WidthInChars { get; private set; }
        public int HeightInChars { get; private set; }

        public TerminalWindow(int widthInChars, int heightInChars, Action quit)
        {
            WidthInChars = widthInChars;
            HeightInChars = heightInChars;
            this.Quit = quit;

            Pages.Add(new Page(widthInChars, heightInChars));
            CurrentPage = Pages[0];

            Clear();
        }

        public void ScreenUpdate(int row, int col, string txt, Rgb24 fg, Rgb24 bg, bool partialUpdate = true)
        {
            lock (readWriteLock)
            {
                foreach (var c in txt)
                {
                    if (row >= CurrentPage.Rows.Count)
                    {
                        break;
                    }

                    if (col >= CurrentPage.Rows[row].Length)
                    {
                        break;
                    }

                    // Don't perform update if cell wants to be empty and is empty
                    bool isEmptyAlready = 
                        char.IsWhiteSpace(c) && 
                        CurrentPage.Rows[row][col].IsEmpty && 
                        CurrentPage.Rows[row][col].ForegroundColor == fg && 
                        CurrentPage.Rows[row][col].BackgroundColor == bg;

                    if (isEmptyAlready)
                    {
                        continue;
                    }

                    // Don't perform update if we are updating the same glyph
                    var glyphToUpdateTo = TerminalFont.GetGlyph(c, fg, bg);
                    if (glyphToUpdateTo.ImageWithRotation == CurrentPage.Rows[row][col].BaseGlyph.ImageWithRotation)
                    {
                        continue;
                    }

                    CurrentPage.Rows[row][col].Set(glyphToUpdateTo, fg, bg);

                    OutputDevices.Display.Draw(
                        CurrentPage.Rows[row][col].Image,
                        CurrentPage.Rows[row][col].Image.Bounds(),
                        LogicalPosToPoint(row, col),
                        default,
                        WaveformMode.Du,
                        DisplayTemp.RemarkableDraw,
                        partialUpdate ? UpdateMode.Partial : UpdateMode.Full);
                }
            }
        }

        public void Clear()
        {
            lock (readWriteLock)
            {
                for (int x = 0; x < CurrentPage.Rows.Count; x++)
                {
                    for (int y = 0; y < CurrentPage.Rows[x].Length; y++)
                    {
                        CurrentPage.Rows[x][y].Clear();
                    }
                }
                using (var blankScreen = new Image<Rgb24>(OutputDevices.Display.VisibleWidth, OutputDevices.Display.VisibleHeight))
                {
                    blankScreen.Mutate(context => context
                        .SetGraphicsOptions(options => options.Antialias = false)
                        .Clear(Color.White));
                    OutputDevices.Display.Draw(blankScreen, blankScreen.Bounds(), Point.Empty, waveformMode: WaveformMode.Auto);
                }
            }
        }

        public void ForceRefresh()
        {
            using (var blankScreen = new Image<Rgb24>(OutputDevices.Display.VisibleWidth, OutputDevices.Display.VisibleHeight))
            {
                blankScreen.Mutate(context => context
                    .SetGraphicsOptions(options => options.Antialias = false)
                    .Clear(Color.White));
                OutputDevices.Display.Draw(blankScreen, blankScreen.Bounds(), Point.Empty, waveformMode: WaveformMode.Auto);
            }

            for (int row = 0; row < CurrentPage.Rows.Count; row++)
            {
                for (int col = 0; col < CurrentPage.Rows[row].Length; col++)
                {
                    OutputDevices.Display.Draw(
                        CurrentPage.Rows[row][col].Image,
                        CurrentPage.Rows[row][col].Image.Bounds(),
                        LogicalPosToPoint(row, col),
                        default,
                        WaveformMode.Du,
                        DisplayTemp.RemarkableDraw,
                        UpdateMode.Full);
                }
            }
        }

        public void ClearLine(int row, int count = -1)
        {
            lock (readWriteLock)
            {
                var max = count == -1 ? CurrentPage.Rows[row].Length : count;

                for (int col = 0; col < max; col++)
                {
                    // we are only going to update the screen if the character is not empty
                    if (!CurrentPage.Rows[row][col].IsEmpty)
                    {
                        CurrentPage.Rows[row][col].Clear();
                        OutputDevices.Display.Draw(
                            CurrentPage.Rows[row][col].Image,
                            CurrentPage.Rows[row][col].Image.Bounds(),
                            LogicalPosToPoint(row, col),
                            default,
                            WaveformMode.Du,
                            DisplayTemp.RemarkableDraw,
                            UpdateMode.Full);
                    }
                }
            }
        }

        private Point LogicalPosToPoint(int row, int col)
        {
            var x = col * TerminalFont.GetWidth();
            var y = OutputDevices.Display.VirtualWidth - (row * TerminalFont.GetHeight()) - TerminalFont.GetHeight();
            return new Point(y, x);
        }

        public void AddPageAndSetAsCurrent()
        {
            //TODO
            //var newPage = new Page(WidthInChars, HeightInChars);
            //Pages.Add(newPage);

            //CurrentPage = newPage;

            Clear();
        }

        public void SetCursor(int row, int col, char cursorChar)
        {
            lock (readWriteLock)
            {
                CurrentPage.Rows[row][col].AddCursor(cursorChar);

                OutputDevices.Display.Draw(
                    CurrentPage.Rows[row][col].Image,
                    CurrentPage.Rows[row][col].Image.Bounds(),
                    LogicalPosToPoint(row, col),
                    default,
                    WaveformMode.Du,
                    DisplayTemp.RemarkableDraw,
                    UpdateMode.Partial);
            }
        }

        public void UnsetCursor(int row, int col)
        {
            lock (readWriteLock)
            {
                CurrentPage.Rows[row][col].RemoveCursor();

                OutputDevices.Display.Draw(
                    CurrentPage.Rows[row][col].Image,
                    CurrentPage.Rows[row][col].Image.Bounds(),
                    LogicalPosToPoint(row, col),
                    default,
                    WaveformMode.Du,
                    DisplayTemp.RemarkableDraw,
                    UpdateMode.Partial);
            }
        }
    }
}
