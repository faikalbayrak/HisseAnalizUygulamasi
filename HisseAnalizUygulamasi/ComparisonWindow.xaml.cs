using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HisseAnalizUygulamasi.Services;

namespace HisseAnalizUygulamasi
{
    public partial class ComparisonWindow : Window
    {
        private readonly FintablesScraper _scraper;
        private ComparisonData _data1;
        private ComparisonData _data2;

        public ComparisonWindow()
        {
            InitializeComponent();
            _scraper = new FintablesScraper();
        }

        private async void BtnFetch1_Click(object sender, RoutedEventArgs e)
        {
            string symbol = txtSymbol1.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Lütfen 1. hisse sembolünü girin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                BtnFetch1.IsEnabled = false;
                BtnFetch1.Content = "Yükleniyor...";

                _data1 = await FetchComparisonData(symbol);

                if (_data1 != null)
                {
                    MessageBox.Show($"{symbol} verileri yüklendi!", "Başarılı",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Her ikisi de yüklendiysе karşılaştır
                    if (_data2 != null)
                    {
                        ShowComparison();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnFetch1.IsEnabled = true;
                BtnFetch1.Content = "VERİ ÇEK";
            }
        }

        private async void BtnFetch2_Click(object sender, RoutedEventArgs e)
        {
            string symbol = txtSymbol2.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Lütfen 2. hisse sembolünü girin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                BtnFetch2.IsEnabled = false;
                BtnFetch2.Content = "Yükleniyor...";

                _data2 = await FetchComparisonData(symbol);

                if (_data2 != null)
                {
                    MessageBox.Show($"{symbol} verileri yüklendi!", "Başarılı",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Her ikisi de yüklendiysе karşılaştır
                    if (_data1 != null)
                    {
                        ShowComparison();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnFetch2.IsEnabled = true;
                BtnFetch2.Content = "VERİ ÇEK";
            }
        }

        private async System.Threading.Tasks.Task<ComparisonData> FetchComparisonData(string symbol)
        {
            // Bilanço verilerini çek
            var bilancoData = await _scraper.GetBilancoDataAsync(symbol);
            if (bilancoData == null) return null;

            // Şirket adını çek
            string companyName = await _scraper.FetchCompanyName(symbol);

            // Güncel fiyatı çek
            decimal? currentPrice = await _scraper.GuncelFiyatCek(symbol);

            // Adil değer hesapla
            decimal adilDeger = (bilancoData.ToplamKaynaklar + bilancoData.UzunVadeliYuk +
                                 bilancoData.KisaVadeliYuk - bilancoData.CalisanBorclari) /
                                 bilancoData.OdenmisSermaaye;

            // İskonto hesapla
            decimal? iskontoOrani = null;
            if (currentPrice.HasValue && currentPrice.Value > 0)
            {
                iskontoOrani = ((adilDeger - currentPrice.Value) / currentPrice.Value) * 100;
            }

            return new ComparisonData
            {
                Symbol = symbol,
                CompanyName = companyName,
                CurrentPrice = currentPrice,
                AdilDeger = adilDeger,
                IskontoOrani = iskontoOrani,
                ToplamKaynaklar = bilancoData.ToplamKaynaklar,
                UzunVadeliYuk = bilancoData.UzunVadeliYuk,
                KisaVadeliYuk = bilancoData.KisaVadeliYuk,
                CalisanBorclari = bilancoData.CalisanBorclari,
                OdenmisSermaaye = bilancoData.OdenmisSermaaye
            };
        }

        private void ShowComparison()
        {
            ComparisonPanel.Visibility = Visibility.Visible;

            // Şirket adlarını göster
            txtCompanyName1.Text = $"{_data1.CompanyName}\n({_data1.Symbol})";
            txtCompanyName2.Text = $"{_data2.CompanyName}\n({_data2.Symbol})";

            // Karşılaştırma satırlarını temizle
            ComparisonRows.Children.Clear();

            // Satırları ekle
            AddComparisonRow("Güncel Fiyat",
                _data1.CurrentPrice?.ToString("N2") + " TL",
                _data2.CurrentPrice?.ToString("N2") + " TL",
                _data1.CurrentPrice < _data2.CurrentPrice ? 1 : 2);

            AddComparisonRow("Adil Değer",
                _data1.AdilDeger.ToString("N2") + " TL",
                _data2.AdilDeger.ToString("N2") + " TL",
                _data1.AdilDeger > _data2.AdilDeger ? 1 : 2);

            AddComparisonRow("İskonto Oranı",
                _data1.IskontoOrani?.ToString("N2") + "%",
                _data2.IskontoOrani?.ToString("N2") + "%",
                _data1.IskontoOrani > _data2.IskontoOrani ? 1 : 2);

            AddComparisonRow("Toplam Kaynaklar",
                FormatLargeNumber(_data1.ToplamKaynaklar),
                FormatLargeNumber(_data2.ToplamKaynaklar),
                _data1.ToplamKaynaklar > _data2.ToplamKaynaklar ? 1 : 2);

            AddComparisonRow("Uzun Vadeli Yük",
                FormatLargeNumber(_data1.UzunVadeliYuk),
                FormatLargeNumber(_data2.UzunVadeliYuk),
                _data1.UzunVadeliYuk < _data2.UzunVadeliYuk ? 1 : 2);

            AddComparisonRow("Kısa Vadeli Yük",
                FormatLargeNumber(_data1.KisaVadeliYuk),
                FormatLargeNumber(_data2.KisaVadeliYuk),
                _data1.KisaVadeliYuk < _data2.KisaVadeliYuk ? 1 : 2);

            AddComparisonRow("Ödenmiş Sermaye",
                FormatLargeNumber(_data1.OdenmisSermaaye),
                FormatLargeNumber(_data2.OdenmisSermaaye),
                _data1.OdenmisSermaaye > _data2.OdenmisSermaaye ? 1 : 2);

            // Kazanan belirle
            DetermineWinner();
        }

        private void AddComparisonRow(string label, string value1, string value2, int winner)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Sol değer
            var border1 = new Border
            {
                Background = winner == 1 ? new SolidColorBrush(Color.FromRgb(200, 230, 201)) : Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10)
            };

            var text1 = new TextBlock
            {
                Text = value1,
                FontSize = 14,
                FontWeight = winner == 1 ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = TextAlignment.Center
            };

            border1.Child = text1;
            Grid.SetColumn(border1, 0);

            // Label
            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(labelText, 1);

            // Sağ değer
            var border2 = new Border
            {
                Background = winner == 2 ? new SolidColorBrush(Color.FromRgb(200, 230, 201)) : Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10)
            };

            var text2 = new TextBlock
            {
                Text = value2,
                FontSize = 14,
                FontWeight = winner == 2 ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = TextAlignment.Center
            };

            border2.Child = text2;
            Grid.SetColumn(border2, 2);

            grid.Children.Add(border1);
            grid.Children.Add(labelText);
            grid.Children.Add(border2);

            ComparisonRows.Children.Add(grid);
        }

        private void DetermineWinner()
        {
            int score1 = 0;
            int score2 = 0;

            // İskonto oranı en önemli kriter
            if (_data1.IskontoOrani.HasValue && _data2.IskontoOrani.HasValue)
            {
                if (_data1.IskontoOrani > _data2.IskontoOrani)
                    score1 += 3;
                else
                    score2 += 3;
            }

            // Adil değer / Güncel fiyat oranı
            decimal ratio1 = _data1.CurrentPrice.HasValue && _data1.CurrentPrice.Value > 0
                ? _data1.AdilDeger / _data1.CurrentPrice.Value : 0;
            decimal ratio2 = _data2.CurrentPrice.HasValue && _data2.CurrentPrice.Value > 0
                ? _data2.AdilDeger / _data2.CurrentPrice.Value : 0;

            if (ratio1 > ratio2)
                score1 += 2;
            else
                score2 += 2;

            // Toplam kaynaklar
            if (_data1.ToplamKaynaklar > _data2.ToplamKaynaklar)
                score1++;
            else
                score2++;

            // Sonuç
            if (score1 > score2)
            {
                txtWinner.Text = $"🏆 {_data1.Symbol}";
                txtWinner.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                txtWinnerReason.Text = $"{_data1.Symbol}, daha yüksek iskonto oranı ve daha iyi değerleme ile öne çıkıyor.";
            }
            else if (score2 > score1)
            {
                txtWinner.Text = $"🏆 {_data2.Symbol}";
                txtWinner.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                txtWinnerReason.Text = $"{_data2.Symbol}, daha yüksek iskonto oranı ve daha iyi değerleme ile öne çıkıyor.";
            }
            else
            {
                txtWinner.Text = "⚖️ BERABERE";
                txtWinner.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                txtWinnerReason.Text = "Her iki hisse de benzer performans gösteriyor.";
            }
        }

        private string FormatLargeNumber(decimal number)
        {
            if (number >= 1_000_000_000)
                return $"{(number / 1_000_000_000):N2} Milyar TL";
            else if (number >= 1_000_000)
                return $"{(number / 1_000_000):N2} Milyon TL";
            else
                return $"{number:N0} TL";
        }
    }

    // Veri sınıfı
    public class ComparisonData
    {
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal AdilDeger { get; set; }
        public decimal? IskontoOrani { get; set; }
        public decimal ToplamKaynaklar { get; set; }
        public decimal UzunVadeliYuk { get; set; }
        public decimal KisaVadeliYuk { get; set; }
        public decimal CalisanBorclari { get; set; }
        public decimal OdenmisSermaaye { get; set; }
    }
}