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
    internal struct Page
    {
        public List<Image<Rgb24>[]> Rows = new List<Image<Rgb24>[]>();

        public Page(int widthInChars, int heightInChars)
        {
            for (int x = 0; x < heightInChars; x++)
            {
                var logicalLine = new Image<Rgb24>[widthInChars].Select(h => new Image<Rgb24>(TerminalFont.GetWidth(), TerminalFont.GetHeight())).ToArray();
                Rows.Add(logicalLine);
            }
        }
    }

    internal class TerminalWindow
    {
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

        public void ScreenUpdate(int row, int col, string txt, Rgb24 fg, Rgb24 bg)
        {
            foreach (var c in txt)
            {
                if (row > CurrentPage.Rows.Count)
                {
                    break;
                }

                if (col > CurrentPage.Rows[row].Length)
                {
                    break;
                }

                // Don't perform update if cell wants to be empty and is empty
                bool currentCellIsEmpty = CurrentPage.Rows[row][col] == TerminalFont.Empty;
                if (currentCellIsEmpty && char.IsWhiteSpace(c))
                {
                    continue;
                }

                // Don't perform update if we are updating the same glyph
                var glyphToUpdateTo = TerminalFont.GetGlyph(c, fg, bg);
                if (glyphToUpdateTo == CurrentPage.Rows[row][col])
                {
                    continue;
                }

                CurrentPage.Rows[row][col] = glyphToUpdateTo;

                OutputDevices.Display.Draw(
                    CurrentPage.Rows[row][col],
                    CurrentPage.Rows[row][col].Bounds(),
                    LogicalPosToPoint(row, col), 
                    default, 
                    WaveformMode.Du, 
                    DisplayTemp.RemarkableDraw,
                    UpdateMode.Partial);
            }
        }

        public void Clear()
        {
            for (int x = 0; x < CurrentPage.Rows.Count; x++)
            {
                for (int y = 0; y < CurrentPage.Rows[x].Length; y++)
                {
                    CurrentPage.Rows[x][y] = TerminalFont.Empty;
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

        public void ClearLine(int row, int count = -1)
        {
            var max = count == -1 ? CurrentPage.Rows[row].Length : count;

            for (int col = 0; col < max; col++)
            {
                // we are only going to update the screen if the character is not empty
                if (CurrentPage.Rows[row][col] != TerminalFont.Empty)
                {
                    CurrentPage.Rows[row][col] = TerminalFont.Empty;
                    OutputDevices.Display.Draw(
                        CurrentPage.Rows[row][col],
                        CurrentPage.Rows[row][col].Bounds(),
                        LogicalPosToPoint(row, col),
                        default,
                        WaveformMode.Du,
                        DisplayTemp.RemarkableDraw,
                        UpdateMode.Full);
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
            var newPage = new Page(WidthInChars, HeightInChars);
            Pages.Add(newPage);

            CurrentPage = newPage;

            Clear();
        }
    }
}
