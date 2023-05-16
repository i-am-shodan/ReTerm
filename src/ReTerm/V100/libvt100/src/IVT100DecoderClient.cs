using System;
using System.Drawing;

namespace libvt100
{
   public enum DeviceStatus
   {
      Unknown = -1,
      Ok = 0,
      Failure = 3,
   }
   
   public interface IVT100DecoderClient : IAnsiDecoderClient
   {
      string GetDeviceCode ( IVT100Decoder _decoder, byte[] raw );
      DeviceStatus GetDeviceStatus ( IVT100Decoder _decoder, byte[] raw);
      /// <summary>Resize the terminal window to _size (given in characters).</summary>
      void ResizeWindow ( IVT100Decoder _decoder, Size _size, byte[] raw);
      /// <summary>Move the terminal window to _size (given in pixels).</summary>
      void MoveWindow ( IVT100Decoder _decoder, Point _position, byte[] raw);
   }
}
