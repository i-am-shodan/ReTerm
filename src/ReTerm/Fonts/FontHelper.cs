using System.IO;
using System.Linq;
using SixLabors.Fonts;

namespace ReTerm.Fonts
{
    public class FontHelper
    {
        public static FontFamily SegoeUi;
        public static FontFamily Default;

        public static void Init(string defaultFontOverride = "", string defaultFontOverrideName = "")
        {
            var fonts = new FontCollection();

            InstallFont(fonts, EmbeddedFonts.segoeui); // Segoe UI
            InstallFont(fonts, EmbeddedFonts.Ubuntu_Mono_derivative_Powerline); // Segoe UI

            SegoeUi = fonts.Find("Segoe UI");

            if (string.IsNullOrWhiteSpace(defaultFontOverride))
            {
                Default = fonts.Find("Ubuntu Mono derivative Powerline");
            }
            else
            {
                InstallFont(fonts, File.ReadAllBytes(defaultFontOverride));

                if (string.IsNullOrWhiteSpace(defaultFontOverrideName)) {
                    var otherFonts = fonts.Families.Where(x => x.Name != "Segoe UI" && x.Name != "Ubuntu Mono derivative Powerline");
                    if (otherFonts.Any())
                    {
                        defaultFontOverrideName = otherFonts.Single().Name;
                    }
                }

                Default = fonts.Find(defaultFontOverrideName);
            }
        }

        private static void InstallFont(IFontCollection fonts, byte[] font)
        {
            using var ms = new MemoryStream(font);
            fonts.Install(ms);
        }
    }
}
