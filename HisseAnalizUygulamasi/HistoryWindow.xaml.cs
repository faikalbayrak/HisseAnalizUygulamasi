using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HisseAnalizUygulamasi.Services;

namespace HisseAnalizUygulamasi
{
    public partial class HistoryWindow : Window
    {
        private readonly HistoricalDataManager _historyManager;

        public HistoryWindow(HistoricalDataManager historyManager)
        {
            InitializeComponent();
            _historyManager = historyManager;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSymbols();
        }

        private void LoadSymbols()
        {
            var symbols = _historyManager.GetAllSymbols();

            cmbSymbols.Items.Clear();

            if (symbols.Count == 0)
            {
                MessageBox.Show("Henüz hiç kayıt yok! Veri çektikten sonra geçmiş oluşacak.",
                    "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var symbol in symbols)
            {
                int count = _historyManager.GetRecordCount(symbol);
                cmbSymbols.Items.Add($"{symbol} ({count} kayıt)");
            }

            if (cmbSymbols.Items.Count > 0)
            {
                cmbSymbols.SelectedIndex = 0;
            }
        }

        private void CmbSymbols_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbSymbols.SelectedItem == null) return;

            string selectedText = cmbSymbols.SelectedItem.ToString();
            string symbol = selectedText.Split(' ')[0];

            LoadRecords(symbol);
        }

        private void LoadRecords(string symbol)
        {
            var records = _historyManager.GetRecords(symbol);

            if (records.Count == 0)
            {
                TrendPanel.Visibility = Visibility.Collapsed;
                HistoryDataGrid.ItemsSource = null;
                return;
            }

            string trendAnalysis = _historyManager.GetTrendAnalysis(symbol);
            txtTrend.Text = trendAnalysis;
            txtRecordCount.Text = $"Toplam {records.Count} kayıt bulundu";
            TrendPanel.Visibility = Visibility.Visible;

            var displayRecords = records.Select(r => new
            {
                TarihStr = r.Tarih.ToString("dd.MM.yyyy HH:mm"),
                r.SirketAdi,
                AdilDegerStr = r.AdilDeger.ToString("N2") + " TL",
                GuncelFiyatStr = r.GuncelFiyat.HasValue ? r.GuncelFiyat.Value.ToString("N2") + " TL" : "Yok",
                IskontoStr = r.IskontoOrani.HasValue ? "%" + r.IskontoOrani.Value.ToString("N2") : "Yok",
                ToplamKaynaklarStr = FormatNumber(r.ToplamKaynaklar),
                Record = r
            }).ToList();

            HistoryDataGrid.ItemsSource = displayRecords;
        }

        private string FormatNumber(decimal number)
        {
            if (number >= 1_000_000_000)
                return $"{(number / 1_000_000_000):N2}M";
            else if (number >= 1_000_000)
                return $"{(number / 1_000_000):N2}m";
            else
                return $"{number:N0}";
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSymbols();
            MessageBox.Show("Geçmiş yenilendi!", "Bilgi",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDeleteSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSymbols.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir sembol seçin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedText = cmbSymbols.SelectedItem.ToString();
            string symbol = selectedText.Split(' ')[0];

            var result = MessageBox.Show(
                $"{symbol} sembolüne ait tüm kayıtlar silinsin mi?",
                "Onay",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _historyManager.DeleteRecords(symbol);
                LoadSymbols();
                HistoryDataGrid.ItemsSource = null;
                TrendPanel.Visibility = Visibility.Collapsed;

                MessageBox.Show($"{symbol} kayıtları silindi!", "Başarılı",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "TÜM GEÇMİŞ KAYITLAR SİLİNSİN Mİ?\n\nBu işlem geri alınamaz!",
                "DİKKAT!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var confirm = MessageBox.Show(
                    "EMİN MİSİNİZ? Tüm veriler kaybolacak!",
                    "SON ONAY",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                if (confirm == MessageBoxResult.Yes)
                {
                    _historyManager.ClearAllRecords();
                    LoadSymbols();
                    HistoryDataGrid.ItemsSource = null;
                    TrendPanel.Visibility = Visibility.Collapsed;

                    MessageBox.Show("Tüm geçmiş silindi!", "Başarılı",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HistoryDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem == null) return;

            dynamic selectedItem = HistoryDataGrid.SelectedItem;
            HistoricalRecord record = selectedItem.Record;

            string detay = $"📊 DETAYLI KAYIT\n\n" +
                           $"Tarih: {record.Tarih:dd.MM.yyyy HH:mm:ss}\n" +
                           $"Şirket: {record.SirketAdi} ({record.Sembol})\n\n" +
                           $"📈 FİNANSAL VERİLER:\n" +
                           $"• Toplam Kaynaklar: {record.ToplamKaynaklar:N0} Bin TL\n" +
                           $"• Uzun Vadeli Yük: {record.UzunVadeliYuk:N0} Bin TL\n" +
                           $"• Kısa Vadeli Yük: {record.KisaVadeliYuk:N0} Bin TL\n" +
                           $"• Çalışan Borçları: {record.CalisanBorclari:N0} Bin TL\n" +
                           $"• Ödenmiş Sermaye: {record.OdenmisSermaaye:N0} Bin TL\n\n" +
                           $"💰 DEĞERLEME:\n" +
                           $"• Adil Değer: {record.AdilDeger:N2} TL\n" +
                           $"• Güncel Fiyat: {(record.GuncelFiyat.HasValue ? record.GuncelFiyat.Value.ToString("N2") + " TL" : "Bulunamadı")}\n" +
                           $"• İskonto Oranı: {(record.IskontoOrani.HasValue ? "%" + record.IskontoOrani.Value.ToString("N2") : "Hesaplanamadı")}";

            MessageBox.Show(detay, "Kayıt Detayı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}