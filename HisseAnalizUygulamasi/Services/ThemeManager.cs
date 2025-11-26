using System.Windows;
using System.Windows.Media;

namespace HisseAnalizUygulamasi.Services
{
    public static class ThemeManager
    {
        public static void ApplyTheme(string theme)
        {
            var resources = Application.Current.Resources;

            if (theme == "Dark")
            {
                // Koyu Tema
                resources["PrimaryColor"] = Color.FromRgb(100, 181, 246); // Açık Mavi
                resources["SecondaryColor"] = Color.FromRgb(129, 199, 132); // Açık Yeşil
                resources["AccentColor"] = Color.FromRgb(255, 213, 79); // Açık Sarı
                resources["BackgroundColor"] = Color.FromRgb(18, 18, 18); // Koyu Siyah
                resources["SurfaceColor"] = Color.FromRgb(30, 30, 30); // Koyu Gri
                resources["TextPrimaryColor"] = Color.FromRgb(255, 255, 255); // Beyaz
                resources["TextSecondaryColor"] = Color.FromRgb(189, 189, 189); // Açık Gri
                resources["BorderColor"] = Color.FromRgb(66, 66, 66); // Orta Gri
                resources["InputBackgroundColor"] = Color.FromRgb(45, 45, 45); // Koyu Gri
            }
            else
            {
                // Açık Tema
                resources["PrimaryColor"] = Color.FromRgb(33, 150, 243); // Mavi
                resources["SecondaryColor"] = Color.FromRgb(76, 175, 80); // Yeşil
                resources["AccentColor"] = Color.FromRgb(255, 193, 7); // Sarı
                resources["BackgroundColor"] = Color.FromRgb(245, 245, 245); // Açık Gri
                resources["SurfaceColor"] = Color.FromRgb(255, 255, 255); // Beyaz
                resources["TextPrimaryColor"] = Color.FromRgb(33, 33, 33); // Siyah
                resources["TextSecondaryColor"] = Color.FromRgb(117, 117, 117); // Gri
                resources["BorderColor"] = Color.FromRgb(224, 224, 224); // Açık Gri
                resources["InputBackgroundColor"] = Color.FromRgb(224, 224, 224); // Açık Gri
            }

            // Brush'ları güncelle
            resources["PrimaryBrush"] = new SolidColorBrush((Color)resources["PrimaryColor"]);
            resources["SecondaryBrush"] = new SolidColorBrush((Color)resources["SecondaryColor"]);
            resources["AccentBrush"] = new SolidColorBrush((Color)resources["AccentColor"]);
            resources["BackgroundBrush"] = new SolidColorBrush((Color)resources["BackgroundColor"]);
            resources["SurfaceBrush"] = new SolidColorBrush((Color)resources["SurfaceColor"]);
            resources["TextPrimaryBrush"] = new SolidColorBrush((Color)resources["TextPrimaryColor"]);
            resources["TextSecondaryBrush"] = new SolidColorBrush((Color)resources["TextSecondaryColor"]);
            resources["BorderBrush"] = new SolidColorBrush((Color)resources["BorderColor"]);
            resources["InputBackgroundBrush"] = new SolidColorBrush((Color)resources["InputBackgroundColor"]);
        }
    }
}