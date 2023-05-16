using System;
using System.Collections.Generic;
using System.Text;
using Graphite.Symbols;
using Graphite.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Graphite.Controls
{
    public class IconLabel<T> : Control where T : Enum
    {
        private readonly SymbolAtlas<T> _atlas;
        private T _icon;
        private int _iconSize;

        public T Icon
        {
            get => _icon;
            set => RedrawWithChange(() => _icon = value);
        }

        public int IconSize
        {
            get => _iconSize;
            set => RedrawWithChange(() => _iconSize = value);
        }

        public IconLabel(SymbolAtlas<T> atlas, int iconSize = 32)
        {
            _atlas = atlas;
            _iconSize = iconSize;

            Size = new SizeF(_iconSize, _iconSize);
        }

        protected override RectangleF GetMinimumRedrawRect()
        {
            var iconRect = new RectangleF(0, 0, Size.Width, Size.Height);
            iconRect.Align(Bounds, TextAlign);
            return iconRect;
        }

        public override void Draw(Image<Rgb24> buffer)
        {
            try
            {
                var rect = GetMinimumRedrawRect();
                var icon = _atlas.GetIcon(IconSize, Icon);

                if (Size.Width == IconSize && Size.Height == IconSize)
                {
                    buffer.Mutate(g => g.DrawImage(icon, rect.Location.ToInteger(), 1));
                }
                else
                {
                    // icon size is different from icon, this is useful to increase the size
                    // of the hitbox, we position the icon in the center
                    var newRectange = new RectangleF(rect.X + ((Size.Width - IconSize) /2), rect.Y + ((Size.Height - IconSize) / 2), rect.Width, rect.Height);
                    buffer.Mutate(g => g.DrawImage(icon, newRectange.Location.ToInteger(), 1));
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to draw image: " + Icon.ToString());
            }
        }
    }
}
