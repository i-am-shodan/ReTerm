using Newtonsoft.Json;
using System;
using System.IO;

namespace ReTerm.Settings
{
    public class TerminalSettings
    {
        private const string SettingsFileLocation = "/home/root/.ReTerm/settings.json";

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
    }
}
