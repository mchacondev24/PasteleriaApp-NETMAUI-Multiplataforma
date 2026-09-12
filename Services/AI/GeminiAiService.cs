using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PasteleriaApp.Data;
using PasteleriaApp.Models;

namespace PasteleriaApp.Services.AI
{
    public class DemandProjectionResult
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; } = 0;
        public decimal EstimatedWeeklyDemand { get; set; } = 0;
        public decimal SuggestedProductionBatch { get; set; } = 0;
        public string RecommendedReason { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = "Media"; // Alta, Media, Baja
    }

    public class GeminiAiService
    {
        private readonly DatabaseService _databaseService;
        private readonly HttpClient _httpClient;

        public GeminiAiService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
        }

        public async Task<string> AskGeminiAsync(string prompt, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return GetOfflineAiInsight(prompt);
            }

            try
            {
                // Call Gemini 2.5 Flash endpoint
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = "Eres el Asistente Experto en Gestión de Pastelerías, Panaderías y Cafeterías 'PasteleriaApp' (Granada, Nicaragua). " + prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return $"[IA Local Fallback] No se pudo conectar a Gemini API ({response.StatusCode}). Respuesta local:\n" + GetOfflineAiInsight(prompt);
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentElem) &&
                        contentElem.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();
                        return text ?? GetOfflineAiInsight(prompt);
                    }
                }

                return GetOfflineAiInsight(prompt);
            }
            catch (Exception ex)
            {
                return $"[IA Local - Error al contactar Gemini: {ex.Message}]\n\n" + GetOfflineAiInsight(prompt);
            }
        }

        public async Task<List<DemandProjectionResult>> CalculateProjectedDemandAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            var products = await db.Table<Product>().Where(p => p.IsActive).ToListAsync();
            var sales = await db.Table<SaleDetail>().ToListAsync();

            var list = new List<DemandProjectionResult>();
            var random = new Random();

            foreach (var p in products)
            {
                var totalSold = sales.Where(s => s.ProductId == p.Id).Sum(s => s.Quantity);
                
                // Base weekly demand calculation + seasonal weighting for cakes/pastries/bread
                decimal baseDemand = totalSold > 0 ? (totalSold * 1.35m) : (p.IsProduced ? 12 : 20);
                if (p.CategoryId == 1) baseDemand += 4; // Pasteles alta demanda de fin de semana
                if (p.CategoryId == 3) baseDemand += 15; // Panadería diaria
                if (p.CategoryId == 4) baseDemand += 10; // Cafés diarios

                decimal suggestedBatch = Math.Max(0, baseDemand - p.Stock);
                string urgency = p.Stock <= p.MinStock ? "Alta" : (p.Stock < baseDemand ? "Media" : "Baja");

                string reason = p.Stock <= p.MinStock
                    ? $"Stock crítico ({p.Stock} {p.Unit}). Alto riesgo de quiebre de inventario este fin de semana."
                    : $"Demanda proyectada de {baseDemand:N0} {p.Unit}s. Mantener producción continua.";

                list.Add(new DemandProjectionResult
                {
                    ProductName = p.Name,
                    CurrentStock = p.Stock,
                    EstimatedWeeklyDemand = Math.Round(baseDemand, 1),
                    SuggestedProductionBatch = Math.Round(suggestedBatch, 1),
                    RecommendedReason = reason,
                    UrgencyLevel = urgency
                });
            }

            return list.OrderByDescending(x => x.UrgencyLevel == "Alta").ThenByDescending(x => x.SuggestedProductionBatch).ToList();
        }

        private string GetOfflineAiInsight(string query)
        {
            var q = query.ToLowerInvariant();
            if (q.Contains("receta") || q.Contains("costo"))
            {
                return "💡 **Recomendación de Costeo y Receta de Pastelería:**\n" +
                       "- Para optimizar el margen del *Pastel Tres Leches*, compre la leche evaporada y condensada en presentación institucional por caja.\n" +
                       "- Mantenga la merma de bizcocho por debajo del 3% utilizando moldes calibrados.\n" +
                       "- El margen bruto sugerido para repostería fina en Granada es del 50% al 65%.";
            }
            else if (q.Contains("demanda") || q.Contains("proyeccion") || q.Contains("venta"))
            {
                return "📊 **Análisis Predictivo Local de Demanda:**\n" +
                       "- Los viernes y sábados la venta de Pasteles de 1 Lb y Porciones de Caramelo aumenta un 85%.\n" +
                       "- Se recomienda iniciar el horneado de Picos y Quesadillas a las 5:00 AM y 2:00 PM para captar el flujo de café matutino y vespertino.\n" +
                       "- Prepare lotes de masa madre y crema chantilly con 24h de anticipación.";
            }
            else if (q.Contains("cumpleaño") || q.Contains("promocion") || q.Contains("cliente"))
            {
                return "🎉 **Estrategia de Fidelización y Cumpleaños:**\n" +
                       "- Envíe recordatorio por WhatsApp a los clientes 3 días antes de su cumpleaños ofreciendo un 10% de descuento en pasteles personalizados.\n" +
                       "- Ofrezca un café gratis por la compra de una porción de pastel en el día exacto de su cumpleaños registrado.";
            }

            return "🍰 **Asistente de Pastelería & Gestión:**\n" +
                   "El sistema está optimizado para controlar la producción de reposterías, panadería, bebidas frías/calientes y platillos. " +
                   "Puede verificar el módulo de Producción para descontar insumos automáticamente y registrar lotes con trazabilidad DGI.";
        }
    }
}
