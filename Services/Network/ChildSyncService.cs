using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PasteleriaApp.Data;
using PasteleriaApp.Models;

namespace PasteleriaApp.Services.Network
{
    public class ChildSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseService _databaseService;

        public ChildSyncService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync(string serverUrl, string token)
        {
            try
            {
                var cleanUrl = serverUrl.TrimEnd('/');
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{cleanUrl}/api/health");
                request.Headers.Add("X-Auth-Token", token);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (true, $"Conexión exitosa con el servidor central: {content}");
                }
                return (false, $"Error al conectar. Código: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, $"Fallo de red: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> PullDataFromServerAsync(string serverUrl, string token)
        {
            try
            {
                var cleanUrl = serverUrl.TrimEnd('/');
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{cleanUrl}/api/sync/pull");
                request.Headers.Add("X-Auth-Token", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"Error al descargar datos. Código: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var imported = await _databaseService.ImportDatabaseJsonAsync(json);
                if (imported)
                {
                    return (true, "Base de datos sincronizada exitosamente con el servidor central.");
                }
                return (false, "Error al procesar el archivo de sincronización.");
            }
            catch (Exception ex)
            {
                return (false, $"Excepción en sincronización: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> PushSaleToServerAsync(string serverUrl, string token, Sale sale)
        {
            try
            {
                var cleanUrl = serverUrl.TrimEnd('/');
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{cleanUrl}/api/sales");
                request.Headers.Add("X-Auth-Token", token);
                var json = JsonSerializer.Serialize(sale);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Venta transmitida al servidor central.");
                }
                return (false, $"Error al enviar venta: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, $"Error de red al enviar venta: {ex.Message}");
            }
        }
    }
}
