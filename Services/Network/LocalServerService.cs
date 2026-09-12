using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PasteleriaApp.Data;
using PasteleriaApp.Models;

namespace PasteleriaApp.Services.Network
{
    public class LocalServerService : IDisposable
    {
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private readonly DatabaseService _databaseService;
        private bool _isRunning = false;
        private int _port = 5055;
        private string _authToken = "PASTELERIA_TOKEN_2026";

        public event Action<string>? OnLogMessage;
        public bool IsRunning => _isRunning;
        public int Port => _port;
        public string ServerUrl => $"http://localhost:{_port}";

        public LocalServerService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task StartAsync(int port = 5055, string authToken = "PASTELERIA_TOKEN_2026")
        {
            if (_isRunning) return;

            _port = port;
            _authToken = authToken;
            _cts = new CancellationTokenSource();

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://*:{_port}/");
                _listener.Start();
                _isRunning = true;
                OnLogMessage?.Invoke($"[SERVIDOR] Iniciado en el puerto {_port}. Escuchando peticiones de terminales hijas.");

                _ = Task.Run(() => ListenLoopAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                try
                {
                    // Fallback to localhost prefix if wildcard requires elevated rights
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://localhost:{_port}/");
                    _listener.Start();
                    _isRunning = true;
                    OnLogMessage?.Invoke($"[SERVIDOR] Iniciado en http://localhost:{_port}/.");
                    _ = Task.Run(() => ListenLoopAsync(_cts.Token));
                }
                catch (Exception fallbackEx)
                {
                    _isRunning = false;
                    OnLogMessage?.Invoke($"[SERVIDOR ERROR] No se pudo iniciar el servidor: {fallbackEx.Message}");
                }
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            if (_listener != null && _listener.IsListening)
            {
                try { _listener.Stop(); _listener.Close(); } catch { }
            }
            _listener = null;
            _isRunning = false;
            OnLogMessage?.Invoke("[SERVIDOR] Servidor detenido.");
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (HttpListenerException) { break; }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"[ERROR RED] {ex.Message}");
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            // Enable CORS for web and cross-app calls
            res.AddHeader("Access-Control-Allow-Origin", "*");
            res.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            res.AddHeader("Access-Control-Allow-Headers", "Content-Type, X-Auth-Token, Authorization");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            var path = req.Url?.AbsolutePath.ToLowerInvariant() ?? "";
            OnLogMessage?.Invoke($"[PETICION] {req.HttpMethod} {path} desde {req.RemoteEndPoint}");

            try
            {
                // OpenAPI / Swagger definition
                if (path == "/swagger/v1/swagger.json" && req.HttpMethod == "GET")
                {
                    var swaggerJson = GetOpenApiSpecification();
                    await SendJsonResponseAsync(res, 200, swaggerJson, isRawString: true);
                    return;
                }

                // Public Health Check
                if (path == "/api/health" && req.HttpMethod == "GET")
                {
                    var health = new
                    {
                        status = "OK",
                        appName = "PasteleriaApp Server Central",
                        developer = "Maxwell Chacón",
                        city = "Granada - Managua, Nicaragua",
                        timestamp = DateTime.Now
                    };
                    await SendJsonResponseAsync(res, 200, health);
                    return;
                }

                // Validate Auth Token for protected endpoints
                var tokenHeader = req.Headers["X-Auth-Token"];
                if (tokenHeader != _authToken && req.QueryString["token"] != _authToken)
                {
                    await SendJsonResponseAsync(res, 401, new { error = "No autorizado. Token de seguridad inválido o ausente." });
                    return;
                }

                var db = await _databaseService.GetConnectionAsync();

                if (path == "/api/products" && req.HttpMethod == "GET")
                {
                    var products = await db.Table<Product>().Where(p => p.IsActive).ToListAsync();
                    await SendJsonResponseAsync(res, 200, products);
                    return;
                }

                if (path == "/api/categories" && req.HttpMethod == "GET")
                {
                    var categories = await db.Table<Category>().ToListAsync();
                    await SendJsonResponseAsync(res, 200, categories);
                    return;
                }

                if (path == "/api/tables" && req.HttpMethod == "GET")
                {
                    var tables = await db.Table<RestaurantTable>().ToListAsync();
                    await SendJsonResponseAsync(res, 200, tables);
                    return;
                }

                if (path == "/api/sales" && req.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
                    var body = await reader.ReadToEndAsync();
                    var sale = JsonSerializer.Deserialize<Sale>(body);
                    if (sale != null)
                    {
                        await db.InsertAsync(sale);
                        OnLogMessage?.Invoke($"[VENTA RECIBIDA] Venta #{sale.InvoiceNumber} por C$ {sale.TotalCordobas:N2} sincronizada.");
                        await SendJsonResponseAsync(res, 201, new { success = true, saleId = sale.Id, invoice = sale.InvoiceNumber });
                        return;
                    }
                    await SendJsonResponseAsync(res, 400, new { error = "Formato de venta inválido." });
                    return;
                }

                if (path == "/api/sync/pull" && req.HttpMethod == "GET")
                {
                    var backupJson = await _databaseService.ExportDatabaseJsonAsync();
                    await SendJsonResponseAsync(res, 200, backupJson, isRawString: true);
                    return;
                }

                await SendJsonResponseAsync(res, 404, new { error = $"Endpoint {path} no encontrado." });
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[ERROR ENDPOINT] {ex.Message}");
                await SendJsonResponseAsync(res, 500, new { error = ex.Message });
            }
        }

