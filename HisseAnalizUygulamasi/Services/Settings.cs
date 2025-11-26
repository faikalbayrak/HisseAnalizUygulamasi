using System;
using System.IO;
using System.Text.Json;

namespace HisseAnalizUygulamasi.Services
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";

        private const string SettingsFileName = "settings.json";

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    string json = File.ReadAllText(SettingsFileName);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings yukleme hatasi: {ex.Message}");
            }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SettingsFileName, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings kaydetme hatasi: {ex.Message}");
            }
        }
    }
}