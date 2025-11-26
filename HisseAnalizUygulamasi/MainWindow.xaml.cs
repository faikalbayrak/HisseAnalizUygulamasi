using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HisseAnalizUygulamasi.Services;

namespace HisseAnalizUygulamasi
{
    public partial class MainWindow : Window
    {
        private readonly FintablesScraper _scraper;
        private List<string> _favorites;
        private AppSettings _settings;
        private HistoricalDataManager _historyManager;
        private const string FavoritesFileName = "favorites.json";

        public MainWindow()
        {
            InitializeComponent();
            _scraper = new FintablesScraper();
            _favorites = new List<string>();
            _settings = AppSettings.Load();
            _historyManager = new HistoricalDataManager();

            // Temayı uygula
            ThemeManager.ApplyTheme(_settings.Theme);
            UpdateThemeIcon();

            // Tüm inputları boş başlat
            ClearAllFields();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Favorileri yükle
            LoadFavorites();
        }

        private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            // Temayı değiştir
            _settings.Theme = _settings.Theme == "Light" ? "Dark" : "Light";
            _settings.Save();

            ThemeManager.ApplyTheme(_settings.Theme);
            UpdateThemeIcon();

            // Favori butonlarını yeniden oluştur (tema değişikliği için)
            UpdateFavoritesUI();
        }

        private void UpdateThemeIcon()
        {
            BtnToggleTheme.Content = _settings.Theme == "Light" ? "🌙" : "☀️";
            BtnToggleTheme.ToolTip = _settings.Theme == "Light" ? "Koyu Tema" : "Açık Tema";
        }

        private void BtnOpenComparison_Click(object sender, RoutedEventArgs e)
        {
            var comparisonWindow = new ComparisonWindow();
            comparisonWindow.Owner = this;
            comparisonWindow.ShowDialog();
        }

