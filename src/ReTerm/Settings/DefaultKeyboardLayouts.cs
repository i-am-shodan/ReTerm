using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReMarkable.NET.Unix.Driver.Keyboard;
using static ReTerm.Settings.TerminalSettings;

namespace ReTerm.Settings
{
    public class DefaultKeyboardLayouts
    {
        
        public static string ConvertJSONToCSharpInitList(string json)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<KeyboardKey, Dictionary<MetaKey, List<char>>>>(json);
            var ret = "        public static Dictionary<KeyboardKey, Dictionary<MetaKey, List<char>>> NewLayout = new() {";

            foreach (var keyToSeq in dict)
            {
                var keyboardKey = keyToSeq.Key;
                var entry = Environment.NewLine+ "            { KeyboardKey."+keyboardKey.ToString()+", new Dictionary<MetaKey, List<char>>() {";

                foreach (var kdbEntry in keyToSeq.Value)
                {
                    string chars = string.Empty;
                    foreach (var c in kdbEntry.Value)
                    {
                        if (char.IsLetterOrDigit(c) || char.IsSymbol(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c))
                        {
                            chars += "'"+c+"', ";
                        }
                        else
                        {
                            chars = "\\x'" + BitConverter.GetBytes(c)[0] + "'";
                        }
                    }
                    chars = chars.TrimEnd(',');
                    entry += Environment.NewLine + "                { MetaKey."+kdbEntry.Key+", new List<char>() { "+chars+" } },";
                }

                entry += Environment.NewLine+ "            } },";
                ret += Environment.NewLine + entry;
            }

            ret += Environment.NewLine + "        };";
            return ret;
        }

