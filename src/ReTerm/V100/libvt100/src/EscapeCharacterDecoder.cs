using System;
using System.Text;
using System.Collections.Generic;

namespace libVT100
{
    public abstract class EscapeCharacterDecoder : IDecoder
    {
        public const byte EscapeCharacter = 0x1B;
        public const byte LeftBracketCharacter = 0x5B;
        public const byte XonCharacter = 17;
        public const byte XoffCharacter = 19;

        protected enum State
        {
            Normal,
            Command,
        }
        protected State m_state;
        protected Encoding m_encoding;
        protected Decoder m_decoder;
        protected Encoder m_encoder;
        private List<byte> m_commandBuffer;
        protected bool m_supportXonXoff;
        protected bool m_xOffReceived;
        protected List<byte[]> m_outBuffer;

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

        public EscapeCharacterDecoder()
        {
            m_state = State.Normal;
            (this as IDecoder).Encoding = Encoding.ASCII;
            m_commandBuffer = new List<byte>();
            m_supportXonXoff = true;
            m_xOffReceived = false;
            m_outBuffer = new List<byte[]>();
        }

        virtual protected bool IsValidParameterCharacter(char _c)
        {
            //return (Char.IsNumber( _c ) || _c == '(' || _c == ')' || _c == ';' || _c == '"' || _c == '?');
            return (Char.IsNumber(_c) || _c == ';' || _c == '"' || _c == '?');
        }

        protected void AddToCommandBuffer(byte _byte)
        {
            if (m_supportXonXoff)
            {
                if (_byte == XonCharacter || _byte == XoffCharacter)
                {
                    return;
                }
            }

            m_commandBuffer.Add(_byte);
        }

        protected void AddToCommandBuffer(byte[] _bytes)
        {
            if (m_supportXonXoff)
            {
                foreach (byte b in _bytes)
                {
                    if (!(b == XonCharacter || b == XoffCharacter))
                    {
                        m_commandBuffer.Add(b);
                    }
                }
            }
            else
            {
                m_commandBuffer.AddRange(_bytes);
            }
        }

        protected virtual bool IsValidOneCharacterCommand(char _command)
        {
            return false;
        }

        protected void ProcessCommandBuffer()
        {
            /*
            System.Console.Write ( "ProcessCommandBuffer: " );
            foreach ( byte b in m_commandBuffer )
            {
                System.Console.Write ( "{0:X2} ", b );
            }
            System.Console.WriteLine ( "" );
            */

            m_state = State.Command;

            if (m_commandBuffer.Count > 1)
            {
                if (m_commandBuffer[0] != EscapeCharacter)
                {
                    throw new Exception("Internal error, first command character _MUST_ be the escape character, please report this bug to the author.");
                }

                int start = 1;
                // Is this a one or two byte escape code?
                if (m_commandBuffer[start] == LeftBracketCharacter)
                {
                    start++;

                    // It is a two byte escape code, but we still need more data
                    if (m_commandBuffer.Count < 3)
                    {
                        return;
                    }
                }

                bool insideQuotes = false;
                int end = start;
                while (end < m_commandBuffer.Count && (IsValidParameterCharacter((char)m_commandBuffer[end]) || insideQuotes))
                {
                    if (m_commandBuffer[end] == '"')
                    {
                        insideQuotes = !insideQuotes;
                    }
                    end++;
                }

                if (m_commandBuffer.Count == 2 && IsValidOneCharacterCommand((char)m_commandBuffer[start]))
                {
                    end = m_commandBuffer.Count - 1;
                }
                if (end == m_commandBuffer.Count)
                {
                    // More data needed
                    return;
                }

                Decoder decoder = (this as IDecoder).Encoding.GetDecoder();
                byte[] parameterData = new byte[end - start];
                for (int i = 0; i < parameterData.Length; i++)
                {
                    parameterData[i] = m_commandBuffer[start + i];
                }
                int parameterLength = decoder.GetCharCount(parameterData, 0, parameterData.Length);
                char[] parameterChars = new char[parameterLength];
                decoder.GetChars(parameterData, 0, parameterData.Length, parameterChars, 0);
                String parameter = new String(parameterChars);

                byte command = m_commandBuffer[end];

                try
                {
                    ProcessCommand(command, parameter);
                }
                finally
                {
                    //System.Console.WriteLine ( "Remove the processed commands" );

                    // Remove the processed commands
                    if (m_commandBuffer.Count == end - 1)
                    {
                        // All command bytes processed, we can go back to normal handling
                        m_commandBuffer.Clear();
                        m_state = State.Normal;
                    }
                    else
                    {
                        bool returnToNormalState = true;
                        for (int i = end + 1; i < m_commandBuffer.Count; i++)
                        {
                            if (m_commandBuffer[i] == EscapeCharacter)
                            {
                                m_commandBuffer.RemoveRange(0, i);
                                ProcessCommandBuffer();
                                returnToNormalState = false;
                            }
                            else
                            {
                                ProcessNormalInput(m_commandBuffer[i]);
                            }
                        }
                        if (returnToNormalState)
                        {
                            m_commandBuffer.Clear();

                            m_state = State.Normal;
                        }
                    }
                }
            }
        }

