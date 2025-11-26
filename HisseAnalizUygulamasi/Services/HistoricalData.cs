using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HisseAnalizUygulamasi.Services
{
    public class HistoricalRecord
    {
        public DateTime Tarih { get; set; }
        public string Sembol { get; set; }
        public string SirketAdi { get; set; }
        public decimal ToplamKaynaklar { get; set; }
        public decimal UzunVadeliYuk { get; set; }
        public decimal KisaVadeliYuk { get; set; }
        public decimal CalisanBorclari { get; set; }
        public decimal OdenmisSermaaye { get; set; }
        public decimal AdilDeger { get; set; }
        public decimal? GuncelFiyat { get; set; }
        public decimal? IskontoOrani { get; set; }
    }

    public class HistoricalDataManager
    {
        private const string HistoryFileName = "history.json";
        private Dictionary<string, List<HistoricalRecord>> _history;

        public HistoricalDataManager()
        {
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryFileName))
                {
                    string json = File.ReadAllText(HistoryFileName);
                    _history = JsonSerializer.Deserialize<Dictionary<string, List<HistoricalRecord>>>(json)
                        ?? new Dictionary<string, List<HistoricalRecord>>();

                    System.Diagnostics.Debug.WriteLine($"Gecmis yuklenidi: {_history.Count} sembol");
                }
                else
                {
                    _history = new Dictionary<string, List<HistoricalRecord>>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gecmis yukleme hatasi: {ex.Message}");
                _history = new Dictionary<string, List<HistoricalRecord>>();
            }
        }

        private void SaveHistory()
        {
            try
            {
                string json = JsonSerializer.Serialize(_history, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(HistoryFileName, json);

                System.Diagnostics.Debug.WriteLine($"Gecmis kaydedildi: {_history.Count} sembol");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gecmis kaydetme hatasi: {ex.Message}");
            }
        }

        public void AddRecord(string symbol, string companyName, BilancoData bilancoData,
            decimal adilDeger, decimal? guncelFiyat, decimal? iskontoOrani)
        {
            if (!_history.ContainsKey(symbol))
            {
                _history[symbol] = new List<HistoricalRecord>();
            }

            var record = new HistoricalRecord
            {
                Tarih = DateTime.Now,
                Sembol = symbol,
                SirketAdi = companyName,
                ToplamKaynaklar = bilancoData.ToplamKaynaklar,
                UzunVadeliYuk = bilancoData.UzunVadeliYuk,
                KisaVadeliYuk = bilancoData.KisaVadeliYuk,
                CalisanBorclari = bilancoData.CalisanBorclari,
                OdenmisSermaaye = bilancoData.OdenmisSermaaye,
                AdilDeger = adilDeger,
                GuncelFiyat = guncelFiyat,
                IskontoOrani = iskontoOrani
            };

            _history[symbol].Add(record);
            SaveHistory();

            System.Diagnostics.Debug.WriteLine($"Kayit eklendi: {symbol} - {record.Tarih}");
        }

        public List<HistoricalRecord> GetRecords(string symbol)
        {
            if (_history.ContainsKey(symbol))
            {
                return _history[symbol].OrderByDescending(r => r.Tarih).ToList();
            }
            return new List<HistoricalRecord>();
        }

        public List<string> GetAllSymbols()
        {
            return _history.Keys.OrderBy(k => k).ToList();
        }

        public int GetRecordCount(string symbol)
        {
            if (_history.ContainsKey(symbol))
            {
                return _history[symbol].Count;
            }
            return 0;
        }

        public void DeleteRecords(string symbol)
        {
            if (_history.ContainsKey(symbol))
            {
                _history.Remove(symbol);
                SaveHistory();
                System.Diagnostics.Debug.WriteLine($"Kayitlar silindi: {symbol}");
            }
        }

        public void ClearAllRecords()
        {
            _history.Clear();
            SaveHistory();
            System.Diagnostics.Debug.WriteLine("Tum kayitlar silindi");
        }

        public HistoricalRecord GetLatestRecord(string symbol)
        {
            if (_history.ContainsKey(symbol) && _history[symbol].Count > 0)
            {
                return _history[symbol].OrderByDescending(r => r.Tarih).FirstOrDefault();
            }
            return null;
        }

        public string GetTrendAnalysis(string symbol)
        {
            if (!_history.ContainsKey(symbol) || _history[symbol].Count < 2)
            {
                return "Trend analizi için en az 2 kayıt gerekli";
            }

            var records = _history[symbol].OrderByDescending(r => r.Tarih).Take(10).ToList();
            var latest = records.First();
            var oldest = records.Last();

            decimal adilDegerDegisim = ((latest.AdilDeger - oldest.AdilDeger) / oldest.AdilDeger) * 100;

            string trend = adilDegerDegisim > 5 ? "📈 YUKSELIŞ" :
                           adilDegerDegisim < -5 ? "📉 DÜŞÜŞ" :
                           "➡️ YATAY";

            return $"{trend} - Adil değer değişimi: %{adilDegerDegisim:F2}";
        }
    }
}