        private async Task SendJsonResponseAsync(HttpListenerResponse res, int statusCode, object data, bool isRawString = false)
        {
            res.StatusCode = statusCode;
            res.ContentType = "application/json; charset=utf-8";
            string responseString = isRawString ? (string)data : JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            res.ContentLength64 = buffer.Length;
            await res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }

        public string GetOpenApiSpecification()
        {
            return @"
{
  ""openapi"": ""3.0.1"",
  ""info"": {
    ""title"": ""PasteleriaApp API Central - Granada, Nicaragua"",
    ""description"": ""API REST para conexión de terminales hijas, POS remoto, comandas y sincronización. Desarrollado por Maxwell Chacón."",
    ""version"": ""1.0.0""
  },
  ""servers"": [
    {
      ""url"": ""http://localhost:" + _port + @"""
    }
  ],
  ""paths"": {
    ""/api/health"": {
      ""get"": {
        ""summary"": ""Verifica el estado del servidor central"",
        ""responses"": {
          ""200"": { ""description"": ""Servidor en línea y operativo."" }
        }
      }
    },
    ""/api/products"": {
      ""get"": {
        ""summary"": ""Obtiene el catálogo de productos disponibles"",
        ""parameters"": [
          { ""name"": ""X-Auth-Token"", ""in"": ""header"", ""required"": true, ""schema"": { ""type"": ""string"" } }
        ],
        ""responses"": {
          ""200"": { ""description"": ""Lista de productos de la pastelería."" }
        }
      }
    },
    ""/api/categories"": {
      ""get"": {
        ""summary"": ""Obtiene las categorías de productos"",
        ""parameters"": [
          { ""name"": ""X-Auth-Token"", ""in"": ""header"", ""required"": true, ""schema"": { ""type"": ""string"" } }
        ],
        ""responses"": {
          ""200"": { ""description"": ""Lista de categorías."" }
        }
      }
    },
    ""/api/tables"": {
      ""get"": {
        ""summary"": ""Obtiene el estado de las mesas y comandas"",
        ""parameters"": [
          { ""name"": ""X-Auth-Token"", ""in"": ""header"", ""required"": true, ""schema"": { ""type"": ""string"" } }
        ],
        ""responses"": {
          ""200"": { ""description"": ""Estado de las mesas."" }
        }
      }
    },
    ""/api/sales"": {
      ""post"": {
        ""summary"": ""Registra una venta desde una terminal hija"",
        ""parameters"": [
          { ""name"": ""X-Auth-Token"", ""in"": ""header"", ""required"": true, ""schema"": { ""type"": ""string"" } }
        ],
        ""requestBody"": {
          ""required"": true,
          ""content"": { ""application/json"": { ""schema"": { ""type"": ""object"" } } }
        },
        ""responses"": {
          ""201"": { ""description"": ""Venta registrada correctamente."" }
        }
      }
    },
    ""/api/sync/pull"": {
      ""get"": {
        ""summary"": ""Descarga base de datos completa para sincronización de hijas"",
        ""parameters"": [
          { ""name"": ""X-Auth-Token"", ""in"": ""header"", ""required"": true, ""schema"": { ""type"": ""string"" } }
        ],
        ""responses"": {
          ""200"": { ""description"": ""Datos JSON consolidados."" }
        }
      }
    }
  }
}";
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
