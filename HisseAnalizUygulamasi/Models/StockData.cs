namespace HisseAnalizUygulamasi.Models
{
    /// <summary>
    /// Hisse senedi verilerini tutan ana model sýnýfý
    /// </summary>
    public class StockData
    {
        // Temel Bilgiler
        public string Symbol { get; set; } = string.Empty;        // Hisse sembolü (ARDYZ, THYAO vb.)
        public string CompanyName { get; set; } = string.Empty;   // Þirket adý
        public DateTime Quarter { get; set; }                     // Çeyrek dönemi

        // Bilanço Verileri
        public decimal TotalResources { get; set; }               // Toplam Kaynaklar
        public decimal LongTermLiabilities { get; set; }          // Toplam Uzun Vadeli Yükümlülükler
        public decimal ShortTermLiabilities { get; set; }         // Toplam Kýsa Vadeli Yükümlülükler
        public decimal EmployeeBenefitLiabilities { get; set; }   // Çalýþanlara Saðlanan Faydalar Kapsamýnda Borçlar
        public decimal PaidInCapital { get; set; }                // Ödenmiþ Sermaye

        // Hesaplanan Deðerler
        public decimal FairValue { get; set; }                    // Adil Deðer
        public decimal CurrentPrice { get; set; }                 // Güncel Fiyat
        public decimal DiscountRate { get; set; }                 // Ýskonto Oraný (%)
        public ColorCategory Category { get; set; }               // Renk kategorisi

        // Kullanýcý Girdileri
        public decimal? TargetPriceOptimistic { get; set; }       // Hedef Fiyat - Ýyimser
        public decimal? TargetPriceModerate { get; set; }         // Hedef Fiyat - Orta
        public decimal? TargetPricePessimistic { get; set; }      // Hedef Fiyat - Kötümser
        public string Notes { get; set; } = string.Empty;         // Kullanýcý yorumu
        public bool IsFavorite { get; set; }                      // Favori mi?

        // Geçmiþ Çeyrek Verileri (karþýlaþtýrma için)
        public List<QuarterData> HistoricalQuarters { get; set; } = new();
    }

    /// <summary>
    /// Renk kategorileri (iskonto durumuna göre)
    /// </summary>
    public enum ColorCategory
    {
        Neutral,    // Nötr (< %50)
        Yellow,     // Sarý (%50-100)
        Green,      // Yeþil (%100-200)
        Blue        // Mavi (> %200)
    }

    /// <summary>
    /// Geçmiþ çeyrek verileri için
    /// </summary>
    public class QuarterData
    {
        public DateTime Quarter { get; set; }           // Q1-2024, Q2-2024 vb.
        public decimal FairValue { get; set; }          // O çeyrekteki adil deðer
        public decimal Price { get; set; }              // O çeyrekteki fiyat
        public decimal DiscountRate { get; set; }       // Ýskonto oraný
        public ColorCategory Category { get; set; }     // Renk kategorisi
    }
}