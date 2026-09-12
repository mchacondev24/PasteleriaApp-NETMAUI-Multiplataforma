using System;
using System.Collections.Generic;
using System.Text;
using PasteleriaApp.Models;

namespace PasteleriaApp.Services.Hardware
{
    public class ThermalPrinterService
    {
        public string GenerateReceiptText(Sale sale, List<SaleDetail> items, AppSettings settings)
        {
            var sb = new StringBuilder();
            string line = new string('=', 32);
            string dash = new string('-', 32);

            sb.AppendLine("================================");
            sb.AppendLine(CenterText(settings.BusinessName.ToUpper(), 32));
            sb.AppendLine(CenterText(settings.BusinessLegalName, 32));
            sb.AppendLine(CenterText($"RUC: {settings.Ruc}", 32));
            sb.AppendLine(CenterText(settings.Address, 32));
            sb.AppendLine(CenterText($"Tel: {settings.Phone}", 32));
            sb.AppendLine(line);

            if (settings.EnableDgiFiscal && !string.IsNullOrEmpty(sale.FiscalInvoiceNumber))
            {
                sb.AppendLine(CenterText("*** FACTURA FISCAL DGI ***", 32));
                sb.AppendLine($"No. Fiscal: {sale.FiscalInvoiceNumber}");
                sb.AppendLine($"Resolución: {settings.FiscalAuthorizationResolution}");
            }
            else
            {
                sb.AppendLine($"Ticket Venta: {sale.InvoiceNumber}");
            }

            sb.AppendLine($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}");
            if (!string.IsNullOrEmpty(sale.TableName))
                sb.AppendLine($"Mesa / Área: {sale.TableName}");
            if (!string.IsNullOrEmpty(sale.WaiterName))
                sb.AppendLine($"Mesero: {sale.WaiterName}");
            sb.AppendLine($"Cliente: {sale.CustomerName}");
            if (!string.IsNullOrEmpty(sale.CustomerIdentification))
                sb.AppendLine($"Cédula/RUC: {sale.CustomerIdentification}");

            sb.AppendLine(dash);
            sb.AppendLine(String.Format("{0,-16}{1,5}{2,11}", "Cant/Desc", "P.U", "Total"));
            sb.AppendLine(dash);

            foreach (var item in items)
            {
                string name = item.ProductName.Length > 32 ? item.ProductName.Substring(0, 32) : item.ProductName;
                sb.AppendLine(name);
                string detail = String.Format("{0,-6}{1,10:N2}{2,16:N2}", $"{item.Quantity} {item.Unit}", item.UnitPrice, item.Total);
                sb.AppendLine(detail);
            }

            sb.AppendLine(dash);
            sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Subtotal:", sale.Subtotal));
            if (sale.Discount > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Descuento:", -sale.Discount));
            if (sale.IvaAmount > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "IVA DGI (15%):", sale.IvaAmount));
            if (sale.RetentionAmount > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Retención DGI:", -sale.RetentionAmount));

            sb.AppendLine(line);
            sb.AppendLine(String.Format("{0,-16}{1,16}", "TOTAL C$:", $"C$ {sale.TotalCordobas:N2}"));
            sb.AppendLine(String.Format("{0,-16}{1,16}", "TOTAL USD ($):", $"$ {sale.TotalUSD:N2}"));
            sb.AppendLine(line);

            sb.AppendLine($"Tipo Pago: {sale.PaymentType} ({sale.PaymentMethod})");
            if (sale.AmountReceivedCordobas > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Recibido C$:", sale.AmountReceivedCordobas));
            if (sale.AmountReceivedUSD > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Recibido USD ($):", sale.AmountReceivedUSD));
            if (sale.ChangeCordobas > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Cambio C$:", sale.ChangeCordobas));
            if (sale.ChangeUSD > 0)
                sb.AppendLine(String.Format("{0,-18}{1,14:N2}", "Cambio USD ($):", sale.ChangeUSD));

            sb.AppendLine(dash);
            sb.AppendLine(CenterText("¡Gracias por su compra!", 32));
            sb.AppendLine(CenterText("Pastelería y Cafetería Tradicional", 32));
            sb.AppendLine(dash);

            // Developer credits and LAFISE donation
            sb.AppendLine(CenterText("--- DESARROLLADO POR ---", 32));
            sb.AppendLine(CenterText(settings.DeveloperName, 32));
            sb.AppendLine(CenterText(settings.DeveloperLocation, 32));
            sb.AppendLine(CenterText(settings.DeveloperEmail, 32));
            sb.AppendLine(CenterText("☕ Invitame a un café - LAFISE:", 32));
            sb.AppendLine(CenterText($"C$: {settings.LafiseCordobasAccount} | $: {settings.LafiseUsdAccount}", 32));
            sb.AppendLine(CenterText($"Digital: {settings.LafiseDigitalAccount}", 32));
            sb.AppendLine("================================\n\n\n");

            return sb.ToString();
        }

        public string GenerateRawBtUrl(string receiptText)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(receiptText));
            return $"rawbt:data;base64,{base64}";
        }

        private string CenterText(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length >= width) return text.Substring(0, width);
            int leftPadding = (width - text.Length) / 2;
            return text.PadLeft(leftPadding + text.Length).PadRight(width);
        }
    }
}
