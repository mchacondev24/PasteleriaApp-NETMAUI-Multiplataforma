using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PasteleriaApp.Services.Hardware
{
    public class ScaleWeighingService : IDisposable
    {
        private SerialPort? _serialPort;
        private CancellationTokenSource? _cts;
        private decimal _currentWeight = 0;
        private decimal _tareWeight = 0;
        private string _unit = "Lb"; // Lb, Kg
        private bool _isConnected = false;
        private bool _isSimulated = false;

        public event Action<decimal, string>? OnWeightChanged;
        public event Action<string>? OnStatusMessage;

        public decimal CurrentWeight => Math.Max(0, _currentWeight - _tareWeight);
        public string Unit => _unit;
        public bool IsConnected => _isConnected;
        public bool IsSimulated => _isSimulated;

        public string[] GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch
            {
                return new[] { "COM1", "COM2", "COM3", "COM4", "VIRTUAL_SCALE" };
            }
        }

        public bool Connect(string portName = "COM1", int baudRate = 9600, string unit = "Lb")
        {
            _unit = unit;
            if (portName == "VIRTUAL_SCALE" || string.IsNullOrWhiteSpace(portName))
            {
                _isSimulated = true;
                _isConnected = true;
                OnStatusMessage?.Invoke("Báscula en modo simulación/virtual activa.");
                return true;
            }

            try
            {
                Disconnect();
                _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                _serialPort.Open();
                _isConnected = true;
                _isSimulated = false;
                _cts = new CancellationTokenSource();

                Task.Run(() => ReadLoopAsync(_cts.Token));
                OnStatusMessage?.Invoke($"Báscula conectada en {portName} a {baudRate} baudios.");
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                _isSimulated = true;
                OnStatusMessage?.Invoke($"No se pudo abrir {portName}. Activando modo virtual: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try { _serialPort.Close(); } catch { }
            }
            _serialPort?.Dispose();
            _serialPort = null;
            _isConnected = false;
        }

        public void SetTare()
        {
            _tareWeight = _currentWeight;
            OnWeightChanged?.Invoke(CurrentWeight, _unit);
            OnStatusMessage?.Invoke($"Tara establecida en {_tareWeight:F3} {_unit}");
        }

        public void ResetZero()
        {
            _tareWeight = 0;
            _currentWeight = 0;
            OnWeightChanged?.Invoke(0, _unit);
            OnStatusMessage?.Invoke("Báscula reseteada a Cero (0.000).");
        }

        public void SetSimulatedWeight(decimal weight)
        {
            _currentWeight = weight;
            OnWeightChanged?.Invoke(CurrentWeight, _unit);
        }

        public decimal CalculatePriceByWeight(decimal weight, decimal pricePerUnit)
        {
            return Math.Round(weight * pricePerUnit, 2);
        }

        private async Task ReadLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    string line = _serialPort.ReadLine();
                    ParseScaleOutput(line);
                }
                catch (TimeoutException) { }
                catch (Exception)
                {
                    await Task.Delay(200, token);
                }
                await Task.Delay(50, token);
            }
        }

        private void ParseScaleOutput(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            // Extract numeric weight using regex (Torrey / CAS format: "ST,GS,+  1.250kg" or "WN 0.500 LB")
            var match = Regex.Match(raw, @"[-+]?[0-9]*\.?[0-9]+");
            if (match.Success && decimal.TryParse(match.Value, out var weight))
            {
                _currentWeight = weight;
                if (raw.ToUpper().Contains("KG"))
                    _unit = "Kg";
                else if (raw.ToUpper().Contains("LB"))
                    _unit = "Lb";

                OnWeightChanged?.Invoke(CurrentWeight, _unit);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
