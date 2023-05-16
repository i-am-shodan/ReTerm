using System;
using System.Collections.Generic;
using ReMarkable.NET.Unix.Driver.Generic;
using ReMarkable.NET.Unix.Driver.Keyboard;

namespace RmEmulator.Devices
{
    public class EmulatedKeyboardDriver : IKeyboardDriver
    {
        private readonly EmulatorWindow _window;

        public EmulatedKeyboardDriver(EmulatorWindow window)
        {
            _window = window;

            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;

            KeyStates = new Dictionary<KeyboardKey, ButtonState>();

            foreach (var v in Enum.GetValues(typeof(KeyboardKey)))
            {
                KeyStates.Add((KeyboardKey)v, ButtonState.Released);
            }
        }

        private void Window_KeyUp(OpenToolkit.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            var code = obj.ScanCode;
            KeyboardKey key = (KeyboardKey)code;

            Released?.Invoke(this, new KeyEventArgs(key));
        }

        private void Window_KeyDown(OpenToolkit.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            var code = obj.ScanCode;
            KeyboardKey key = (KeyboardKey)code;

            Pressed?.Invoke(this, new KeyPressEventArgs(key, obj.IsRepeat));
        }

        public Dictionary<KeyboardKey, ButtonState> KeyStates { get; }

        public event EventHandler<KeyPressEventArgs> Pressed;
        public event EventHandler<KeyEventArgs> Released;
    }
}