        protected void ProcessNormalInput(byte _data)
        {
            //System.Console.WriteLine ( "ProcessNormalInput: {0:X2}", _data );
            if (_data == EscapeCharacter)
            {
                throw new Exception("Internal error, ProcessNormalInput was passed an escape character, please report this bug to the author.");
            }
            if (m_supportXonXoff)
            {
                if (_data == XonCharacter || _data == XoffCharacter)
                {
                    return;
                }
            }

            byte[] data = new byte[] { _data };
            int charCount = m_decoder.GetCharCount(data, 0, 1);
            char[] characters = new char[charCount];
            m_decoder.GetChars(data, 0, 1, characters, 0);

            if (charCount > 0)
            {
                OnCharacters(characters);
            }
            else
            {
                //System.Console.WriteLine ( "char count was zero" );
            }

        }

        void IDecoder.Input(byte[] _data)
        {
            /*
            System.Console.Write ( "Input[{0}]: ", m_state );
            foreach ( byte b in _data )
            {
                System.Console.Write ( "{0:X2} ", b );
            }
            System.Console.WriteLine ( "" );
            */

            if (_data.Length == 0)
            {
                throw new ArgumentException("Input can not process an empty array.");
            }

            if (m_supportXonXoff)
            {
                foreach (byte b in _data)
                {
                    if (b == XoffCharacter)
                    {
                        m_xOffReceived = true;
                    }
                    else if (b == XonCharacter)
                    {
                        m_xOffReceived = false;
                        if (m_outBuffer.Count > 0)
                        {
                            foreach (byte[] output in m_outBuffer)
                            {
                                OnOutput(output);
                            }
                        }
                    }
                }
            }

            switch (m_state)
            {
                case State.Normal:
                    if (_data[0] == EscapeCharacter)
                    {
                        AddToCommandBuffer(_data);
                        ProcessCommandBuffer();
                    }
                    else
                    {
                        int i = 0;
                        while (i < _data.Length && _data[i] != EscapeCharacter)
                        {
                            ProcessNormalInput(_data[i]);
                            i++;
                        }
                        if (i != _data.Length)
                        {
                            while (i < _data.Length)
                            {
                                AddToCommandBuffer(_data[i]);
                                i++;
                            }
                            ProcessCommandBuffer();
                        }
                    }
                    break;

                case State.Command:
                    AddToCommandBuffer(_data);
                    ProcessCommandBuffer();
                    break;
            }
        }

        void IDecoder.CharacterTyped(char _character)
        {
            byte[] data = m_encoding.GetBytes(new char[] { _character });
            OnOutput(data);
        }

        void IDisposable.Dispose()
        {
            m_encoding = null;
            m_decoder = null;
            m_encoder = null;
            m_commandBuffer = null;
        }

        abstract protected void OnCharacters(char[] _characters);
        abstract protected void ProcessCommand(byte _command, String _parameter);

        virtual public event DecoderOutputDelegate Output;
        virtual protected void OnOutput(byte[] _output)
        {
            if (Output != null)
            {
                if (m_supportXonXoff && m_xOffReceived)
                {
                    m_outBuffer.Add(_output);
                }
                else
                {
                    Output(this, _output);
                }
            }
        }
    }
}