        public static Dictionary<KeyboardKey, Dictionary<MetaKey, List<char>>> UK = new()
        {
            { KeyboardKey.Enter, new Dictionary<MetaKey, List<char>>() {{ MetaKey.None, new List<char>() { '\n' } } } },
            { KeyboardKey.Backspace, new Dictionary<MetaKey, List<char>>() {{ MetaKey.None, new List<char>() { '\b', '\x0' } } } },
            { KeyboardKey.Space, new Dictionary<MetaKey, List<char>>() {{ MetaKey.None, new List<char>() { ' ' } } } },

            { KeyboardKey.Tab, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\t'} },
                { MetaKey.Shift, new List<char>() { '\x1b' } }
            } },
            { KeyboardKey.CapsLock, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '|' } },
                { MetaKey.Shift, new List<char>() { '\x1b' } }
            } },
            { KeyboardKey.Grave, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '`' } } ,
                { MetaKey.Shift, new List<char>() { '"' } }
            } },
            { KeyboardKey.Semicolon, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { ':' } },
                { MetaKey.Shift, new List<char>() { ';' } }
            } },
            { KeyboardKey.Apostrophe, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\'' } },
                { MetaKey.Shift, new List<char>() { '@' } }
            } },
            { KeyboardKey.Comma, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { ',' } },
                { MetaKey.Shift, new List<char>() { '<' } },
                { MetaKey.Opt, new List<char>() { '{',  } },
            } },
            { KeyboardKey.Period, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '.' } },
                { MetaKey.Shift, new List<char>() { '>' } },
                { MetaKey.Opt, new List<char>() { '}',  } },
            } },
            { KeyboardKey.Slash, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '/' } } ,
                { MetaKey.Shift, new List<char>() { '?' } },
                { MetaKey.Opt, new List<char>() { '\\' } },
                { MetaKey.Ctrl, new List<char>() { '\x1c' } } ,
            } },
            { KeyboardKey.Equal, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '-' } } ,
                { MetaKey.Shift, new List<char>() { '_' } },
                { MetaKey.Opt, new List<char>() { '=' } }
            } },
            { KeyboardKey.Down, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x42' } }
            } },
            { KeyboardKey.Up, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x41' } }
            } },
            { KeyboardKey.Left, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x44' } }
            } },
            { KeyboardKey.Right, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x43' } }
            } },
            { KeyboardKey.NumberRow1, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '1' } } ,
                { MetaKey.Shift, new List<char>() { '!' } }
            } },
            { KeyboardKey.NumberRow2, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '2' } } ,
                { MetaKey.Shift, new List<char>() { '"' } }
            } },
            { KeyboardKey.NumberRow3, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '3' } } ,
                { MetaKey.Shift, new List<char>() { '£' } },
                { MetaKey.Opt, new List<char>() { '#' } }
            } },
            { KeyboardKey.NumberRow4, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '4' } } ,
                { MetaKey.Shift, new List<char>() { '$' } }
            } },
            { KeyboardKey.NumberRow5, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '5' } } ,
                { MetaKey.Shift, new List<char>() { '%' } }
            } },
            { KeyboardKey.NumberRow6, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '6' } } ,
                { MetaKey.Shift, new List<char>() { '^' } }
            } },
            { KeyboardKey.NumberRow7, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '7' } } ,
                { MetaKey.Shift, new List<char>() { '&' } }
            } },
            { KeyboardKey.NumberRow8, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '8' } } ,
                { MetaKey.Shift, new List<char>() { '*' } }
            } },
            { KeyboardKey.NumberRow9, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '9' } } ,
                { MetaKey.Shift, new List<char>() { '(' } },
                { MetaKey.Opt, new List<char>() { '~' } },
            } },
            { KeyboardKey.NumberRow0, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '0' } } ,
                { MetaKey.Shift, new List<char>() { ')' } },
                { MetaKey.Opt, new List<char>() { '+' } }
            } },
            { KeyboardKey.A, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'a' } } ,
                { MetaKey.Shift, new List<char>() { 'A' } },
                { MetaKey.Ctrl, new List<char>() { '\u0001' } },
            } },
            { KeyboardKey.B, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'b' } } ,
                { MetaKey.Shift, new List<char>() { 'B' } }
            } },
            { KeyboardKey.C, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'c' } } ,
                { MetaKey.Shift, new List<char>() { 'C' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x03' } }
            } },
            { KeyboardKey.D, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'd' } } ,
                { MetaKey.Shift, new List<char>() { 'D' } }
            } },
            { KeyboardKey.E, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'e' } } ,
                { MetaKey.Shift, new List<char>() { 'E' } },
                { MetaKey.Ctrl, new List<char>() { '\u0005' } },
            } },
            { KeyboardKey.F, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'f' } } ,
                { MetaKey.Shift, new List<char>() { 'F' } }
            } },
            { KeyboardKey.G, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'g' } } ,
                { MetaKey.Shift, new List<char>() { 'G' } },
                { MetaKey.Ctrl, new List<char>() { '\x07' } } ,
            } },
            { KeyboardKey.H, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'h' } } ,
                { MetaKey.Shift, new List<char>() { 'H' } }
            } },
            { KeyboardKey.I, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'i' } } ,
                { MetaKey.Shift, new List<char>() { 'I' } }
            } },
            { KeyboardKey.J, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'j' } } ,
                { MetaKey.Shift, new List<char>() { 'J' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x0a' } } ,
            } },
            { KeyboardKey.K, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'k' } } ,
                { MetaKey.Shift, new List<char>() { 'K' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x0b' } } ,
            } },
            { KeyboardKey.L, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'l' } } ,
                { MetaKey.Shift, new List<char>() { 'L' } }
            } },
            { KeyboardKey.M, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'm' } } ,
                { MetaKey.Shift, new List<char>() { 'M' } }
            } },
            { KeyboardKey.N, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'n' } } ,
                { MetaKey.Shift, new List<char>() { 'N' } }
            } },
            { KeyboardKey.O, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'o' } } ,
                { MetaKey.Shift, new List<char>() { 'O' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x0f' } } ,
            } },
            { KeyboardKey.P, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'p' } } ,
                { MetaKey.Shift, new List<char>() { 'P' } }
            } },
            { KeyboardKey.Q, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'q' } } ,
                { MetaKey.Shift, new List<char>() { 'Q' } }
            } },
            { KeyboardKey.R, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'r' } } ,
                { MetaKey.Shift, new List<char>() { 'R' } },
                { MetaKey.Ctrl, new List<char>() { '\x12' } } ,
            } },
            { KeyboardKey.S, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 's' } } ,
                { MetaKey.Shift, new List<char>() { 'S' } }
            } },
            { KeyboardKey.T, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 't' } } ,
                { MetaKey.Shift, new List<char>() { 'T' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x14' } } ,
            } },
            { KeyboardKey.U, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'u' } } ,
                { MetaKey.Shift, new List<char>() { 'U' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x15' } } ,
            } },
            { KeyboardKey.V, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'v' } } ,
                { MetaKey.Shift, new List<char>() { 'V' } }
            } },
            { KeyboardKey.W, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'w' } } ,
                { MetaKey.Shift, new List<char>() { 'W' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x17' } } ,
            } },
            { KeyboardKey.X, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'x' } } ,
                { MetaKey.Shift, new List<char>() { 'X' } } ,
                { MetaKey.Ctrl, new List<char>() { '\x18' } } ,
            } },
            { KeyboardKey.Y, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'y' } } ,
                { MetaKey.Shift, new List<char>() { 'Y' } }
            } },
            { KeyboardKey.Z, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'z' } } ,
                { MetaKey.Shift, new List<char>() { 'Z' } }
            } },
        };

        public static Dictionary<KeyboardKey, Dictionary<MetaKey, List<char>>> US = new()
        {
            { KeyboardKey.Enter, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\r',  } },
            } },

            { KeyboardKey.Backslash, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '~',  } },
                { MetaKey.Shift, new List<char>() { '¨',  } },
            } },

            { KeyboardKey.Backspace, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\b', '\x0' } },
            } },

            { KeyboardKey.Space, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { ' ',  } },
            } },

            { KeyboardKey.Tab, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\t',  } },
                { MetaKey.Shift, new List<char>() { '\u001b' } },
            } },

            { KeyboardKey.CapsLock, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '|',  } },
                { MetaKey.Shift, new List<char>() { '\u001b' } },
            } },

            { KeyboardKey.Grave, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '`',  } },
                { MetaKey.Shift, new List<char>() { '\u005c',  } },
            } },

            { KeyboardKey.Semicolon, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { ';',  } },
                { MetaKey.Shift, new List<char>() { ':',  } },
            } },

            { KeyboardKey.Apostrophe, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\'',  } },
                { MetaKey.Shift, new List<char>() { '\"',  } },
            } },

            { KeyboardKey.Comma, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { ',',  } },
                { MetaKey.Shift, new List<char>() { '<',  } },
                { MetaKey.Opt, new List<char>() { '{',  } },
            } },

            { KeyboardKey.Period, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '.',  } },
                { MetaKey.Shift, new List<char>() { '>',  } },
                { MetaKey.Opt, new List<char>() { '}',  } },
            } },

            { KeyboardKey.Slash, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '/',  } },
                { MetaKey.Shift, new List<char>() { '?',  } },
                { MetaKey.Opt, new List<char>() { '\\',  } },
                { MetaKey.Ctrl, new List<char>() { '\u001c' } },
            } },

            { KeyboardKey.Equal, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '-',  } },
                { MetaKey.Shift, new List<char>() { '_',  } },
                { MetaKey.Opt, new List<char>() { '=',  } },
            } },

	        { KeyboardKey.Down, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x42' } }
            } },

            { KeyboardKey.Up, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x41' } }
            } },

            { KeyboardKey.Left, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x44' } }
            } },

            { KeyboardKey.Right, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '\x1B', '\x5B', '\x43' } }
            } },

            { KeyboardKey.NumberRow1, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '1',  } },
                { MetaKey.Shift, new List<char>() { '!',  } },
                { MetaKey.Opt, new List<char>() { '\u001b' } },
            } },

            { KeyboardKey.NumberRow2, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '2',  } },
                { MetaKey.Shift, new List<char>() { '@',  } },
            } },

            { KeyboardKey.NumberRow3, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '3',  } },
                { MetaKey.Shift, new List<char>() { '#',  } },
            } },

            { KeyboardKey.NumberRow4, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '4',  } },
                { MetaKey.Shift, new List<char>() { '$',  } },
            } },

            { KeyboardKey.NumberRow5, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '5',  } },
                { MetaKey.Shift, new List<char>() { '%',  } },
            } },

            { KeyboardKey.NumberRow6, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '6',  } },
                { MetaKey.Shift, new List<char>() { '^',  } },
            } },

            { KeyboardKey.NumberRow7, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '7',  } },
                { MetaKey.Shift, new List<char>() { '&',  } },
            } },

            { KeyboardKey.NumberRow8, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '8',  } },
                { MetaKey.Shift, new List<char>() { '*',  } },
            } },

            { KeyboardKey.NumberRow9, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '9',  } },
                { MetaKey.Shift, new List<char>() { '(',  } },
                { MetaKey.Opt, new List<char>() { '"',  } },
            } },

            { KeyboardKey.NumberRow0, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '0',  } },
                { MetaKey.Shift, new List<char>() { ')',  } },
                { MetaKey.Opt, new List<char>() { '+',  } },
            } },

            { KeyboardKey.A, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'a',  } },
                { MetaKey.Shift, new List<char>() { 'A',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0001' } },
            } },

            { KeyboardKey.B, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'b',  } },
                { MetaKey.Shift, new List<char>() { 'B',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0002' } },
            } },

            { KeyboardKey.C, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'c',  } },
                { MetaKey.Shift, new List<char>() { 'C',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0003' } },
            } },

            { KeyboardKey.D, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'd',  } },
                { MetaKey.Shift, new List<char>() { 'D',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0004' } },
            } },

            { KeyboardKey.E, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'e',  } },
                { MetaKey.Shift, new List<char>() { 'E',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0005' } },
            } },

            { KeyboardKey.F, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'f',  } },
                { MetaKey.Shift, new List<char>() { 'F',  } },
            } },

            { KeyboardKey.G, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'g',  } },
                { MetaKey.Shift, new List<char>() { 'G',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0007' } },
            } },

            { KeyboardKey.H, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'h',  } },
                { MetaKey.Shift, new List<char>() { 'H',  } },
            } },

            { KeyboardKey.I, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'i',  } },
                { MetaKey.Shift, new List<char>() { 'I',  } },
            } },

            { KeyboardKey.J, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'j',  } },
                { MetaKey.Shift, new List<char>() { 'J',  } },
                { MetaKey.Ctrl, new List<char>() { '\n',  } },
            } },

            { KeyboardKey.K, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'k',  } },
                { MetaKey.Shift, new List<char>() { 'K',  } },
                { MetaKey.Ctrl, new List<char>() { '\u000b',  } },
            } },

            { KeyboardKey.L, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'l',  } },
                { MetaKey.Shift, new List<char>() { 'L',  } },
                { MetaKey.Ctrl, new List<char>() { '\u000c',  } },
            } },

            { KeyboardKey.M, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'm',  } },
                { MetaKey.Shift, new List<char>() { 'M',  } },
            } },

            { KeyboardKey.N, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'n',  } },
                { MetaKey.Shift, new List<char>() { 'N',  } },
            } },

            { KeyboardKey.O, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'o',  } },
                { MetaKey.Shift, new List<char>() { 'O',  } },
                { MetaKey.Ctrl, new List<char>() { '\u000f'} },
            } },

            { KeyboardKey.P, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'p',  } },
                { MetaKey.Shift, new List<char>() { 'P',  } },
            } },

            { KeyboardKey.Q, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'q',  } },
                { MetaKey.Shift, new List<char>() { 'Q',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0011' } },
            } },

            { KeyboardKey.R, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'r',  } },
                { MetaKey.Shift, new List<char>() { 'R',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0012' } },
            } },

            { KeyboardKey.S, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 's',  } },
                { MetaKey.Shift, new List<char>() { 'S',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0013' } },
            } },

            { KeyboardKey.T, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 't',  } },
                { MetaKey.Shift, new List<char>() { 'T',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0014' } },
            } },

            { KeyboardKey.U, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'u',  } },
                { MetaKey.Shift, new List<char>() { 'U',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0015' } },
            } },

            { KeyboardKey.V, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'v',  } },
                { MetaKey.Shift, new List<char>() { 'V',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0016' } },
            } },

            { KeyboardKey.W, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'w',  } },
                { MetaKey.Shift, new List<char>() { 'W',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0017' } },
            } },

            { KeyboardKey.X, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'x',  } },
                { MetaKey.Shift, new List<char>() { 'X',  } },
                { MetaKey.Ctrl, new List<char>() { '\u0018' } },
            } },

            { KeyboardKey.Y, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'y',  } },
                { MetaKey.Shift, new List<char>() { 'Y',  } },
            } },

            { KeyboardKey.Z, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { 'z',  } },
                { MetaKey.Shift, new List<char>() { 'Z',  } },
                { MetaKey.Ctrl, new List<char>() { '\u001a' } },
            } },
        };
    }
}