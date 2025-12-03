using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HisseAnalizUygulamasi.Services
{
    public class BilancoData
    {
        public decimal ToplamKaynaklar { get; set; }
        public decimal UzunVadeliYuk { get; set; }
        public decimal KisaVadeliYuk { get; set; }
        public decimal CalisanBorclari { get; set; }
        public decimal OdenmisSermaaye { get; set; }
    }

    public class FintablesScraper
    {
        private readonly HttpClient _client;

        public FintablesScraper()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<BilancoData> GetBilancoDataAsync(string symbol)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"\n=== BILANCO VERISI CEKILIYOR: {symbol} ===");

                string url = $"https://fintables.com/sirketler/{symbol}/finansal-tablolar/bilanco";
                System.Diagnostics.Debug.WriteLine($"URL: {url}");

                var response = await _client.GetStringAsync(url);
                System.Diagnostics.Debug.WriteLine($"HTML uzunlugu: {response.Length} karakter");

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                await Task.Delay(2000); // Rate limiting

                // Yardımcı fonksiyon - bilanço değeri çeker (ALTERNATİF İSİMLER İLE)
                decimal? GetBalanceValue(string itemName, string sectionHeader = null)
                {
                    HtmlNode row = null;

                    // Alternatif isimler listesi
                    var alternativeNames = new List<string> { itemName };

                    if (itemName == "Toplam Kaynaklar")
                    {
                        alternativeNames.Add("Toplam Aktifler");
                        alternativeNames.Add("Toplam Varlıklar");
                        alternativeNames.Add("Aktif Toplamı");
                    }
                    else if (itemName == "Toplam Uzun Vadeli Yükümlülükler")
                    {
                        alternativeNames.Add("Uzun Vadeli Yükümlülükler");
                        alternativeNames.Add("Uzun Vadeli Borçlar");
                    }
                    else if (itemName == "Toplam Kısa Vadeli Yükümlülükler")
                    {
                        alternativeNames.Add("Kısa Vadeli Yükümlülükler");
                        alternativeNames.Add("Kısa Vadeli Borçlar");
                    }

                    // Tüm alternatif isimleri dene
                    foreach (var name in alternativeNames)
                    {
                        if (sectionHeader != null)
                        {
                            var section = doc.DocumentNode.SelectSingleNode(
                                $"//tbody[.//td[contains(text(), '{sectionHeader}')]]");

                            if (section != null)
                            {
                                row = section.SelectSingleNode(
                                    $".//tr[.//div[contains(text(), '{name}')]]");

                                if (row != null) break;
                            }
                        }
                        else
                        {
                            row = doc.DocumentNode.SelectSingleNode(
                                $"//tr[.//div[contains(text(), '{name}')]]");

                            if (row != null) break;
                        }
                    }

                    if (row == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"BULUNAMADI (row): {itemName} - Tum alternatifler denendi");
                        return null;
                    }

                    var valueSpan = row.SelectSingleNode(
                        ".//td[2]//span[contains(@class, 'tabular-nums') and not(ancestor::*[contains(@class, 'absolute')])]");

                    if (valueSpan == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"VALUE SPAN BULUNAMADI: {itemName}");
                        return null;
                    }

                    string valueText = valueSpan.InnerText.Trim();
                    System.Diagnostics.Debug.WriteLine($"✓ BULUNDU: {itemName} = {valueText}");

                    return ParseAmount(valueText);
                }

                // *** YENİ: TÜM VERİLERİ ÇEK - BULUNAMAYANLAR 0 ***
                var toplamKaynaklar = GetBalanceValue("Toplam Kaynaklar") ?? 0;
                var uzunVadeliYuk = GetBalanceValue("Toplam Uzun Vadeli Yükümlülükler") ?? 0;
                var kisaVadeliYuk = GetBalanceValue("Toplam Kısa Vadeli Yükümlülükler") ?? 0;

                var calisanKisa = GetBalanceValue("Çalışanlara Sağlanan Faydalar",
                    "Kısa Vadeli Yükümlülükler") ?? 0;
                var calisanUzun = GetBalanceValue("Çalışanlara Sağlanan Faydalar",
                    "Uzun Vadeli Yükümlülükler") ?? 0;
                var calisanBorclari = calisanKisa + calisanUzun;

                var odenmisSermaye = GetBalanceValue("Ödenmiş Sermaye") ?? 0;

                System.Diagnostics.Debug.WriteLine($"\nCalisan Borclari: Kisa={calisanKisa:N0}, Uzun={calisanUzun:N0}, Toplam={calisanBorclari:N0}");

                // *** YENİ: SADECE BÖLEN 0 İSE HATA VER ***
                if (odenmisSermaye == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⛔ HATA: Ödenmiş Sermaye 0 veya bulunamadı! Hesaplama yapılamaz.");
                    return null;
                }

                // Toplam Kaynaklar 0 ise uyarı ver ama devam et
                if (toplamKaynaklar == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ UYARI: Toplam Kaynaklar 0 veya bulunamadı!");
                }

                // Özet log
                System.Diagnostics.Debug.WriteLine("\n=== BILANCO OZETI ===");
                System.Diagnostics.Debug.WriteLine($"Toplam Kaynaklar: {toplamKaynaklar:N0} {(toplamKaynaklar == 0 ? "⚠️ (0 kabul edildi)" : "✓")}");
                System.Diagnostics.Debug.WriteLine($"Uzun Vadeli Yük: {uzunVadeliYuk:N0} {(uzunVadeliYuk == 0 ? "⚠️ (0 kabul edildi)" : "✓")}");
                System.Diagnostics.Debug.WriteLine($"Kısa Vadeli Yük: {kisaVadeliYuk:N0} {(kisaVadeliYuk == 0 ? "⚠️ (0 kabul edildi)" : "✓")}");
                System.Diagnostics.Debug.WriteLine($"Çalışan Borçları: {calisanBorclari:N0} {(calisanBorclari == 0 ? "⚠️ (0 kabul edildi)" : "✓")}");
                System.Diagnostics.Debug.WriteLine($"Ödenmiş Sermaye: {odenmisSermaye:N0} ✓");
                System.Diagnostics.Debug.WriteLine("=== BILANCO VERILERI CEKILDI ===\n");

                return new BilancoData
                {
                    ToplamKaynaklar = toplamKaynaklar,
                    UzunVadeliYuk = uzunVadeliYuk,
                    KisaVadeliYuk = kisaVadeliYuk,
                    CalisanBorclari = calisanBorclari,
                    OdenmisSermaaye = odenmisSermaye
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⛔ BILANCO CEKME HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        private decimal? ParseAmount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                text = text.Replace(".", "").Replace(",", "").Replace(" ", "").Trim();

                if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result * 1;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> FetchCompanyName(string symbol)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"\n=== SIRKET ADI CEKILIYOR: {symbol} ===");

                string url = $"https://fintables.com/sirketler/{symbol}";
                System.Diagnostics.Debug.WriteLine($"URL: {url}");

                var response = await _client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                await Task.Delay(1000); // Rate limiting

                var h3Node = doc.DocumentNode.SelectSingleNode(
                    "//h3[contains(text(), 'hisse fiyatı bugün kaç TL?')]");

                if (h3Node != null)
                {
                    string h3Text = h3Node.InnerText.Trim();
                    System.Diagnostics.Debug.WriteLine($"h3 text: {h3Text}");

                    int parenIndex = h3Text.IndexOf('(');
                    if (parenIndex > 0)
                    {
                        string companyName = h3Text.Substring(0, parenIndex).Trim();
                        System.Diagnostics.Debug.WriteLine($"Sirket Adi: {companyName}");
                        return companyName;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Sirket adi bulunamadi!");
                return symbol;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SIRKET ADI CEKME HATASI: {ex.Message}");
                return symbol;
            }
        }

        public async Task<decimal?> GuncelFiyatCek(string symbol)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"\n=== GUNCEL FIYAT CEKILIYOR: {symbol} ===");

                string url = $"https://fintables.com/sirketler/{symbol}";
                System.Diagnostics.Debug.WriteLine($"URL: {url}");

                var response = await _client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                await Task.Delay(1000); // Rate limiting

                var priceDiv = doc.DocumentNode.SelectSingleNode(
                    "//div[starts-with(text(), 'Bugün itibarıyla') or contains(., 'Bugün itibarıyla')]");

                if (priceDiv != null)
                {
                    string fullText = priceDiv.InnerText;
                    System.Diagnostics.Debug.WriteLine($"Price div text: {fullText}");

                    var match = Regex.Match(fullText, @"hissesinin fiyatı\s+([\d,]+)\s+TL");

                    if (!match.Success)
                    {
                        match = Regex.Match(fullText, @"fiyatı\s+([\d,]+)\s+TL");
                    }

                    if (!match.Success)
                    {
                        match = Regex.Match(fullText, @"([\d,]+)\s+TL'dir");
                    }

                    if (!match.Success)
                    {
                        match = Regex.Match(fullText, @"(\d+,\d+)");
                    }

                    if (match.Success)
                    {
                        string priceText = match.Groups[1].Value.Trim();
                        System.Diagnostics.Debug.WriteLine($"Regex match: {priceText}");

                        priceText = priceText.Replace(",", ".");

                        if (decimal.TryParse(priceText, NumberStyles.Any,
                            CultureInfo.InvariantCulture, out decimal price))
                        {
                            System.Diagnostics.Debug.WriteLine($"Guncel Fiyat: {price:N2} TL");
                            return price;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Parse edilemedi: {priceText}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Regex match bulunamadi!");
                        System.Diagnostics.Debug.WriteLine($"Aranan text: {fullText}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Price div bulunamadi!");
                }

                System.Diagnostics.Debug.WriteLine("Fiyat bulunamadi!");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FIYAT CEKME HATASI: {ex.Message}");
                return null;
            }
        }
    }
}