using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections.Concurrent;
using System.Reflection;
using ReTerm.Fonts;

namespace Sandbox.Terminal
{
    public class Glyph
    {
        public Glyph(Image<Rgb24> baseImg, Image<Rgb24> icon)
        {
            this.ImageNoRotation = baseImg;
            this.ImageWithRotation = icon;
        }

        public Image<Rgb24> ImageWithRotation { get; private set; }

        public Image<Rgb24> ImageNoRotation { get; private set; }

        public char Character { get; private set; }
    }

    public class TerminalFont
    {
        public static Rgb24 Black = new Rgb24(0, 0, 0);
        public static Rgb24 White = new Rgb24(255, 255, 255);

        public static Font CurrentFont = GetFontAndInit(36);
        public static Glyph Empty;
        private static int width = 0;
        private static int height = 0;

        public static readonly ConcurrentDictionary<Rgb24, ConcurrentDictionary<char, Glyph>> glyphLookup = new();

        public static Font GetFontAndInit(int size)
        {
            CurrentFont = Fonts.UbuntuMonoSpaced.CreateFont(size); //GetFont().CreateFont(size);

            GetWidth();
            GetHeight();

            Empty = GetEmptyGlyph();

            return CurrentFont;
        }

        public static int GetWidth()
        {
            if (width == 0)
            {
                var strSize = TextMeasurer.Measure('M'.ToString(), new RendererOptions(CurrentFont));
                width = (int)Math.Round(strSize.Width);
            }
            return width;
        }

        public static int GetHeight()
        {
            if (height == 0)
            {
                var strSize = TextMeasurer.Measure('M'.ToString(), new RendererOptions(CurrentFont));

                height = (int)Math.Round(strSize.Height);
            }
            return height;
        }

        public static Glyph GetGlyph(char c, Rgb24 fg, Rgb24 bg)
        {
            if (glyphLookup.ContainsKey(fg) && glyphLookup[fg].ContainsKey(c))
            {
                return glyphLookup[fg][c];
            }
            else
            {
                CreateGlyph(c, fg, bg).Wait(); // todo
                return glyphLookup[fg][c];
            }
        }

        private static Glyph GetEmptyGlyph()
        {
            var _icon = new Image<Rgb24>(GetWidth(), GetHeight(), White);

            var baseImg = _icon.Clone();

            _icon.Mutate(x => x.Rotate(90));
            return new Glyph(baseImg, _icon);
        }

        public static async Task BuildGlyphCache()
        {
            List<Task> tasks = new();

            glyphLookup.TryAdd(Black, new ConcurrentDictionary<char, Glyph>());
            glyphLookup.TryAdd(White, new ConcurrentDictionary<char, Glyph>());

            for (char c = ' '; c <= 'z'; ++c)
            {
                tasks.Add(CreateGlyph(c, Black, White));
                tasks.Add(CreateGlyph(c, White, Black));
            }

            await Task.WhenAll(tasks);
        }

        private static Task CreateGlyph(char c, Rgb24 fg, Rgb24 bg)
        {
            return Task.Run(() =>
            {
                var img = new Image<Rgb24>(GetWidth(), GetHeight(), bg);
                img.Mutate(x => x.DrawText(c.ToString(), TerminalFont.CurrentFont, fg, new PointF(0, 0)));

                var baseImg = img.Clone();

                img.Mutate(x => x.Rotate(90));

                if (!glyphLookup.ContainsKey(fg))
                {
                    glyphLookup[fg] = new ConcurrentDictionary<char, Glyph>();
                }
                if (!glyphLookup[fg].ContainsKey(c))
                {
                    glyphLookup[fg][c] = new Glyph(baseImg, img);
                }
            });
        }
    }
}
