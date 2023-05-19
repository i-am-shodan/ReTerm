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

namespace Sandbox.Terminal
{
    public class TerminalFont
    {
        public static Rgb24 Black = new Rgb24(0, 0, 0);
        public static Rgb24 White = new Rgb24(255, 255, 255);

        public static Font CurrentFont = GetFontAndInit(36);
        public static Image<Rgb24> Empty;
        private static int width = 0;
        private static int height = 0;

        public static Font GetFontAndInit(int size)
        {
            CurrentFont = GetFont().CreateFont(size);

            GetWidth();
            GetHeight();

            Empty = GetEmptyGlyph();

            return CurrentFont;
        }

        private static FontFamily GetFont()
        {
            var fonts = new FontCollection();

            var filePath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Ubuntu Mono derivative Powerline.ttf");

            using var ms = new MemoryStream(File.ReadAllBytes(filePath));
            fonts.Install(ms);

            return fonts.Find("Ubuntu Mono derivative Powerline");
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

        public static Image<Rgb24> GetGlyph(char c, Rgb24 fg, Rgb24 bg)
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

        private static Image<Rgb24> GetEmptyGlyph()
        {
            var _icon = new Image<Rgb24>(GetWidth(), GetHeight(), White);
            _icon.Mutate(x => x.DrawText(" ", CurrentFont, Black, new PointF(0, 0)));
            return _icon;
        }

        public static readonly ConcurrentDictionary<Rgb24, ConcurrentDictionary<char, Image<Rgb24>>> glyphLookup = new();

        public static async Task BuildGlyphCache()
        {
            List<Task> tasks = new();

            glyphLookup.TryAdd(Black, new ConcurrentDictionary<char, Image<Rgb24>>());
            glyphLookup.TryAdd(White, new ConcurrentDictionary<char, Image<Rgb24>>());

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
                img.Mutate(x => x.Rotate(90));

                if (!glyphLookup.ContainsKey(fg))
                {
                    glyphLookup[fg] = new ConcurrentDictionary<char, Image<Rgb24>>();
                }
                if (!glyphLookup[fg].ContainsKey(c))
                {
                    glyphLookup[fg][c] = img;
                }
            });
        }
    }
}
