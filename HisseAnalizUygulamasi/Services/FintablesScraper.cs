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

                await Task.Delay(1000); // Rate limiting

                // Yardımcı fonksiyon - bilanço değeri çeker
                decimal? GetBalanceValue(string itemName, string sectionHeader = null)
                {
                    HtmlNode row = null;

                    if (sectionHeader != null)
                    {
                        // Belirli bölüm içinde ara
                        var section = doc.DocumentNode.SelectSingleNode(
                            $"//tbody[.//td[contains(text(), '{sectionHeader}')]]");

                        if (section != null)
                        {
                            row = section.SelectSingleNode(
                                $".//tr[.//div[contains(text(), '{itemName}')]]");
                        }
                    }
                    else
                    {
                        // Tüm tabloda ara
                        row = doc.DocumentNode.SelectSingleNode(
                            $"//tr[.//div[contains(text(), '{itemName}')]]");
                    }

                    if (row == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"BULUNAMADI (row): {itemName}");
                        return null;
                    }

                    // İKİNCİ td içinde "absolute" class'ı OLMAYAN tabular-nums span'ini al
                    var valueSpan = row.SelectSingleNode(
                        ".//td[2]//span[contains(@class, 'tabular-nums') and not(ancestor::*[contains(@class, 'absolute')])]");

                    if (valueSpan == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"VALUE SPAN BULUNAMADI: {itemName}");
                        return null;
                    }

                    string valueText = valueSpan.InnerText.Trim();
                    System.Diagnostics.Debug.WriteLine($"BULUNDU: {itemName} = {valueText}");

                    return ParseAmount(valueText);
                }

                // Verileri çek
                var toplamKaynaklar = GetBalanceValue("Toplam Kaynaklar");
                var uzunVadeliYuk = GetBalanceValue("Toplam Uzun Vadeli Yükümlülükler");
                var kisaVadeliYuk = GetBalanceValue("Toplam Kısa Vadeli Yükümlülükler");

                // Çalışan borçları - hem kısa hem uzun vadeli topla
                var calisanKisa = GetBalanceValue("Çalışanlara Sağlanan Faydalar",
                    "Kısa Vadeli Yükümlülükler") ?? 0;
                var calisanUzun = GetBalanceValue("Çalışanlara Sağlanan Faydalar",
                    "Uzun Vadeli Yükümlülükler") ?? 0;
                var calisanBorclari = calisanKisa + calisanUzun;

                System.Diagnostics.Debug.WriteLine(
                    $"Calisan Borclari Detay: Kisa={calisanKisa}, Uzun={calisanUzun}, Toplam={calisanBorclari}");

                var odenmisSermaye = GetBalanceValue("Ödenmiş Sermaye");

                // Kontrol
                if (!toplamKaynaklar.HasValue || !uzunVadeliYuk.HasValue ||
                    !kisaVadeliYuk.HasValue || !odenmisSermaye.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("HATA: Bazi degerler bulunamadi!");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine("=== BILANCO VERILERI BASARIYLA CEKILDI ===\n");

                return new BilancoData
                {
                    ToplamKaynaklar = toplamKaynaklar.Value,
                    UzunVadeliYuk = uzunVadeliYuk.Value,
                    KisaVadeliYuk = kisaVadeliYuk.Value,
                    CalisanBorclari = calisanBorclari,
                    OdenmisSermaaye = odenmisSermaye.Value
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BILANCO CEKME HATASI: {ex.Message}");
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
                // "412.436.354" formatından "412436354" yap
                text = text.Replace(".", "").Replace(",", "").Replace(" ", "").Trim();

                if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    // Fintables "Bin TRY" cinsinden veri veriyor
                    return result * 1000;
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

                // "hisse fiyatı bugün kaç TL?" içeren h3'ü bul
                var h3Node = doc.DocumentNode.SelectSingleNode(
                    "//h3[contains(text(), 'hisse fiyatı bugün kaç TL?')]");

                if (h3Node != null)
                {
                    string h3Text = h3Node.InnerText.Trim();
                    System.Diagnostics.Debug.WriteLine($"h3 text: {h3Text}");

                    // "Ard Grup Bilişim Teknolojileri A.Ş (Borsa Kodu: ARDYZ) hisse fiyatı bugün kaç TL?"
                    // Parantez öncesi kısmı al
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

                // "Bugün itibarıyla" ile başlayan div'i bul
                var priceDiv = doc.DocumentNode.SelectSingleNode(
                    "//div[starts-with(text(), 'Bugün itibarıyla') or contains(., 'Bugün itibarıyla')]");

                if (priceDiv != null)
                {
                    string fullText = priceDiv.InnerText;
                    System.Diagnostics.Debug.WriteLine($"Price div text: {fullText}");

                    // Farklı pattern'ler dene

                    // Pattern 1: "hissesinin fiyatı 31,26 TL'dir"
                    var match = Regex.Match(fullText, @"hissesinin fiyatı\s+([\d,]+)\s+TL");

                    if (!match.Success)
                    {
                        // Pattern 2: "fiyatı 31,26 TL"
                        match = Regex.Match(fullText, @"fiyatı\s+([\d,]+)\s+TL");
                    }

                    if (!match.Success)
                    {
                        // Pattern 3: Herhangi bir "sayı TL" formatı
                        match = Regex.Match(fullText, @"([\d,]+)\s+TL'dir");
                    }

                    if (!match.Success)
                    {
                        // Pattern 4: En basit - sayı virgül sayı formatı
                        match = Regex.Match(fullText, @"(\d+,\d+)");
                    }

                    if (match.Success)
                    {
                        string priceText = match.Groups[1].Value.Trim();
                        System.Diagnostics.Debug.WriteLine($"Regex match: {priceText}");

                        // Virgülü noktaya çevir
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