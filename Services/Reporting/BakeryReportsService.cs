using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PasteleriaApp.Data;
using PasteleriaApp.Models;

namespace PasteleriaApp.Services.Reporting
{
    public class ReportRow
    {
        public string Col1 { get; set; } = string.Empty;
        public string Col2 { get; set; } = string.Empty;
        public string Col3 { get; set; } = string.Empty;
        public string Col4 { get; set; } = string.Empty;
        public string Col5 { get; set; } = string.Empty;
        public string Col6 { get; set; } = string.Empty;
        public decimal Numeric1 { get; set; } = 0;
        public decimal Numeric2 { get; set; } = 0;
        public decimal Numeric3 { get; set; } = 0;
    }

    public class ReportResult
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<ReportRow> Rows { get; set; } = new();
        public decimal TotalCordobas { get; set; } = 0;
        public decimal TotalUSD { get; set; } = 0;
        public string SummaryText { get; set; } = string.Empty;
    }

    public class BakeryReportsService
    {
        private readonly DatabaseService _databaseService;

        public BakeryReportsService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Report 1: Ventas y Facturación
        public async Task<ReportResult> GetSalesReportAsync(DateTime fromDate, DateTime toDate, string? paymentMethod = null)
        {
            var db = await _databaseService.GetConnectionAsync();
            var sales = await db.Table<Sale>().ToListAsync();
            var filtered = sales.Where(s => s.SaleDate.Date >= fromDate.Date && s.SaleDate.Date <= toDate.Date && s.Status != "Anulada");

            if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod != "Todos")
            {
                filtered = filtered.Where(s => s.PaymentMethod == paymentMethod || s.PaymentType == paymentMethod);
            }

            var list = filtered.OrderByDescending(s => s.SaleDate).ToList();

            var result = new ReportResult
            {
                Title = "Reporte 1: Ventas y Facturación Diaria / Periódica",
                Subtitle = $"Período: {fromDate:dd/MM/yyyy} al {toDate:dd/MM/yyyy}",
                Headers = new List<string> { "No. Factura", "Fecha", "Cliente", "Método Pago", "IVA DGI (15%)", "Total C$", "Total USD ($)" }
            };

            foreach (var s in list)
            {
                result.Rows.Add(new ReportRow
                {
                    Col1 = !string.IsNullOrEmpty(s.FiscalInvoiceNumber) ? s.FiscalInvoiceNumber : s.InvoiceNumber,
                    Col2 = s.SaleDate.ToString("dd/MM/yyyy HH:mm"),
                    Col3 = s.CustomerName,
                    Col4 = $"{s.PaymentType} ({s.PaymentMethod})",
                    Col5 = $"C$ {s.IvaAmount:N2}",
                    Col6 = $"C$ {s.TotalCordobas:N2}",
                    Numeric1 = s.TotalCordobas,
                    Numeric2 = s.TotalUSD,
                    Numeric3 = s.IvaAmount
                });
            }

            result.TotalCordobas = list.Sum(x => x.TotalCordobas);
            result.TotalUSD = list.Sum(x => x.TotalUSD);
            result.SummaryText = $"Total Transacciones: {list.Count} | Total IVA DGI: C$ {list.Sum(x => x.IvaAmount):N2} | Total Ventas: C$ {result.TotalCordobas:N2} (${result.TotalUSD:N2})";

            return result;
        }

        // Report 2: Costos de Recetas y Margen de Utilidad
        public async Task<ReportResult> GetRecipeCostsReportAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            var recipes = await db.Table<Recipe>().ToListAsync();
            var products = await db.Table<Product>().ToListAsync();

            var result = new ReportResult
            {
                Title = "Reporte 2: Costos de Recetas y Margen de Utilidad por Producto",
                Subtitle = "Análisis de costo unitario de elaboración vs. precio de venta",
                Headers = new List<string> { "Producto Elaborado", "Rendimiento", "Costo Insumos", "Costo Unitario Total", "Precio Venta C$", "Margen Bruto (%)" }
            };

            foreach (var r in recipes)
            {
                var prod = products.FirstOrDefault(p => p.Id == r.ProductId);
                decimal salePrice = prod?.SalePrice ?? 0;
                decimal cost = r.FinalUnitCost > 0 ? r.FinalUnitCost : r.EstimatedCost;
                decimal margin = salePrice > 0 ? Math.Round(((salePrice - cost) / salePrice) * 100, 1) : 0;

                result.Rows.Add(new ReportRow
                {
                    Col1 = r.ProductName,
                    Col2 = $"{r.YieldQuantity} {r.YieldUnit}",
                    Col3 = $"C$ {r.EstimatedCost:N2}",
                    Col4 = $"C$ {cost:N2}",
                    Col5 = $"C$ {salePrice:N2}",
                    Col6 = $"{margin:N1}%",
                    Numeric1 = cost,
                    Numeric2 = salePrice,
                    Numeric3 = margin
                });
            }

            result.SummaryText = $"Total Recetas Registradas: {recipes.Count} | Margen promedio de repostería: {(result.Rows.Any() ? result.Rows.Average(x => x.Numeric3) : 0):N1}%";
            return result;
        }

        // Report 3: Control de Producción y Rendimiento de Lotes
        public async Task<ReportResult> GetProductionOrdersReportAsync(DateTime fromDate, DateTime toDate)
        {
            var db = await _databaseService.GetConnectionAsync();
            var orders = await db.Table<ProductionOrder>().ToListAsync();
            var filtered = orders.Where(o => o.CreatedAt.Date >= fromDate.Date && o.CreatedAt.Date <= toDate.Date).ToList();

            var result = new ReportResult
            {
                Title = "Reporte 3: Control de Producción y Rendimiento de Lotes",
                Subtitle = $"Período: {fromDate:dd/MM/yyyy} al {toDate:dd/MM/yyyy}",
                Headers = new List<string> { "No. Orden", "Lote", "Producto", "Cant. Planificada", "Cant. Obtenida", "Estado", "Costo Lote C$" }
            };

            foreach (var o in filtered)
            {
                result.Rows.Add(new ReportRow
                {
                    Col1 = o.OrderNumber,
                    Col2 = o.BatchCode,
                    Col3 = o.ProductName,
                    Col4 = o.QuantityToProduce.ToString("N1"),
                    Col5 = o.ActualQuantityProduced.ToString("N1"),
                    Col6 = o.Status,
                    Numeric1 = o.TotalCost
                });
            }

            result.TotalCordobas = filtered.Sum(x => x.TotalCost);
            result.SummaryText = $"Total Órdenes: {filtered.Count} | Costo Total de Producción: C$ {result.TotalCordobas:N2}";
            return result;
        }

        // Report 4: Kardex y Movimientos de Insumos
        public async Task<ReportResult> GetIngredientsKardexReportAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            var ingredients = await db.Table<Ingredient>().ToListAsync();

            var result = new ReportResult
            {
                Title = "Reporte 4: Kardex e Inventario de Materia Prima / Insumos",
                Subtitle = "Trazabilidad de harina, azúcar, lácteos, huevos y materias primas",
                Headers = new List<string> { "Código", "Insumo", "Unidad", "Costo Unit.", "Stock Actual", "Stock Mínimo", "Valor Inventario C$" }
            };

            foreach (var ing in ingredients)
            {
                decimal totalVal = ing.CurrentStock * ing.CostPerUnit;
                result.Rows.Add(new ReportRow
                {
                    Col1 = ing.Code,
                    Col2 = ing.Name,
                    Col3 = ing.Unit,
                    Col4 = $"C$ {ing.CostPerUnit:N2}",
                    Col5 = ing.CurrentStock.ToString("N1"),
                    Col6 = ing.MinStock.ToString("N1"),
                    Numeric1 = totalVal
                });
            }

            result.TotalCordobas = result.Rows.Sum(x => x.Numeric1);
            result.SummaryText = $"Total Insumos: {ingredients.Count} | Valor Total de Materia Prima en Bodega: C$ {result.TotalCordobas:N2}";
            return result;
        }

        // Report 5: Proyección de Demanda Estimada con IA
        public async Task<ReportResult> GetProjectedDemandReportAsync(AI.GeminiAiService aiService)
        {
            var projections = await aiService.CalculateProjectedDemandAsync();

            var result = new ReportResult
            {
                Title = "Reporte 5: Proyección de Demanda Estimada con Inteligencia Artificial",
                Subtitle = "Modelado predictivo de demanda semanal para optimizar horneado y compras",
                Headers = new List<string> { "Producto", "Stock Actual", "Demanda Proyectada", "Lote Sugerido a Producir", "Nivel de Urgencia", "Justificación IA" }
            };

            foreach (var p in projections)
            {
                result.Rows.Add(new ReportRow
                {
                    Col1 = p.ProductName,
                    Col2 = p.CurrentStock.ToString("N1"),
                    Col3 = p.EstimatedWeeklyDemand.ToString("N1"),
                    Col4 = p.SuggestedProductionBatch.ToString("N1"),
                    Col5 = p.UrgencyLevel,
                    Col6 = p.RecommendedReason
                });
            }

            result.SummaryText = $"Total Productos Analizados: {projections.Count} | Urgencias Altas: {projections.Count(x => x.UrgencyLevel == "Alta")}";
            return result;
        }

        // Report 6: Ventas al Crédito y Cuentas por Cobrar (CxC)
        public async Task<ReportResult> GetAccountsReceivableReportAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            var customers = await db.Table<Customer>().Where(c => c.CurrentCreditBalance > 0).ToListAsync();

            var result = new ReportResult
            {
                Title = "Reporte 6: Ventas al Crédito y Cuentas por Cobrar (CxC)",
                Subtitle = "Control de clientes con saldo pendiente de pago",
                Headers = new List<string> { "Cliente", "Cédula / RUC", "Teléfono", "Límite Crédito C$", "Saldo Adeudado C$", "Estado" }
            };

            foreach (var c in customers)
            {
                result.Rows.Add(new ReportRow
                {
                    Col1 = c.FullName,
                    Col2 = c.IdentificationNumber,
                    Col3 = c.Phone,
                    Col4 = $"C$ {c.CreditLimit:N2}",
                    Col5 = $"C$ {c.CurrentCreditBalance:N2}",
                    Col6 = c.CurrentCreditBalance >= c.CreditLimit ? "Límite Alcanzado" : "Al Corriente",
                    Numeric1 = c.CurrentCreditBalance
                });
            }

            result.TotalCordobas = customers.Sum(x => x.CurrentCreditBalance);
            result.SummaryText = $"Clientes con Deuda: {customers.Count} | Total Cartera por Cobrar: C$ {result.TotalCordobas:N2}";
            return result;
        }

        // Report 7: Compras e Insumos al Crédito (CxP)
        public async Task<ReportResult> GetAccountsPayableReportAsync()
        {
            var db = await _databaseService.GetConnectionAsync();
            var suppliers = await db.Table<Supplier>().Where(s => s.CurrentDebt > 0).ToListAsync();

            var result = new ReportResult
            {
                Title = "Reporte 7: Compras e Insumos al Crédito (Cuentas por Pagar - CxP)",
                Subtitle = "Deudas pendientes con proveedores de materias primas y empaques",
                Headers = new List<string> { "Proveedor", "Contacto", "RUC", "Teléfono", "Plazo Días", "Saldo Pendiente C$" }
            };

            foreach (var s in suppliers)
            {
                result.Rows.Add(new ReportRow
                {
                    Col1 = s.CompanyName,
                    Col2 = s.ContactName,
                    Col3 = s.Ruc,
                    Col4 = s.Phone,
                    Col5 = $"{s.PaymentTermsDays} días",
                    Col6 = $"C$ {s.CurrentDebt:N2}",
                    Numeric1 = s.CurrentDebt
                });
            }

            result.TotalCordobas = suppliers.Sum(x => x.CurrentDebt);
            result.SummaryText = $"Proveedores por Pagar: {suppliers.Count} | Total Deuda a Proveedores: C$ {result.TotalCordobas:N2}";
            return result;
        }

        // Report 8: Top Productos Más Vendidos (Ranking ABC)
        public async Task<ReportResult> GetTopSellingProductsReportAsync(DateTime fromDate, DateTime toDate)
        {
            var db = await _databaseService.GetConnectionAsync();
            var sales = await db.Table<Sale>().Where(s => s.SaleDate.Date >= fromDate.Date && s.SaleDate.Date <= toDate.Date && s.Status != "Anulada").ToListAsync();
            var saleIds = sales.Select(s => s.Id).ToList();

            var allDetails = await db.Table<SaleDetail>().ToListAsync();
            var filteredDetails = allDetails.Where(d => saleIds.Contains(d.SaleId)).ToList();

            var grouped = filteredDetails
                .GroupBy(d => new { d.ProductId, d.ProductName })
                .Select(g => new
                {
                    g.Key.ProductName,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            var result = new ReportResult
            {
                Title = "Reporte 8: Ranking de Productos Más Vendidos (Top Pasteles, Panes, Cafés y Batidos)",
                Subtitle = $"Período: {fromDate:dd/MM/yyyy} al {toDate:dd/MM/yyyy}",
                Headers = new List<string> { "Posición", "Producto", "Cantidad Vendida", "Total Ingresos C$", "% del Total" }
            };

            decimal grandTotal = grouped.Sum(x => x.TotalRevenue);
            int rank = 1;

            foreach (var g in grouped)
            {
                decimal pct = grandTotal > 0 ? (g.TotalRevenue / grandTotal) * 100 : 0;
                result.Rows.Add(new ReportRow
                {
                    Col1 = $"#{rank++}",
                    Col2 = g.ProductName,
                    Col3 = g.TotalQuantity.ToString("N1"),
                    Col4 = $"C$ {g.TotalRevenue:N2}",
                    Col5 = $"{pct:N1}%",
                    Numeric1 = g.TotalRevenue
                });
            }

            result.TotalCordobas = grandTotal;
            result.SummaryText = $"Productos Vendidos en el período: {grouped.Count} | Ingresos Consolidados: C$ {grandTotal:N2}";
            return result;
        }

        // Report 9: Cumpleaños de Clientes del Mes y Fidelización
        public async Task<ReportResult> GetCustomerBirthdaysReportAsync(int month)
        {
            var db = await _databaseService.GetConnectionAsync();
            var customers = await db.Table<Customer>().Where(c => c.Birthday != null).ToListAsync();
            var monthCustomers = customers.Where(c => c.Birthday!.Value.Month == month).OrderBy(c => c.Birthday!.Value.Day).ToList();

            var monthNames = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

            var result = new ReportResult
            {
                Title = "Reporte 9: Agenda de Cumpleaños de Clientes y Fidelización",
                Subtitle = $"Clientes con cumpleaños en el mes de {monthNames[month]}",
                Headers = new List<string> { "Día", "Cliente", "Teléfono", "Email", "Dirección", "Promoción Sugerida" }
            };

            foreach (var c in monthCustomers)
            {
                result.Rows.Add(new ReportRow
                {
                    Col1 = $"{c.Birthday!.Value.Day:D2}/{month:D2}",
                    Col2 = c.FullName,
                    Col3 = c.Phone,
                    Col4 = c.Email,
                    Col5 = c.Address,
                    Col6 = "🎂 10% Descuento en Pastel de Celebración + Café Gratis"
                });
            }

            result.SummaryText = $"Total Cumpleañeros en {monthNames[month]}: {monthCustomers.Count} clientes.";
            return result;
        }

        // Report 10: Auditoría de Mesas, Meseros y Comandas de Salón
        public async Task<ReportResult> GetTablesAndWaitersReportAsync(DateTime fromDate, DateTime toDate)
        {
            var db = await _databaseService.GetConnectionAsync();
            var sales = await db.Table<Sale>().Where(s => s.SaleDate.Date >= fromDate.Date && s.SaleDate.Date <= toDate.Date && s.TableId != null && s.Status != "Anulada").ToListAsync();

            var groupedWaiters = sales
                .GroupBy(s => string.IsNullOrEmpty(s.WaiterName) ? "Mostrador / Sin Asignar" : s.WaiterName)
                .Select(g => new
                {
                    Waiter = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.TotalCordobas)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var result = new ReportResult
            {
                Title = "Reporte 10: Auditoría de Comandas, Mesas y Rendimiento de Meseros",
                Subtitle = $"Período: {fromDate:dd/MM/yyyy} al {toDate:dd/MM/yyyy}",
                Headers = new List<string> { "Mesero / Atendido Por", "Comandas / Mesas Atendidas", "Total Facturado C$", "Ticket Promedio C$" }
            };

            foreach (var w in groupedWaiters)
            {
                decimal avg = w.Count > 0 ? w.Total / w.Count : 0;
                result.Rows.Add(new ReportRow
                {
                    Col1 = w.Waiter,
                    Col2 = w.Count.ToString(),
                    Col3 = $"C$ {w.Total:N2}",
                    Col4 = $"C$ {avg:N2}",
                    Numeric1 = w.Total
                });
            }

            result.TotalCordobas = groupedWaiters.Sum(x => x.Total);
            result.SummaryText = $"Total Comandas de Salón: {sales.Count} | Total Venta en Salón: C$ {result.TotalCordobas:N2}";
            return result;
        }
    }
}
