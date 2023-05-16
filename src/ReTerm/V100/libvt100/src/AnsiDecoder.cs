using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace libvt100
{
    public class AnsiDecoder : EscapeCharacterDecoder, IAnsiDecoder
    {
        protected List<IAnsiDecoderClient> m_listeners;

        Encoding IDecoder.Encoding
        {
            get
            {
                return m_encoding;
            }
            set
            {
                if (m_encoding != value)
                {
                    m_encoding = value;
                    m_decoder = m_encoding.GetDecoder();
                    m_encoder = m_encoding.GetEncoder();
                }
            }
        }

        public AnsiDecoder()
           : base()
        {
            m_listeners = new List<IAnsiDecoderClient>();
        }

        private int DecodeInt(String _value, int _default)
        {
            if (_value.Length == 0)
            {
                return _default;
            }
            int ret;
            if (Int32.TryParse(_value.TrimStart('0'), out ret))
            {
                return ret;
            }
            else
            {
                return _default;
            }
        }

        protected override void ProcessCommand(byte _command, String _parameter, byte[] rawParameterData)
        {
            //System.Console.WriteLine ( "ProcessCommand: {0} {1}", (char) _command, _parameter );
            switch ((char)_command)
            {
                case 'A':
                    OnMoveCursor(Direction.Up, DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'B':
                    OnMoveCursor(Direction.Down, DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'C':
                    OnMoveCursor(Direction.Forward, DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'D':
                    OnMoveCursor(Direction.Backward, DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'E':
                    OnMoveCursorToBeginningOfLineBelow(DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'F': // end
                    OnMoveCursorToBeginningOfLineAbove(DecodeInt(_parameter, 1), rawParameterData);
                    //OnKey(Keys.End, rawParameterData);
                    break;

                case 'G':
                    OnMoveCursorToColumn(DecodeInt(_parameter, 1) - 1, rawParameterData);
                    break;

                case 'H': // home
                    //OnKey(Keys.Home, rawParameterData);
                    break;
                case 'f':
                    {
                        int separator = _parameter.IndexOf(';');
                        if (separator == -1)
                        {
                            OnMoveCursorTo(new Point(0, 0), rawParameterData);
                        }
                        else
                        {
                            String row = _parameter.Substring(0, separator);
                            String column = _parameter.Substring(separator + 1, _parameter.Length - separator - 1);
                            OnMoveCursorTo(new Point(DecodeInt(column, 1) - 1, DecodeInt(row, 1) - 1), rawParameterData);
                        }
                    }
                    break;

                case 'J':
                    OnClearScreen((ClearDirection)DecodeInt(_parameter, 0), rawParameterData);
                    break;

                case 'K':
                    OnClearLine((ClearDirection)DecodeInt(_parameter, 0), rawParameterData);
                    break;

                case 'S':
                    OnScrollPageUpwards(DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'T':
                    OnScrollPageDownwards(DecodeInt(_parameter, 1), rawParameterData);
                    break;

                case 'm':
                    {
                        String[] commands = _parameter.Split(';');
                        GraphicRendition[] renditionCommands = new GraphicRendition[commands.Length];
                        for (int i = 0; i < commands.Length; ++i)
                        {
                            renditionCommands[i] = (GraphicRendition)DecodeInt(commands[i], 0);
                            //System.Console.WriteLine ( "Rendition command: {0} = {1}", commands[i], renditionCommands[i]);
                        }
                        OnSetGraphicRendition(renditionCommands, rawParameterData);
                    }
                    break;

                case 'n':
                    if (_parameter == "6")
                    {
                        Point cursorPosition = OnGetCursorPosition(rawParameterData);
                        cursorPosition.X++;
                        cursorPosition.Y++;
                        String row = cursorPosition.Y.ToString();
                        String column = cursorPosition.X.ToString();
                        byte[] output = new byte[2 + row.Length + 1 + column.Length + 1];
                        int i = 0;
                        output[i++] = EscapeCharacter;
                        output[i++] = LeftBracketCharacter;
                        foreach (char c in row)
                        {
                            output[i++] = (byte)c;
                        }
                        output[i++] = (byte)';';
                        foreach (char c in column)
                        {
                            output[i++] = (byte)c;
                        }
                        output[i++] = (byte)'R';
                        OnOutput(output);
                    }
                    else
                    {
                        throw new InvalidCommandException(_command, _parameter);
                    }
                    break;

                case 's':
                    OnSaveCursor(rawParameterData);
                    break;

                case 'u':
                    OnRestoreCursor(rawParameterData);
                    break;

                case 'l':
                    switch (_parameter)
                    {
                        case "20":
                            // Set line feed mode
                            OnModeChanged(AnsiMode.LineFeed, rawParameterData);
                            break;

                        case "?1":
                            // Set cursor key to cursor  DECCKM 
                            OnModeChanged(AnsiMode.CursorKeyToCursor, rawParameterData);
                            break;

                        case "?2":
                            // Set ANSI (versus VT52)  DECANM
                            OnModeChanged(AnsiMode.VT52, rawParameterData);
                            break;

                        case "?3":
                            // Set number of columns to 80  DECCOLM 
                            OnModeChanged(AnsiMode.Columns80, rawParameterData);
                            break;

                        case "?4":
                            // Set jump scrolling  DECSCLM 
                            OnModeChanged(AnsiMode.JumpScrolling, rawParameterData);
                            break;

                        case "?5":
                            // Set normal video on screen  DECSCNM 
                            OnModeChanged(AnsiMode.NormalVideo, rawParameterData);
                            break;

                        case "?6":
                            // Set origin to absolute  DECOM 
                            OnModeChanged(AnsiMode.OriginIsAbsolute, rawParameterData);
                            break;

                        case "?7":
                            // Reset auto-wrap mode  DECAWM 
                            // Disable line wrap
                            OnModeChanged(AnsiMode.DisableLineWrap, rawParameterData);
                            break;

                        case "?8":
                            // Reset auto-repeat mode  DECARM 
                            OnModeChanged(AnsiMode.DisableAutoRepeat, rawParameterData);
                            break;

                        case "?9":
                            // Reset interlacing mode  DECINLM 
                            OnModeChanged(AnsiMode.DisableInterlacing, rawParameterData);
                            break;

                        case "?25":
                            OnModeChanged(AnsiMode.HideCursor, rawParameterData);
                            break;

                        default:
                            throw new InvalidParameterException(_command, _parameter);
                    }
                    break;

                case 'h':
                    switch (_parameter)
                    {
                        case "":
                            //Set ANSI (versus VT52)  DECANM
                            OnModeChanged(AnsiMode.ANSI, rawParameterData);
                            break;

                        case "20":
                            // Set new line mode
                            OnModeChanged(AnsiMode.NewLine, rawParameterData);
                            break;

                        case "?1":
                            // Set cursor key to application  DECCKM
                            OnModeChanged(AnsiMode.CursorKeyToApplication, rawParameterData);
                            break;

                        case "?3":
                            // Set number of columns to 132  DECCOLM
                            OnModeChanged(AnsiMode.Columns132, rawParameterData);
                            break;

                        case "?4":
                            // Set smooth scrolling  DECSCLM
                            OnModeChanged(AnsiMode.SmoothScrolling, rawParameterData);
                            break;

                        case "?5":
                            // Set reverse video on screen  DECSCNM
                            OnModeChanged(AnsiMode.ReverseVideo, rawParameterData);
                            break;

                        case "?6":
                            // Set origin to relative  DECOM
                            OnModeChanged(AnsiMode.OriginIsRelative, rawParameterData);
                            break;

                        case "?7":
                            //  Set auto-wrap mode  DECAWM
                            // Enable line wrap
                            OnModeChanged(AnsiMode.LineWrap, rawParameterData);
                            break;

                        case "?8":
                            // Set auto-repeat mode  DECARM
                            OnModeChanged(AnsiMode.AutoRepeat, rawParameterData);
                            break;

                        case "?9":
                            /// Set interlacing mode 
                            OnModeChanged(AnsiMode.Interlacing, rawParameterData);
                            break;

                        case "?25":
                            OnModeChanged(AnsiMode.ShowCursor, rawParameterData);
                            break;

                        default:
                            throw new InvalidParameterException(_command, _parameter);
                    }
                    break;

                case '>':
                    // Set numeric keypad mode
                    OnModeChanged(AnsiMode.NumericKeypad, rawParameterData);
                    break;

                case '=':
                    OnModeChanged(AnsiMode.AlternateKeypad, rawParameterData);
                    // Set alternate keypad mode (rto: non-numeric, presumably)
                    break;

                case (char)0x52:
                    var p = _parameter.Split(';');
                    var width = int.Parse(p[1]);
                    var height = int.Parse(p[0]);
                    OnSize(new Size(width, height), rawParameterData);
                    break;

                case (char)0x4f:
                    switch (_parameter)
                    {
                        //case "A":
                        //    OnKey(Keys.Up, rawParameterData);
                        //    break;
                        //case "B":
                        //    OnKey(Keys.Down, rawParameterData);
                        //    break;
                        //case "C":
                        //    OnKey(Keys.Right, rawParameterData);
                        //    break;
                        //case "D":
                        //    OnKey(Keys.Left, rawParameterData);
                        //    break;
                        //case "P":
                        //    OnKey(Keys.F1, rawParameterData);
                        //    break;
                        //case "Q":
                        //    OnKey(Keys.F2, rawParameterData);
                        //    break;
                        //case "R":
                        //    OnKey(Keys.F3, rawParameterData);
                        //    break;
                        //case "S":
                        //    OnKey(Keys.F4, rawParameterData);
                        //    break;
                        //case "T":
                        //    OnKey(Keys.F5, rawParameterData);
                        //    break;
                        //case "U":
                        //    OnKey(Keys.F6, rawParameterData);
                        //    break;
                        //case "V":
                        //    OnKey(Keys.F7, rawParameterData);
                        //    break;
                        default:
                            throw new NotImplementedException("Invalid param" + _parameter);
                    }
                    break;

                case (char)0x7e: // VT220, https://invisible-island.net/xterm/ctlseqs/ctlseqs.html
                    switch (_parameter)
                    {
                        //case "1":
                        //    OnKey(Keys.Home, rawParameterData);
                        //    break;
                        //case "2":
                        //    OnKey(Keys.Insert, rawParameterData);
                        //    break;
                        //case "3":
                        //    OnKey(Keys.Delete, rawParameterData);
                        //    break;
                        //case "4":
                        //    OnKey(Keys.End, rawParameterData);
                        //    break;
                        //case "5":
                        //    OnKey(Keys.PageUp, rawParameterData);
                        //    break;
                        //case "6":
                        //    OnKey(Keys.PageDown, rawParameterData);
                        //    break;
                        //case "15":
                        //    OnKey(Keys.F5, rawParameterData);
                        //    break;
                        //case "17":
                        //    OnKey(Keys.F6, rawParameterData);
                        //    break;
                        //case "18":
                        //    OnKey(Keys.F7, rawParameterData);
                        //    break;
                        //case "19":
                        //    OnKey(Keys.F8, rawParameterData);
                        //    break;
                        //case "20":
                        //    OnKey(Keys.F9, rawParameterData);
                        //    break;
                        //case "21":
                        //    OnKey(Keys.F10, rawParameterData);
                        //    break;
                        default:
                            throw new NotImplementedException("Invalid param" + _parameter);
                    }
                    break;

                default:
                    throw new InvalidCommandException(_command, _parameter);
            }
        }

        protected override bool IsValidOneCharacterCommand(char _command)
        {
            // Esc=	Set alternate keypad mode	DECKPAM
            // Esc>    Set numeric keypad mode DECKPNM
            return _command == '=' || _command == '>';
        }

        protected virtual void OnSetGraphicRendition(GraphicRendition[] _commands, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.SetGraphicRendition(this, _commands, raw);
            }
        }

        protected virtual void OnScrollPageUpwards(int _linesToScroll, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.ScrollPageUpwards(this, _linesToScroll, raw);
            }
        }

        protected virtual void OnScrollPageDownwards(int _linesToScroll, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.ScrollPageDownwards(this, _linesToScroll, raw);
            }
        }

        protected virtual void OnModeChanged(AnsiMode _mode, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.ModeChanged(this, _mode, raw);
            }
        }

        protected virtual void OnSaveCursor(byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.SaveCursor(this, raw);
            }
        }

        protected virtual void OnRestoreCursor(byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.RestoreCursor(this, raw);
            }
        }

        protected virtual Point OnGetCursorPosition(byte[] raw)
        {
            Point ret;
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                ret = client.GetCursorPosition(this, raw);
                if (!ret.IsEmpty)
                {
                    return ret;
                }
            }
            return Point.Empty;
        }

        protected virtual void OnClearScreen(ClearDirection _direction, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.ClearScreen(this, _direction, raw);
            }
        }

        protected virtual void OnClearLine(ClearDirection _direction, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.ClearLine(this, _direction, raw);
            }
        }

        protected virtual void OnMoveCursorTo(Point _position, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.MoveCursorTo(this, _position, raw);
            }
        }

        protected virtual void OnMoveCursorToColumn(int _columnNumber, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.MoveCursorToColumn(this, _columnNumber, raw);
            }
        }

        protected virtual void OnMoveCursor(Direction _direction, int _amount, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.MoveCursor(this, _direction, _amount, raw);
            }
        }

        protected virtual void OnMoveCursorToBeginningOfLineBelow(int _lineNumberRelativeToCurrentLine, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.MoveCursorToBeginningOfLineBelow(this, _lineNumberRelativeToCurrentLine, raw);
            }
        }

        protected virtual void OnMoveCursorToBeginningOfLineAbove(int _lineNumberRelativeToCurrentLine, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.MoveCursorToBeginningOfLineAbove(this, _lineNumberRelativeToCurrentLine, raw);
            }
        }

        protected override void OnCharacters(char[] _characters, byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.Characters(this, _characters, raw);
            }
        }

        protected override void OnSize(Size size, byte[] rawParamData)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.SetSize(this, size, rawParamData);
            }
        }

        //protected override void OnKey(Keys key, byte[] raw)
        //{
        //    foreach (IAnsiDecoderClient client in m_listeners)
        //    {
        //        client.Key(key, raw);
        //    }
        //}

        protected override void OnUnknown(byte[] raw)
        {
            foreach (IAnsiDecoderClient client in m_listeners)
            {
                client.Error(raw);
            }
        }

        private static string[] FUNCTIONKEY_MAP = { 
        //      F1     F2     F3     F4     F5     F6     F7     F8     F9     F10    F11  F12
            "11", "12", "13", "14", "15", "17", "18", "19", "20", "21", "23", "24",
        //      F13    F14    F15    F16    F17  F18    F19    F20    F21    F22
            "25", "26", "28", "29", "31", "32", "33", "34", "23", "24" };

        //bool IDecoder.KeyPressed(Keys _modifiers, Keys _key)
        //{
        //    if ((int)Keys.F1 <= (int)_key && (int)_key <= (int)Keys.F12)
        //    {
        //        byte[] r = new byte[5];
        //        r[0] = 0x1B;
        //        r[1] = (byte)'[';
        //        int n = (int)_key - (int)Keys.F1;
        //        if ((_modifiers & Keys.Shift) != Keys.None)
        //            n += 10;
        //        char tail;
        //        if (n >= 20)
        //            tail = (_modifiers & Keys.Control) != Keys.None ? '@' : '$';
        //        else
        //            tail = (_modifiers & Keys.Control) != Keys.None ? '^' : '~';
        //        string f = FUNCTIONKEY_MAP[n];
        //        r[2] = (byte)f[0];
        //        r[3] = (byte)f[1];
        //        r[4] = (byte)tail;
        //        OnOutput(r);
        //        return true;
        //    }
        //    else if (_key == Keys.Left || _key == Keys.Right || _key == Keys.Up || _key == Keys.Down)
        //    {
        //        byte[] r = new byte[3];
        //        r[0] = 0x1B;
        //        //if ( _cursorKeyMode == TerminalMode.Normal )
        //        r[1] = (byte)'[';
        //        //else
        //        //    r[1] = (byte) 'O';

        //        switch (_key)
        //        {
        //            case Keys.Up:
        //                r[2] = (byte)'A';
        //                break;
        //            case Keys.Down:
        //                r[2] = (byte)'B';
        //                break;
        //            case Keys.Right:
        //                r[2] = (byte)'C';
        //                break;
        //            case Keys.Left:
        //                r[2] = (byte)'D';
        //                break;
        //            default:
        //                throw new ArgumentException("unknown cursor key code", "key");
        //        }
        //        OnOutput(r);
        //        return true;
        //    }
        //    else
        //    {
        //        byte[] r = new byte[4];
        //        r[0] = 0x1B;
        //        r[1] = (byte)'[';
        //        r[3] = (byte)'~';
        //        if (_key == Keys.Insert)
        //        {
        //            r[2] = (byte)'1';
        //        }
        //        else if (_key == Keys.Home)
        //        {
        //            r[2] = (byte)'2';
        //        }
        //        else if (_key == Keys.PageUp)
        //        {
        //            r[2] = (byte)'3';
        //        }
        //        else if (_key == Keys.Delete)
        //        {
        //            r[2] = (byte)'4';
        //        }
        //        else if (_key == Keys.End)
        //        {
        //            r[2] = (byte)'5';
        //        }
        //        else if (_key == Keys.PageDown)
        //        {
        //            r[2] = (byte)'6';
        //        }
        //        else if (_key == Keys.Enter)
        //        {
        //            //return new byte[] { 0x1B, (byte) 'M', (byte) '~' };
        //            //r[1] = (byte) 'O';
        //            //r[2] = (byte) 'M';
        //            //return new byte[] { (byte) '\r', (byte) '\n' };
        //            r = new byte[] { 13 };
        //        }
        //        else if (_key == Keys.Escape)
        //        {
        //            r = new byte[] { 0x1B };
        //        }
        //        else if (_key == Keys.Tab)
        //        {
        //            r = new byte[] { (byte)'\t' };
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //        OnOutput(r);
        //        return true;
        //    }
        //}

        void IAnsiDecoder.Subscribe(IAnsiDecoderClient _client)
        {
            m_listeners.Add(_client);
        }

        void IAnsiDecoder.UnSubscribe(IAnsiDecoderClient _client)
        {
            m_listeners.Remove(_client);
        }

        void IDisposable.Dispose()
        {
            m_listeners.Clear();
            m_listeners = null;
        }
    }
}
