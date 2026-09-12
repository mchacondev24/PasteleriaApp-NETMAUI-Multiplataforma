using System;
using System.Text;
using System.Threading.Tasks;

namespace PasteleriaApp.Services.Hardware
{
    public class BarcodeScannerService
    {
        private readonly StringBuilder _buffer = new();
        private DateTime _lastKeystrokeTime = DateTime.MinValue;
        private const int MaxKeystrokeDelayMs = 60; // < 60ms threshold for barcode burst detection
        private const int MinBarcodeLength = 3;

        public event Action<string>? OnBarcodeScanned;

        public void ProcessKeystroke(char keyChar)
        {
            var now = DateTime.Now;
            var timeDiff = (now - _lastKeystrokeTime).TotalMilliseconds;

            if (_buffer.Length > 0 && timeDiff > MaxKeystrokeDelayMs)
            {
                // Delay too long; discarded as manual human typing
                _buffer.Clear();
            }

            _lastKeystrokeTime = now;

            if (keyChar == '\r' || keyChar == '\n')
            {
                if (_buffer.Length >= MinBarcodeLength)
                {
                    var code = _buffer.ToString().Trim();
                    _buffer.Clear();
                    OnBarcodeScanned?.Invoke(code);
                }
                else
                {
                    _buffer.Clear();
                }
            }
            else
            {
                _buffer.Append(keyChar);
            }
        }

        public void TriggerManualScan(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                OnBarcodeScanned?.Invoke(code.Trim());
            }
        }
    }
}
