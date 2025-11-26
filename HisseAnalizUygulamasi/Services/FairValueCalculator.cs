using HisseAnalizUygulamasi.Models;

namespace HisseAnalizUygulamasi.Services
{
    /// <summary>
    /// Adil değer hesaplama servisi
    /// </summary>
    public static class FairValueCalculator
    {
        /// <summary>
        /// Ana hesaplama fonksiyonu
        /// Formül: (Toplam Kaynaklar + Uzun Vadeli Yük. + Kısa Vadeli Yük. - Çalışan Borçları) / Ödenmiş Sermaye
        /// </summary>
        public static void Calculate(StockData stock)
        {
            // Formül hesaplaması
            decimal numerator = stock.TotalResources
                              + stock.LongTermLiabilities
                              + stock.ShortTermLiabilities
                              - stock.EmployeeBenefitLiabilities;

            // Sıfıra bölme kontrolü
            stock.FairValue = stock.PaidInCapital != 0
                ? numerator / stock.PaidInCapital
                : 0;

            // İskonto oranı hesabı (ters orantı)
            if (stock.CurrentPrice > 0 && stock.FairValue > 0)
            {
                stock.DiscountRate = ((stock.FairValue - stock.CurrentPrice) / stock.FairValue) * 100;
            }
            else
            {
                stock.DiscountRate = 0;
            }

            // Renk kategorisi belirleme
            DetermineCategory(stock);
        }

        /// <summary>
        /// Renk kategorisini belirler
        /// %50'nin alti: Notr, %50-100: Sari, %100-200: Yesil, %200+: Mavi
        /// </summary>
        private static void DetermineCategory(StockData stock)
        {
            if (stock.FairValue == 0)
            {
                stock.Category = ColorCategory.Neutral;
                return;
            }

            decimal ratio = (stock.CurrentPrice / stock.FairValue) * 100;

            if (ratio < 50)
            {
                stock.Category = ColorCategory.Neutral;
            }
            else if (ratio >= 50 && ratio < 100)
            {
                stock.Category = ColorCategory.Yellow;
            }
            else if (ratio >= 100 && ratio < 200)
            {
                stock.Category = ColorCategory.Green;
            }
            else // ratio >= 200
            {
                stock.Category = ColorCategory.Blue;
            }
        }

        /// <summary>
        /// Karşılaştırma hesaplamaları
        /// 1. Adil Değer / Güncel Fiyat
        /// 2. Hedef Fiyat / Güncel Fiyat
        /// 3. Adil Değer / Hedef Fiyat
        /// </summary>
        public static (decimal fairToCurrentRatio, decimal targetToCurrentRatio, decimal fairToTargetRatio)
            CalculateComparison(StockData stock, decimal targetPrice)
        {
            if (stock.CurrentPrice == 0 || targetPrice == 0 || stock.FairValue == 0)
                return (0, 0, 0);

            decimal fairToCurrentRatio = stock.FairValue / stock.CurrentPrice;
            decimal targetToCurrentRatio = targetPrice / stock.CurrentPrice;
            decimal fairToTargetRatio = stock.FairValue / targetPrice;

            return (fairToCurrentRatio, targetToCurrentRatio, fairToTargetRatio);
        }

        /// <summary>
        /// Kategori prefix'ini dondurur (emoji yerine)
        /// </summary>
        public static string GetCategoryPrefix(ColorCategory category)
        {
            return category switch
            {
                ColorCategory.Blue => "[MAVI]",
                ColorCategory.Green => "[YESIL]",
                ColorCategory.Yellow => "[SARI]",
                ColorCategory.Neutral => "[NOTR]",
                _ => "[NOTR]"
            };
        }

        /// <summary>
        /// Kategori emoji'sini dondurur - Emoji yerine text prefix
        /// </summary>
        public static string GetCategoryEmoji(ColorCategory category)
        {
            return GetCategoryPrefix(category);
        }

        /// <summary>
        /// Kategori ismini döndürür
        /// </summary>
        public static string GetCategoryName(ColorCategory category)
        {
            return category switch
            {
                ColorCategory.Blue => "Mavi",
                ColorCategory.Green => "Yeşil",
                ColorCategory.Yellow => "Sarı",
                ColorCategory.Neutral => "Nötr",
                _ => "Bilinmiyor"
            };
        }
    }
}