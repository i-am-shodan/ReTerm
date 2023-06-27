using System.IO;
using SixLabors.Fonts;

namespace ReTerm.Fonts
{
    public class Fonts
    {
        public static readonly FontFamily SegoeUi;
        public static readonly FontFamily UbuntuMonoSpaced;

        static Fonts()
        {
            var fonts = new FontCollection();

            InstallFont(fonts, EmbeddedFonts.segoeui); // Segoe UI
            InstallFont(fonts, EmbeddedFonts.Ubuntu_Mono_derivative_Powerline); // Segoe UI

            SegoeUi = fonts.Find("Segoe UI");
            UbuntuMonoSpaced = fonts.Find("Ubuntu Mono derivative Powerline");

        }

        private static void InstallFont(IFontCollection fonts, byte[] font)
        {
            using var ms = new MemoryStream(font);
            fonts.Install(ms);
        }
    }
}
