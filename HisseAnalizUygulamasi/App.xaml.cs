using AutoUpdaterDotNET;
using System;
using System.Windows;

namespace HisseAnalizUygulamasi
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ========================================
            // OTOMATIK GÜNCELLEME SİSTEMİ
            // ========================================

            // Güncelleme ayarları
            AutoUpdater.ShowSkipButton = false;              // "Atla" butonu gösterme
            AutoUpdater.Mandatory = false;                   // Zorunlu güncelleme değil
            AutoUpdater.UpdateMode = Mode.ForcedDownload;    // Otomatik indir
            AutoUpdater.ShowRemindLaterButton = true;        // "Sonra Hatırlat" butonu

            // Güncelleme kontrolü yap
            // NOT: [GITHUB_USERNAME] kısmını kendi GitHub kullanıcı adınla değiştir!
            AutoUpdater.Start("https://raw.githubusercontent.com/faikalbayrak/HisseAnalizUygulamasi/main/update.xml");
        }
    }
}