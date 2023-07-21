using Newtonsoft.Json;
using ReMarkable.NET.Unix.Driver.Keyboard;
using ReMarkable.NET.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        public static async Task<TerminalSettings> Get() {
            Console.WriteLine("Loading settings file from: " + SettingsFileLocation);

            TerminalSettings settings;

            // try and load
            if (File.Exists(SettingsFileLocation))
            {
                Console.WriteLine("Loading settings");
                var json = File.ReadAllText(SettingsFileLocation);
                settings = JsonConvert.DeserializeObject<TerminalSettings>(json);
            }
            else
            {
                Console.WriteLine("Settings not found, creating");
                settings = new TerminalSettings();
                
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
            }

            if (settings.KeyboardKeyLookup == null || settings.KeyboardKeyLookup.Count == 0)
            {
                var lang = string.Empty;
                if (DeviceType.GetDevice() != Device.Emulator)
                {
                    lang = await File.ReadAllTextAsync("/sys/pogo/status/lang");
                }

                switch (lang.Trim())
                {
                    case "UK":
                        Console.WriteLine("Detected language UK");
                        settings.KeyboardKeyLookup = DefaultKeyboardLayouts.UK;
                        break;
                    case "US":
                        Console.WriteLine("Detected language US");
                        settings.KeyboardKeyLookup = DefaultKeyboardLayouts.US;
                        break;
                    default:
                        Console.WriteLine("Unknown lang: "+lang + " using UK");
                        settings.KeyboardKeyLookup = DefaultKeyboardLayouts.UK;
                        break;
                }
            }

            return settings;
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
        /// https://en.wikipedia.org/wiki/ASCII#ASCII_control_code_chart
        /// </summary>
        public Dictionary<KeyboardKey, Dictionary<MetaKey, List<char>>> KeyboardKeyLookup { get; set; } = null;
    }
}