        private void BtnOpenQuarterly_Click(object sender, RoutedEventArgs e)
        {
            var quarterlyWindow = new QuarterlyAnalysisWindow();
            quarterlyWindow.Owner = this;
            quarterlyWindow.ShowDialog();
        }

        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(FavoritesFileName))
                {
                    string json = File.ReadAllText(FavoritesFileName);
                    _favorites = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

                    System.Diagnostics.Debug.WriteLine($"Favoriler yuklendi: {_favorites.Count} adet");
                    UpdateFavoritesUI();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Favori yukleme hatasi: {ex.Message}");
                _favorites = new List<string>();
            }
        }

        private void SaveFavorites()
        {
            try
            {
                string json = JsonSerializer.Serialize(_favorites, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(FavoritesFileName, json);

                System.Diagnostics.Debug.WriteLine($"Favoriler kaydedildi: {_favorites.Count} adet");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Favori kaydetme hatasi: {ex.Message}");
            }
        }

        private void UpdateFavoritesUI()
        {
            FavoritesPanel.Children.Clear();

            if (_favorites.Count == 0)
            {
                txtNoFavorites.Visibility = Visibility.Visible;
                return;
            }

            txtNoFavorites.Visibility = Visibility.Collapsed;

            foreach (var symbol in _favorites)
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 5, 5)
                };

                var btn = new Button
                {
                    Content = symbol,
                    Background = (Brush)Application.Current.Resources["PrimaryBrush"],
                    Foreground = Brushes.White,
                    Padding = new Thickness(15, 5, 15, 5),
                    Margin = new Thickness(0, 0, 2, 0),
                    FontWeight = FontWeights.Bold,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = symbol
                };
                btn.Click += FavoriteButton_Click;

                var btnRemove = new Button
                {
                    Content = "✕",
                    Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    Foreground = Brushes.White,
                    Width = 25,
                    Height = 25,
                    FontWeight = FontWeights.Bold,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = symbol,
                    ToolTip = "Favorilerden Çıkar"
                };
                btnRemove.Click += RemoveFavoriteButton_Click;

                stackPanel.Children.Add(btn);
                stackPanel.Children.Add(btnRemove);
                FavoritesPanel.Children.Add(stackPanel);
            }
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string symbol)
            {
                txtSymbol.Text = symbol;
                await FetchDataForSymbol(symbol);
            }
        }

        private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string symbol)
            {
                var result = MessageBox.Show(
                    $"{symbol} favorilerden çıkarılsın mı?",
                    "Favori Çıkar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _favorites.Remove(symbol);
                    SaveFavorites();
                    UpdateFavoritesUI();
                }
            }
        }

        private void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            string symbol = txtSymbol.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Lütfen bir sembol girin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_favorites.Contains(symbol))
            {
                MessageBox.Show($"{symbol} zaten favorilerde!", "Bilgi",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _favorites.Add(symbol);
            SaveFavorites();
            UpdateFavoritesUI();

            MessageBox.Show($"{symbol} favorilere eklendi!", "Başarılı",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearAllFields()
        {
            txtSymbol.Clear();
            txtCompanyName.Clear();
            txtCurrentPrice.Clear();
            txtTotalResources.Clear();
            txtLongTerm.Clear();
            txtShortTerm.Clear();
            txtEmployeeBenefits.Clear();
            txtPaidCapital.Clear();

            ResultPanel.Visibility = Visibility.Collapsed;
        }

        private async Task FetchDataForSymbol(string symbol)
        {
            try
            {
                BtnFetchData.IsEnabled = false;
                BtnFetchData.Content = "Veriler çekiliyor...";

                System.Diagnostics.Debug.WriteLine($"\n========== WEB'DEN VERI CEKME BASLADI ==========");
                System.Diagnostics.Debug.WriteLine($"Sembol: {symbol}");

                var bilancoData = await _scraper.GetBilancoDataAsync(symbol);

                if (bilancoData == null)
                {
                    MessageBox.Show("Bilanço verileri alınamadı! Lütfen sembolü kontrol edin.",
                        "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string companyName = await _scraper.FetchCompanyName(symbol);
                decimal? currentPrice = await _scraper.GuncelFiyatCek(symbol);

                txtCompanyName.Text = companyName;
                txtCurrentPrice.Text = currentPrice?.ToString("N2") ?? "Bulunamadı";
                txtTotalResources.Text = bilancoData.ToplamKaynaklar.ToString("N0");
                txtLongTerm.Text = bilancoData.UzunVadeliYuk.ToString("N0");
                txtShortTerm.Text = bilancoData.KisaVadeliYuk.ToString("N0");
                txtEmployeeBenefits.Text = bilancoData.CalisanBorclari.ToString("N0");
                txtPaidCapital.Text = bilancoData.OdenmisSermaaye.ToString("N0");

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

                // *** YENİ: Geçmişe kaydet ***
                _historyManager.AddRecord(symbol, companyName, bilancoData,
                    adilDeger, currentPrice, iskontoOrani);

                MessageBox.Show("Veriler başarıyla çekildi ve geçmişe kaydedildi!", "Başarılı",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine("========== VERI CEKME TAMAMLANDI ==========\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"HATA: {ex}");
            }
            finally
            {
                BtnFetchData.IsEnabled = true;
                BtnFetchData.Content = "WEB'DEN GERÇEK VERİ ÇEK";
            }
        }

        private async void BtnFetchData_Click(object sender, RoutedEventArgs e)
        {
            string symbol = txtSymbol.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Lütfen bir hisse sembolü girin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await FetchDataForSymbol(symbol);
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtTotalResources.Text.Replace(".", "").Replace(",", ""),
                    out decimal toplamKaynaklar))
                {
                    MessageBox.Show("Toplam Kaynaklar değeri geçersiz!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtLongTerm.Text.Replace(".", "").Replace(",", ""),
                    out decimal uzunVadeli))
                {
                    MessageBox.Show("Uzun Vadeli Yükümlülükler değeri geçersiz!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtShortTerm.Text.Replace(".", "").Replace(",", ""),
                    out decimal kisaVadeli))
                {
                    MessageBox.Show("Kısa Vadeli Yükümlülükler değeri geçersiz!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtEmployeeBenefits.Text.Replace(".", "").Replace(",", ""),
                    out decimal calisanBorclari))
                {
                    MessageBox.Show("Çalışan Borçları değeri geçersiz!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtPaidCapital.Text.Replace(".", "").Replace(",", ""),
                    out decimal odenmisSermaye))
                {
                    MessageBox.Show("Ödenmiş Sermaye değeri geçersiz!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal? guncelFiyat = null;
                if (decimal.TryParse(txtCurrentPrice.Text.Replace(".", "").Replace(",", "."),
                    out decimal fiyat))
                {
                    guncelFiyat = fiyat;
                }

                decimal adilDeger = (toplamKaynaklar + uzunVadeli + kisaVadeli - calisanBorclari) / odenmisSermaye;

                string iskonto = "Fiyat bilgisi yok";
                string kategori = "Bilinmiyor";
                Color kategoriBg = Colors.Gray;

                if (guncelFiyat.HasValue && guncelFiyat.Value > 0)
                {
                    decimal iskontoOrani = ((adilDeger - guncelFiyat.Value) / guncelFiyat.Value) * 100;
                    iskonto = $"%{iskontoOrani:F2}";

                    if (iskontoOrani < -50)
                    {
                        kategori = "Nötr (<%50 iskonto)";
                        kategoriBg = Colors.LightGray;
                    }
                    else if (iskontoOrani >= -50 && iskontoOrani < 0)
                    {
                        kategori = "Sarı (%50-100 arası)";
                        kategoriBg = Colors.Gold;
                    }
                    else if (iskontoOrani >= 0 && iskontoOrani < 100)
                    {
                        kategori = "Yeşil (%100-200 arası)";
                        kategoriBg = Colors.LightGreen;
                    }
                    else
                    {
                        kategori = "Mavi (>%200)";
                        kategoriBg = Colors.LightBlue;
                    }
                }

                txtAdilDeger.Text = adilDeger.ToString("N2");
                txtIskonto.Text = iskonto;
                txtKategori.Text = kategori;
                KategoriPanel.Background = new SolidColorBrush(kategoriBg);

                ResultPanel.Visibility = Visibility.Visible;

                System.Diagnostics.Debug.WriteLine($"HESAPLAMA SONUCU: Adil Deger={adilDeger:N2}, Iskonto={iskonto}, Kategori={kategori}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hesaplama hatası: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"HESAPLAMA HATASI: {ex}");
            }
        }

        private void BtnOpenHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_historyManager);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }
    }
}