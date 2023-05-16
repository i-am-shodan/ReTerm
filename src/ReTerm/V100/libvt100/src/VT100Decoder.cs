﻿using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace libvt100
{
    public class VT100Decoder : AnsiDecoder, IVT100Decoder
    {
        protected List<IVT100DecoderClient> m_vt100Listeners;

        public VT100Decoder()
           : base()
        {
            m_vt100Listeners = new List<IVT100DecoderClient>();
        }

        protected override bool IsValidParameterCharacter(char _c)
        {
            return _c == '=' || _c == ' ' || base.IsValidParameterCharacter(_c);
        }

        protected override void ProcessCommand(byte _command, string _parameter, byte[] raw)
        {
            switch ((char)_command)
            {
                case 'c':
                    string deviceCode = OnGetDeviceCode(raw);
                    OnUnknown(raw);
                    break;

                case 'n':
                    if (_parameter == "5")
                    {
                        DeviceStatus status = OnGetDeviceStatus(raw);
                        string stringStatus = ((int)status).ToString();
                        byte[] output = new byte[2 + stringStatus.Length + 1];
                        int i = 0;
                        output[i++] = EscapeCharacter;
                        output[i++] = LeftBracketCharacter;
                        foreach (char c in stringStatus)
                        {
                            output[i++] = (byte)c;
                        }
                        output[i++] = (byte)'n';
                        OnOutput(output);
                    }
                    else
                    {
                        base.ProcessCommand(_command, _parameter, raw);
                    }
                    break;

                case '(':
                case ')':
                case 'r':
                    // Set normal font
                    OnUnknown(raw);
                    break;

                case 't':
                    string[] parameters = _parameter.Split(';');
                    switch (parameters[0])
                    {
                        case "3":
                            if (parameters.Length >= 3)
                            {
                                int left, top;
                                if (Int32.TryParse(parameters[1], out left) && Int32.TryParse(parameters[2], out top))
                                {
                                    OnMoveWindow(new Point(left, top), raw);
                                }
                            }
                            break;

                        case "8":
                            if (parameters.Length >= 3)
                            {
                                int rows, columns;
                                if (Int32.TryParse(parameters[1], out rows) && Int32.TryParse(parameters[2], out columns))
                                {
                                    OnResizeWindow(new Size(columns, rows), raw);
                                }
                            }
                            break;
                        default:
                            OnUnknown(raw);
                            break;

                    }
                    break;

                case '!':
                    // Graphics Repeat Introducer
                    OnUnknown(raw);
                    break;

                default:
                    base.ProcessCommand(_command, _parameter, raw);
                    break;
            }
        }

        virtual protected string OnGetDeviceCode(byte[] rawParameterData)
        {
            foreach (IVT100DecoderClient client in m_vt100Listeners)
            {
                string deviceCode = client.GetDeviceCode(this, rawParameterData);
                if (deviceCode != null)
                {
                    return deviceCode;
                }
            }
            return "UNKNOWN";
        }

        virtual protected DeviceStatus OnGetDeviceStatus(byte[] rawParameterData)
        {
            foreach (IVT100DecoderClient client in m_vt100Listeners)
            {
                DeviceStatus status = client.GetDeviceStatus(this, rawParameterData);
                if (status != DeviceStatus.Unknown)
                {
                    return status;
                }
            }
            return DeviceStatus.Failure;
        }

        virtual protected void OnResizeWindow(Size _size, byte[] rawParameterData)
        {
            foreach (IVT100DecoderClient client in m_vt100Listeners)
            {
                client.ResizeWindow(this, _size, rawParameterData);
            }
        }

        virtual protected void OnMoveWindow(Point _position, byte[] rawParameterData)
        {
            foreach (IVT100DecoderClient client in m_vt100Listeners)
            {
                client.MoveWindow(this, _position, rawParameterData);
            }
        }

        void IVT100Decoder.Subscribe(IVT100DecoderClient _client)
        {
            m_listeners.Add(_client);
            m_vt100Listeners.Add(_client);
        }

        void IVT100Decoder.UnSubscribe(IVT100DecoderClient _client)
        {
            m_vt100Listeners.Remove(_client);
            m_listeners.Remove(_client);
        }
    }
}
