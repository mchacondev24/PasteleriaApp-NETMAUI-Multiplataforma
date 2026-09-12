using System;
using PasteleriaApp.Models;

namespace PasteleriaApp.Services.Fiscal
{
    public class DgiNicaraguaFiscalService
    {
        public const decimal StandardIvaRate = 0.15m; // 15% IVA Nicaragua
        public const decimal DefaultExchangeRateCordobasToUsd = 36.62m;

        public (decimal Subtotal, decimal IvaAmount, decimal RetentionAmount, decimal TotalCordobas, decimal TotalUSD) 
            CalculateFiscalTotals(decimal subtotalWithoutIva, decimal discount = 0, bool applyRetention = false, decimal retentionRate = 0.02m, decimal exchangeRate = DefaultExchangeRateCordobasToUsd)
        {
            decimal baseTaxable = Math.Max(0, subtotalWithoutIva - discount);
            decimal ivaAmount = Math.Round(baseTaxable * StandardIvaRate, 2);
            decimal retentionAmount = applyRetention ? Math.Round(baseTaxable * retentionRate, 2) : 0;
            decimal totalCordobas = Math.Round(baseTaxable + ivaAmount - retentionAmount, 2);
            decimal totalUSD = exchangeRate > 0 ? Math.Round(totalCordobas / exchangeRate, 2) : 0;

            return (baseTaxable, ivaAmount, retentionAmount, totalCordobas, totalUSD);
        }

        public string FormatFiscalInvoiceNumber(string prefix, int currentNumber)
        {
            return $"{prefix}{currentNumber:D8}";
        }

        public bool ValidateRucNicaragua(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc)) return false;
            ruc = ruc.Trim().ToUpper();
            // RUC format in Nicaragua is usually 14 alphanumeric characters (e.g., J0310000123456 or 0012904950006A)
            return ruc.Length >= 14;
        }

        public bool ValidateCedulaNicaragua(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return false;
            // Standard Cedula format: 000-000000-0000X
            var clean = cedula.Replace("-", "").Trim().ToUpper();
            return clean.Length == 14;
        }
    }
}
