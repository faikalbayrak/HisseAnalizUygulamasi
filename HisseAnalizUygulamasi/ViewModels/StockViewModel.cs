using HisseAnalizUygulamasi.Models;
using HisseAnalizUygulamasi.Services;
using System.ComponentModel;

namespace HisseAnalizUygulamasi.ViewModels
{
    /// <summary>
    /// DataGrid'de gösterilecek hisse view model'i
    /// INotifyPropertyChanged ile UI güncellemelerini sağlar
    /// </summary>
    public class StockViewModel : INotifyPropertyChanged
    {
        private StockData _stockData;

        public StockViewModel(StockData stockData)
        {
            _stockData = stockData;
        }

        // Temel Özellikler
        public string Symbol => _stockData.Symbol;
        public string CompanyName => _stockData.CompanyName;
        public decimal CurrentPrice => _stockData.CurrentPrice;
        public decimal FairValue => _stockData.FairValue;
        public decimal DiscountRate => _stockData.DiscountRate;
        public ColorCategory Category => _stockData.Category;

        // UI için formatlanmış değerler
        public string CurrentPriceFormatted => $"{_stockData.CurrentPrice:N2} ₺";
        public string FairValueFormatted => $"{_stockData.FairValue:N2} ₺";
        public string DiscountRateFormatted => $"%{_stockData.DiscountRate:N1}";

        // Emoji ve renkler
        public string CategoryEmoji => FairValueCalculator.GetCategoryPrefix(_stockData.Category);
        public string CategoryName => FairValueCalculator.GetCategoryName(_stockData.Category);
        public string FavoriteIcon => _stockData.IsFavorite ? "[FAV]" : "[   ]";

        // Favori toggle
        public bool IsFavorite
        {
            get => _stockData.IsFavorite;
            set
            {
                _stockData.IsFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
                OnPropertyChanged(nameof(FavoriteIcon));
            }
        }

        // Kullanıcı girdileri
        public decimal? TargetPriceOptimistic
        {
            get => _stockData.TargetPriceOptimistic;
            set
            {
                _stockData.TargetPriceOptimistic = value;
                OnPropertyChanged(nameof(TargetPriceOptimistic));
            }
        }

        public decimal? TargetPriceModerate
        {
            get => _stockData.TargetPriceModerate;
            set
            {
                _stockData.TargetPriceModerate = value;
                OnPropertyChanged(nameof(TargetPriceModerate));
            }
        }

        public decimal? TargetPricePessimistic
        {
            get => _stockData.TargetPricePessimistic;
            set
            {
                _stockData.TargetPricePessimistic = value;
                OnPropertyChanged(nameof(TargetPricePessimistic));
            }
        }

        public string Notes
        {
            get => _stockData.Notes;
            set
            {
                _stockData.Notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        // Geçmiş çeyrekler (karşılaştırma için)
        public string QuarterQ1 => GetQuarterDisplay(0);
        public string QuarterQ2 => GetQuarterDisplay(1);
        public string QuarterQ3 => GetQuarterDisplay(2);
        public string QuarterQ4 => GetQuarterDisplay(3);
        public string QuarterQ5 => GetQuarterDisplay(4);

        private string GetQuarterDisplay(int index)
        {
            if (_stockData.HistoricalQuarters == null || _stockData.HistoricalQuarters.Count <= index)
                return "-";

            var quarter = _stockData.HistoricalQuarters[index];
            string prefix = FairValueCalculator.GetCategoryPrefix(quarter.Category);
            return $"{prefix} {quarter.DiscountRate:N0}%";
        }

        /// <summary>
        /// Veriyi günceller ve UI'a bildirir
        /// </summary>
        public void Update(StockData newData)
        {
            _stockData = newData;
            OnPropertyChanged(string.Empty); // Tüm özellikleri güncelle
        }

        /// <summary>
        /// Ham veriyi döndürür (kaydetme için)
        /// </summary>
        public StockData GetStockData() => _stockData;

        // INotifyPropertyChanged implementasyonu
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}