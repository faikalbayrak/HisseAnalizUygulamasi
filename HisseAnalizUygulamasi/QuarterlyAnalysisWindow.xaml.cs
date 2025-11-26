using System;
using System.Collections.Generic;
using System.Windows;
using HisseAnalizUygulamasi.Services;

namespace HisseAnalizUygulamasi
{
    public partial class QuarterlyAnalysisWindow : Window
    {
        private readonly FintablesScraper _scraper;

        public QuarterlyAnalysisWindow()
        {
            InitializeComponent();
            _scraper = new FintablesScraper();
        }

        private async void BtnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            string symbol = txtSymbol.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Lütfen bir hisse sembolü girin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                BtnAnalyze.IsEnabled = false;
                LoadingPanel.Visibility = Visibility.Visible;
                DataPanel.Visibility = Visibility.Collapsed;

                string companyName = await _scraper.FetchCompanyName(symbol);
                txtCompanyName.Text = $"{companyName} ({symbol})";

                var bilancoData = await _scraper.GetBilancoDataAsync(symbol);

                if (bilancoData == null)
                {
                    MessageBox.Show("Bilanço verileri alınamadı!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal adilDeger = (bilancoData.ToplamKaynaklar + bilancoData.UzunVadeliYuk +
                                     bilancoData.KisaVadeliYuk - bilancoData.CalisanBorclari) /
                                     bilancoData.OdenmisSermaaye;

                var quarterlyData = new List<QuarterlyData>
                {
                    new QuarterlyData
                    {
                        Period = "2025 Ç3",
                        AdilDeger = adilDeger.ToString("N2") + " TL"
                    }
                };

                QuarterlyDataGrid.ItemsSource = quarterlyData;

                LoadingPanel.Visibility = Visibility.Collapsed;
                DataPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnAnalyze.IsEnabled = true;
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class QuarterlyData
    {
        public string Period { get; set; }
        public string AdilDeger { get; set; }
    }
}