using libVT100;
using Sandbox;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]

namespace Common.ANSI.ANSIParser
{
    internal class PseudoUnixTerminal : IVT100DecoderClient
    {
        private int CurrentRow = 1;
        private int CurrentColumn = 1;
        private TerminalWindow window;

        public PseudoUnixTerminal(TerminalWindow window)
        {
            this.window = window;
        }

        public void Characters(IAnsiDecoder _sender, char[] _chars)
        {
            foreach (var c in _chars)
            {
                switch (c)
                {
                    case '\a':
                        // bell
                        continue;
                    case '\t':
                        CurrentColumn++;
                        continue;
                    case '\b':
                        window.ClearLine(CurrentColumn, 1);
                        CurrentColumn--;
                        continue;
                    case '\r':
                        continue;
                    default:
                        break;

                }

                if (c == '\n')
                {
                    CurrentRow++;
                    CurrentColumn = 1;
                    continue;
                }
                else if (char.IsControl(c))
                {
                    Console.WriteLine("Warning control code escaped VT100 handling, ignoring "+(byte) c);
                    continue;
                }

                if (CurrentRow >= window.HeightInChars)
                {
                    // we need to add a new page
                    window.AddPageAndSetAsCurrent();
                    CurrentRow = 1;
                    // Keep the current column as it is
                }

                window.ScreenUpdate(CurrentRow, CurrentColumn, c.ToString(), new Rgb24(0, 0, 0), new Rgb24(255, 255, 255));
                CurrentColumn++;
            }
        }

        public void SetSize(IAnsiDecoder _sender, Size size)
        {
            Console.WriteLine("[!] NOTIMPLEMENTED - Set size: " + size.ToString());
        }

        public void Error(byte[] raw)
        {
        }

        public void ClearLine(IAnsiDecoder _sender, ClearDirection _direction)
        {
            CurrentColumn = 1;
            window.ClearLine(CurrentRow);
        }

        public void ClearScreen(IAnsiDecoder _sender, ClearDirection _direction)
        {
            CurrentRow = 1;
            CurrentColumn = 1;
            window.Clear();
        }

        public Point GetCursorPosition(IAnsiDecoder _sender)
        {
            return new Point(CurrentRow, CurrentColumn);
        }

        public Size GetSize(IAnsiDecoder _sender)
        {
            return new Size(window.WidthInChars, window.HeightInChars);
        }

        public void ModeChanged(IAnsiDecoder _sender, AnsiMode _mode)
        {
            Console.WriteLine("[!] NOTIMPLEMENTED - Mode change requested: " + _mode.ToString());
        }

        public void MoveCursor(IAnsiDecoder _sender, Direction _direction, int _amount)
        {
            // We don't currently have a cursor!
            Console.WriteLine("[!] NOTIMPLEMENTED - MoveCursor: "+_amount);
        }

        public void MoveCursorTo(IAnsiDecoder _sender, Point _position)
        {
            Console.WriteLine("MoveCursorToBeginningOfLineAbove: " + _position.X + ", " + _position.Y);

            CurrentRow = _position.X;
            CurrentColumn = _position.Y;
        }

        public void MoveCursorToBeginningOfLineAbove(IAnsiDecoder _sender, int _lineNumberRelativeToCurrentLine)
        {
            Console.WriteLine("MoveCursorToBeginningOfLineAbove: "+ _lineNumberRelativeToCurrentLine);

            CurrentColumn = 1;
            CurrentRow = CurrentRow - _lineNumberRelativeToCurrentLine;

            if (CurrentRow < 1)
            {
                CurrentRow = 1;
            }
        }

        public void MoveCursorToBeginningOfLineBelow(IAnsiDecoder _sender, int _lineNumberRelativeToCurrentLine)
        {
            CurrentColumn = 1;
            CurrentRow += _lineNumberRelativeToCurrentLine;
        }

        public void MoveCursorToColumn(IAnsiDecoder _sender, int _columnNumber)
        {
            CurrentColumn = _columnNumber;
        }

        private int savedCursorRow = -1;
        private int savedCursorColumn = -1;

        public void RestoreCursor(IAnsiDecoder _sender)
        {
            CurrentRow = savedCursorRow;
            CurrentColumn= savedCursorColumn;
        }

        public void SaveCursor(IAnsiDecoder _sernder)
        {
            savedCursorRow = CurrentRow;
            savedCursorColumn = CurrentColumn;
        }

        public void ScrollPageDownwards(IAnsiDecoder _sender, int _linesToScroll)
        {
            Console.WriteLine("[!] NOTIMPLEMENTED - Scroll downwards");
        }

        public void ScrollPageUpwards(IAnsiDecoder _sender, int _linesToScroll)
        {
            Console.WriteLine("[!] NOTIMPLEMENTED - Scroll upwards");
        }

        public void SetGraphicRendition(IAnsiDecoder _sender, GraphicRendition[] _commands)
        {
            // We don't support graphics!

            //Console.WriteLine("[!] NOTIMPLEMENTED - Set graphics");
        }

        public string GetDeviceCode(IVT100Decoder _decoder)
        {
            return "UNKNOWN";
        }

        public DeviceStatus GetDeviceStatus(IVT100Decoder _decoder)
        {
            return DeviceStatus.Ok;
        }

        public void ResizeWindow(IVT100Decoder _decoder, Size _size)
        {
            Console.WriteLine("[!] NOTIMPLEMENTED - Resize window");
        }

        public void MoveWindow(IVT100Decoder _decoder, Point _position)
        {
            Console.WriteLine("[!] NOTIMPLEMENTED - Move window");
        }

        public void Dispose()
        {
        }
    }
}
