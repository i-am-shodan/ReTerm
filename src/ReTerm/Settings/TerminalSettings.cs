using Newtonsoft.Json;
using ReMarkable.NET.Unix.Driver.Keyboard;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReTerm.Settings
{
    public class TerminalSettings
    {
        private const string SettingsFileLocation = "/home/root/.ReTerm/settings.json";

        public enum MetaKey
        {
            None,
            Shift,
            Opt,
            Ctrl
        }

        public static TerminalSettings Get() {
            Console.WriteLine("Loading settings file from: " + SettingsFileLocation);

            // try and load
            if (File.Exists(SettingsFileLocation))
            {
                Console.WriteLine("Loading settings");
                var json = File.ReadAllText(SettingsFileLocation);
                return JsonConvert.DeserializeObject<TerminalSettings>(json);
            }
            else
            {
                Console.WriteLine("Settings not found, creating");
                var settings = new TerminalSettings();
                
                var settingdStr = JsonConvert.SerializeObject(settings, Formatting.Indented);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileLocation));
                    Console.WriteLine("Created settings dir");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not create settings dir: "+ex.Message);
                }

                try
                {
                    File.WriteAllText(SettingsFileLocation, settingdStr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not write settings file: "+ex.Message);
                }

                return settings;
            }
        }

        /// <summary>
        ///  When set to true the cursor won't blink
        /// </summary>
        public bool DontBlinkCursor { get; set; } = false;

        /// <summary>
        /// Disables marks on screen
        /// </summary>
        public bool DisableMouseAndTouch { get; set; } = false;

        /// <summary>
        /// The time to elapse before clearing the touch marks
        /// </summary>
        public TimeSpan TouchMarksClearTime { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The rate at which the cursor switches on and off
        /// </summary>
        public TimeSpan CursorBlinkInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Forces the terminal background color to a specific value
        /// </summary>
        public string ForceBackgroundHexColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// Forces the terminal foreground color to a specific value
        /// </summary>
        public string ForceForegroundHexColor { get; set; } = "#000000";

        /// <summary>
        /// The path to TTF file to load instead of the included font file
        /// </summary>
        public string FontPath { get; set; } = "";

        /// <summary>
        /// The name of the font to use in the font referenced by FontPath
        /// </summary>
        public string FontName { get; set; } = "";

        /// <summary>
        /// The font size to use
        /// </summary>
        public int FontSize { get; set; } = 36;

        /// <summary>
        /// This is the table used to translate a keyboard key into a character that is sent to bash
        /// </summary>
        public Dictionary<KeyboardKey, Dictionary<MetaKey, List<char>>> KeyboardKeyLookup = new()
        {
            { KeyboardKey.Enter, new Dictionary<MetaKey, List<char>>() {{ MetaKey.None, new List<char>() { '\n' } } } },
            { KeyboardKey.Backspace, new Dictionary<MetaKey, List<char>>() {{ MetaKey.None, new List<char>() { '\b' } } } },
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
                { MetaKey.Shift, new List<char>() { '<' } }
            } },
            { KeyboardKey.Period, new Dictionary<MetaKey, List<char>>() {
                { MetaKey.None, new List<char>() { '.' } },
                { MetaKey.Shift, new List<char>() { '>' } }
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
                { MetaKey.Shift, new List<char>() { 'A' } }
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
                { MetaKey.Shift, new List<char>() { 'E' } }
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
                { MetaKey.Shift, new List<char>() { 'R' } }
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
    }
}
