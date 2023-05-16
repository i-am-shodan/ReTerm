using Graphite;
using Graphite.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Sandbox.Terminal;

namespace Sandbox.Controls
{
    public class GlyphControl : Control
    {
        private Image<Rgb24> _icon;
        private readonly Window ourw;
        public bool DoNotCallUpdate = false;

        public GlyphControl(int gWidth, int gHeight, Window w)
        {
            _icon = TerminalFont.Empty;

            Size = new SizeF(_icon.Width, _icon.Height);
            Window = w;
            ourw = w;
        }

        public Image<Rgb24> Icon
        {
            get => _icon;
            set {
                _icon = value;
                ourw.Refresh(GetMinimumRedrawRect());
            }
        }

        private static Point ToInteger(PointF point)
        {
            return new Point((int)point.X, (int)point.Y);
        }

        public override void Draw(Image<Rgb24> buffer)
        {
            if (!DoNotCallUpdate)
            {
                var rect = GetMinimumRedrawRect();
                buffer.Mutate(g => g.DrawImage(_icon, ToInteger(rect.Location), 1));
            }
        }

        protected override RectangleF GetMinimumRedrawRect()
        {
            var iconRect = new RectangleF(0, 0, Size.Width, Size.Height);
            iconRect.Align(Bounds, TextAlign);
            return iconRect;
        }

        internal void ClearContents()
        {
            Icon = TerminalFont.Empty;
        }

        internal void Set(char c, Rgb24 fg, Rgb24 bg)
        {
            Icon = TerminalFont.GetGlyph(c, fg, bg);
        }
    }
}
