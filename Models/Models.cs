using System;
using SQLite;

namespace PasteleriaApp.Models
{
    [Table("Categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "🍰";
        public string Color { get; set; } = "#E91E63";
        public int SortOrder { get; set; } = 0;
    }

    [Table("Products")]
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Code { get; set; } = string.Empty;
        [Indexed]
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Indexed]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal CostPrice { get; set; } = 0;
        public decimal SalePrice { get; set; } = 0;
        public decimal PriceUSD { get; set; } = 0;
        public decimal Stock { get; set; } = 0;
        public decimal MinStock { get; set; } = 5;
        public string Unit { get; set; } = "Unidad"; // Unidad, Lb, Kg, Porción, Vaso
        public bool IsWeighable { get; set; } = false;
        public bool IsDirectSale { get; set; } = true;
        public bool IsProduced { get; set; } = false; // Elaborado en pastelería
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Ingredients")]
    public class Ingredient
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = "Lb"; // Lb, Kg, Gramos, Litros, Onzas, Huevos, Unidad
        public decimal CostPerUnit { get; set; } = 0;
        public decimal CurrentStock { get; set; } = 0;
        public decimal MinStock { get; set; } = 5;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Recipes")]
    public class Recipe
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal YieldQuantity { get; set; } = 1;
        public string YieldUnit { get; set; } = "Unidad";
        public string PreparationInstructions { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; } = 0;
        public decimal LaborCostPercentage { get; set; } = 15; // 15% mano de obra
        public decimal FinalUnitCost { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("RecipeItems")]
    public class RecipeItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int RecipeId { get; set; }
        [Indexed]
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; } = 0;
        public string Unit { get; set; } = "Lb";
        public decimal UnitCost { get; set; } = 0;
        public decimal SubtotalCost { get; set; } = 0;
    }

    [Table("ProductionOrders")]
    public class ProductionOrder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string OrderNumber { get; set; } = string.Empty;
        [Indexed]
        public int RecipeId { get; set; }
        [Indexed]
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityToProduce { get; set; } = 1;
        public decimal ActualQuantityProduced { get; set; } = 0;
        public string BatchCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendiente"; // Pendiente, EnProceso, Completado, Cancelado
        public string Notes { get; set; } = string.Empty;
        public decimal TotalCost { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
    }

    [Table("ProductionItemsUsed")]
    public class ProductionItemUsed
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ProductionOrderId { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal QuantityPlanned { get; set; } = 0;
        public decimal QuantityActual { get; set; } = 0;
        public string Unit { get; set; } = "Lb";
        public decimal UnitCost { get; set; } = 0;
        public decimal SubtotalCost { get; set; } = 0;
    }

    [Table("Sales")]
    public class Sale
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string InvoiceNumber { get; set; } = string.Empty;
        public string FiscalInvoiceNumber { get; set; } = string.Empty;
        public string DgiRuc { get; set; } = string.Empty;
        [Indexed]
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = "Cliente General";
        public string CustomerIdentification { get; set; } = string.Empty;
        public int? TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string WaiterName { get; set; } = string.Empty;
        public int? ShiftId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public decimal Subtotal { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal IvaAmount { get; set; } = 0;
        public decimal RetentionAmount { get; set; } = 0;
        public decimal TotalCordobas { get; set; } = 0;
        public decimal TotalUSD { get; set; } = 0;
        public decimal ExchangeRate { get; set; } = 36.62m;
        public string PaymentType { get; set; } = "Contado"; // Contado, Credito, Mixto
        public string PaymentMethod { get; set; } = "Efectivo"; // Efectivo, Tarjeta, Transferencia_LAFISE, Mixto
        public decimal AmountReceivedCordobas { get; set; } = 0;
        public decimal AmountReceivedUSD { get; set; } = 0;
        public decimal ChangeCordobas { get; set; } = 0;
        public decimal ChangeUSD { get; set; } = 0;
        public string Status { get; set; } = "Completada"; // Completada, Anulada, Pendiente
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("SaleDetails")]
    public class SaleDetail
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int SaleId { get; set; }
        [Indexed]
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal UnitCost { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal IvaRate { get; set; } = 0.15m;
        public decimal Subtotal { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string Unit { get; set; } = "Unidad";
        public string Notes { get; set; } = string.Empty;
    }

    [Table("Customers")]
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [Indexed]
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public decimal CreditLimit { get; set; } = 0;
        public decimal CurrentCreditBalance { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("CustomerCreditMovements")]
    public class CustomerCreditMovement
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int CustomerId { get; set; }
        public int? SaleId { get; set; }
        public string MovementType { get; set; } = "Cargo_Venta"; // Cargo_Venta, Abono_Pago, Ajuste
        public decimal AmountCordobas { get; set; } = 0;
        public decimal AmountUSD { get; set; } = 0;
        public decimal BalanceAfter { get; set; } = 0;
        public string ReferenceDoc { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("RestaurantTables")]
    public class RestaurantTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; } = 4;
        public string Status { get; set; } = "Disponible"; // Disponible, Ocupada, Reservada, EnLimpieza
        public int? CurrentSaleId { get; set; }
        public string CurrentWaiter { get; set; } = string.Empty;
        public DateTime? OpenedAt { get; set; }
    }

    [Table("Suppliers")]
    public class Supplier
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int PaymentTermsDays { get; set; } = 15;
        public decimal CurrentDebt { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Purchases")]
    public class Purchase
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Indexed]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public decimal TotalCordobas { get; set; } = 0;
        public decimal TotalUSD { get; set; } = 0;
        public decimal ExchangeRate { get; set; } = 36.62m;
        public string PaymentType { get; set; } = "Contado"; // Contado, Credito
        public string Status { get; set; } = "Recibida"; // Recibida, Pendiente, Anulada
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("PurchaseDetails")]
    public class PurchaseDetail
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PurchaseId { get; set; }
        [Indexed]
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 0;
        public string Unit { get; set; } = "Lb";
        public decimal UnitCost { get; set; } = 0;
        public decimal Subtotal { get; set; } = 0;
    }

    [Table("CashRegisterShifts")]
    public class CashRegisterShift
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ShiftNumber { get; set; } = 1;
        public string OpenedBy { get; set; } = "Administrador";
        public string ClosedBy { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }
        public decimal InitialCashCordobas { get; set; } = 500;
        public decimal InitialCashUSD { get; set; } = 0;
        public decimal TotalSalesCashCordobas { get; set; } = 0;
        public decimal TotalSalesCashUSD { get; set; } = 0;
        public decimal TotalSalesCardCordobas { get; set; } = 0;
        public decimal TotalSalesTransferCordobas { get; set; } = 0;
        public decimal TotalSalesCreditCordobas { get; set; } = 0;
        public decimal FinalCashExpectedCordobas { get; set; } = 0;
        public decimal FinalCashCountedCordobas { get; set; } = 0;
        public decimal DifferenceCordobas { get; set; } = 0;
        public string Status { get; set; } = "Abierto"; // Abierto, Cerrado
        public string Notes { get; set; } = string.Empty;
    }

    [Table("AppSettings")]
    public class AppSettings
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;
        public string BusinessName { get; set; } = "Pastelería & Repostería Granada";
        public string BusinessLegalName { get; set; } = "Pastelería Granada S.A.";
        public string Ruc { get; set; } = "J0310000123456";
        public string Phone { get; set; } = "+505 8888-9999";
        public string Email { get; set; } = "contacto@pasteleriagranada.com";
        public string Address { get; set; } = "Calle Real Xalteva, Granada, Nicaragua";
        public string City { get; set; } = "Granada, Nicaragua";
        public string LogoUrl { get; set; } = "";
        public string DefaultCurrency { get; set; } = "C$";
        public decimal UsdExchangeRate { get; set; } = 36.62m;
        public decimal IvaRate { get; set; } = 0.15m;
        public bool EnableDgiFiscal { get; set; } = true;
        public string FiscalAuthorizationResolution { get; set; } = "DGI-RES-2026-98745";
        public string FiscalPrefix { get; set; } = "FISC-001-";
        public int CurrentFiscalNumber { get; set; } = 1001;
        public int FiscalLimitNumber { get; set; } = 50000;
        public string DeveloperName { get; set; } = "Maxwell Chacón";
        public string DeveloperEmail { get; set; } = "ing.chacon.maxwell@gmail.com";
        public string DeveloperCedula { get; set; } = "201-290495-0006A";
        public string DeveloperLocation { get; set; } = "Granada - Managua, Nicaragua";
        public string LafiseCordobasAccount { get; set; } = "138027529";
        public string LafiseUsdAccount { get; set; } = "133258435";
        public string LafiseDigitalAccount { get; set; } = "133238477";
        public string LicenseTerms { get; set; } = "Libre para su uso en negocios comerciales, pequeñas empresas, uso propio o fines educativos. Prohibida su venta, redistribución, comercialización o licenciamiento no autorizado sin el consentimiento expreso y por escrito del autor (Maxwell Chacón).";
        public string BackupHour { get; set; } = "22:00";
        public bool AutoBackupEnabled { get; set; } = true;
        public string ServerMode { get; set; } = "Standalone"; // Standalone, CentralServer, ChildClient
        public string CentralServerUrl { get; set; } = "http://192.168.1.100:5055";
        public string CentralServerToken { get; set; } = "PASTELERIA_TOKEN_2026";
        public int LocalServerPort { get; set; } = 5055;
        public string GeminiApiKey { get; set; } = "";
        public bool FirebaseEnabled { get; set; } = false;
        public string FirebaseProjectId { get; set; } = "";
        public string SelectedTheme { get; set; } = "light"; // light, dark, pastel, coffee
        public string PrimaryColor { get; set; } = "#E91E63"; // Material Pink / Berry default
    }
}
