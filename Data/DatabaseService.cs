using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SQLite;
using PasteleriaApp.Models;

namespace PasteleriaApp.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(appData, "PasteleriaApp");
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
            }
            _dbPath = Path.Combine(appDir, "pasteleria.db3");
        }

        public string GetDatabasePath() => _dbPath;

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database != null)
                return _database;

            _database = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await InitializeDatabaseAsync();
            return _database;
        }

        private async Task InitializeDatabaseAsync()
        {
            if (_database == null) return;

            // Create all tables
            await _database.CreateTableAsync<Category>();
            await _database.CreateTableAsync<Product>();
            await _database.CreateTableAsync<Ingredient>();
            await _database.CreateTableAsync<Recipe>();
            await _database.CreateTableAsync<RecipeItem>();
            await _database.CreateTableAsync<ProductionOrder>();
            await _database.CreateTableAsync<ProductionItemUsed>();
            await _database.CreateTableAsync<Sale>();
            await _database.CreateTableAsync<SaleDetail>();
            await _database.CreateTableAsync<Customer>();
            await _database.CreateTableAsync<CustomerCreditMovement>();
            await _database.CreateTableAsync<RestaurantTable>();
            await _database.CreateTableAsync<Supplier>();
            await _database.CreateTableAsync<Purchase>();
            await _database.CreateTableAsync<PurchaseDetail>();
            await _database.CreateTableAsync<CashRegisterShift>();
            await _database.CreateTableAsync<AppSettings>();

            // Seed initial data if empty
            await SeedInitialDataAsync();
        }

        private async Task SeedInitialDataAsync()
        {
            if (_database == null) return;

            // 1. Settings
            var settings = await _database.Table<AppSettings>().FirstOrDefaultAsync();
            if (settings == null)
            {
                await _database.InsertAsync(new AppSettings());
            }

            // 2. Categories
            var catCount = await _database.Table<Category>().CountAsync();
            if (catCount == 0)
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Pasteles y Tortas", Icon = "🎂", Color = "#E91E63", SortOrder = 1 },
                    new Category { Name = "Repostería & Dulces", Icon = "🧁", Color = "#9C27B0", SortOrder = 2 },
                    new Category { Name = "Panadería Artesanal", Icon = "🥖", Color = "#FF9800", SortOrder = 3 },
                    new Category { Name = "Cafetería y Calientes", Icon = "☕", Color = "#795548", SortOrder = 4 },
                    new Category { Name = "Batidos & Bebidas Frías", Icon = "🥤", Color = "#00BCD4", SortOrder = 5 },
                    new Category { Name = "Platillos y Bocadillos", Icon = "🥪", Color = "#4CAF50", SortOrder = 6 },
                    new Category { Name = "Insumos y Materia Prima", Icon = "📦", Color = "#607D8B", SortOrder = 7 }
                };
                await _database.InsertAllAsync(categories);
            }

            // 3. Ingredients (Materia Prima)
            var ingCount = await _database.Table<Ingredient>().CountAsync();
            if (ingCount == 0)
            {
                var ingredients = new List<Ingredient>
                {
                    new Ingredient { Code = "INS-001", Name = "Harina de Trigo Todo Uso", Unit = "Lb", CostPerUnit = 22.50m, CurrentStock = 120, MinStock = 25 },
                    new Ingredient { Code = "INS-002", Name = "Azúcar Refinada Blanca", Unit = "Lb", CostPerUnit = 18.00m, CurrentStock = 80, MinStock = 20 },
                    new Ingredient { Code = "INS-003", Name = "Huevos de Granja", Unit = "Unidad", CostPerUnit = 6.50m, CurrentStock = 360, MinStock = 60 },
                    new Ingredient { Code = "INS-004", Name = "Mantequilla Pura sin Sal", Unit = "Lb", CostPerUnit = 85.00m, CurrentStock = 30, MinStock = 10 },
                    new Ingredient { Code = "INS-005", Name = "Leche Condensada", Unit = "Lata", CostPerUnit = 55.00m, CurrentStock = 45, MinStock = 12 },
                    new Ingredient { Code = "INS-006", Name = "Leche Evaporada", Unit = "Lata", CostPerUnit = 48.00m, CurrentStock = 45, MinStock = 12 },
                    new Ingredient { Code = "INS-007", Name = "Crema Chantilly Líquida", Unit = "Litros", CostPerUnit = 140.00m, CurrentStock = 15, MinStock = 5 },
                    new Ingredient { Code = "INS-008", Name = "Café Gourmet Grano Matagalpa", Unit = "Lb", CostPerUnit = 180.00m, CurrentStock = 25, MinStock = 8 },
                    new Ingredient { Code = "INS-009", Name = "Cacao Puro / Chocolate", Unit = "Lb", CostPerUnit = 95.00m, CurrentStock = 20, MinStock = 6 },
                    new Ingredient { Code = "INS-010", Name = "Pitahaya / Frutas Naturales", Unit = "Lb", CostPerUnit = 35.00m, CurrentStock = 40, MinStock = 10 }
                };
                await _database.InsertAllAsync(ingredients);
            }

            // 4. Products
            var prodCount = await _database.Table<Product>().CountAsync();
            if (prodCount == 0)
            {
                var products = new List<Product>
                {
                    new Product { Code = "PAS-001", Barcode = "74310001001", Name = "Pastel Tres Leches Tradicional (1 Lb)", Description = "Bizcocho bañado en mezcla de tres leches y decorado con chantilly y canela", CategoryId = 1, CategoryName = "Pasteles y Tortas", CostPrice = 280m, SalePrice = 480m, PriceUSD = 13.11m, Stock = 8, MinStock = 2, Unit = "Unidad", IsProduced = true },
                    new Product { Code = "PAS-002", Barcode = "74310001002", Name = "Pastel de Chocolate Fudge Supremo", Description = "Bizcocho húmedo de chocolate oscuro con ganache artesanal", CategoryId = 1, CategoryName = "Pasteles y Tortas", CostPrice = 320m, SalePrice = 550m, PriceUSD = 15.02m, Stock = 6, MinStock = 2, Unit = "Unidad", IsProduced = true },
                    new Product { Code = "PAS-003", Barcode = "74310001003", Name = "Porción Tres Leches de Caramelo", Description = "Porción individual con topping de dulce de leche nicaragüense", CategoryId = 2, CategoryName = "Repostería & Dulces", CostPrice = 35m, SalePrice = 75m, PriceUSD = 2.05m, Stock = 24, MinStock = 6, Unit = "Porción", IsProduced = true },
                    new Product { Code = "REP-001", Barcode = "74310001004", Name = "Picos Nicaragüenses Rellenos de Queso", Description = "Tradicional pan dulce triangular con queso dulce artesanal", CategoryId = 3, CategoryName = "Panadería Artesanal", CostPrice = 12m, SalePrice = 25m, PriceUSD = 0.68m, Stock = 50, MinStock = 15, Unit = "Unidad", IsProduced = true },
                    new Product { Code = "REP-002", Barcode = "74310001005", Name = "Quesadilla Tradicional al Horno", Description = "Elaborada con queso seco, crema y harina de arroz", CategoryId = 3, CategoryName = "Panadería Artesanal", CostPrice = 15m, SalePrice = 30m, PriceUSD = 0.82m, Stock = 40, MinStock = 10, Unit = "Unidad", IsProduced = true },
                    new Product { Code = "REP-003", Barcode = "74310001006", Name = "Torta de Maíz Dulce Pesada", Description = "Venta por libra o peso en báscula", CategoryId = 3, CategoryName = "Panadería Artesanal", CostPrice = 45m, SalePrice = 90m, PriceUSD = 2.46m, Stock = 18, MinStock = 5, Unit = "Lb", IsWeighable = true, IsProduced = true },
                    new Product { Code = "CAF-001", Barcode = "74310001007", Name = "Café Espresso Doble de Altura", Description = "Extracción doble de café gourmet de Matagalpa", CategoryId = 4, CategoryName = "Cafetería y Calientes", CostPrice = 15m, SalePrice = 45m, PriceUSD = 1.23m, Stock = 100, MinStock = 20, Unit = "Taza", IsDirectSale = true },
                    new Product { Code = "CAF-002", Barcode = "74310001008", Name = "Cappuccino Vainilla y Canela", Description = "Espresso con leche texturizada y esencia de vainilla", CategoryId = 4, CategoryName = "Cafetería y Calientes", CostPrice = 25m, SalePrice = 65m, PriceUSD = 1.77m, Stock = 100, MinStock = 20, Unit = "Taza", IsDirectSale = true },
                    new Product { Code = "BEB-001", Barcode = "74310001009", Name = "Batido Natural de Pitahaya con Leche", Description = "Pitahaya fresca granadina con leche condensada y hielo", CategoryId = 5, CategoryName = "Batidos & Bebidas Frías", CostPrice = 28m, SalePrice = 70m, PriceUSD = 1.91m, Stock = 50, MinStock = 10, Unit = "Vaso", IsDirectSale = true },
                    new Product { Code = "PLA-001", Barcode = "74310001010", Name = "Sandwich Croissant de Jamón y Queso Gouda", Description = "Croissant artesanal horneado con jamón ahumado y queso derretido", CategoryId = 6, CategoryName = "Platillos y Bocadillos", CostPrice = 55m, SalePrice = 120m, PriceUSD = 3.28m, Stock = 20, MinStock = 5, Unit = "Unidad", IsProduced = true }
                };
                await _database.InsertAllAsync(products);
            }

            // 5. Recipe for Pastel Tres Leches
            var recCount = await _database.Table<Recipe>().CountAsync();
            if (recCount == 0)
            {
                var recipe = new Recipe
                {
                    ProductId = 1,
                    ProductName = "Pastel Tres Leches Tradicional (1 Lb)",
                    YieldQuantity = 1,
                    YieldUnit = "Unidad",
                    PreparationInstructions = "1. Batir claras a punto de nieve con azúcar. 2. Incorporar yemas y harina tamizada. 3. Hornear a 180°C por 30 min. 4. Bañar con mezcla de leches y refrigerar 6h. 5. Decorar con crema chantilly.",
                    EstimatedCost = 245.00m,
                    LaborCostPercentage = 15m,
                    FinalUnitCost = 280.00m
                };
                await _database.InsertAsync(recipe);

                var recipeItems = new List<RecipeItem>
                {
                    new RecipeItem { RecipeId = recipe.Id, IngredientId = 1, IngredientName = "Harina de Trigo Todo Uso", QuantityRequired = 1.0m, Unit = "Lb", UnitCost = 22.50m, SubtotalCost = 22.50m },
                    new RecipeItem { RecipeId = recipe.Id, IngredientId = 2, IngredientName = "Azúcar Refinada Blanca", QuantityRequired = 0.75m, Unit = "Lb", UnitCost = 18.00m, SubtotalCost = 13.50m },
                    new RecipeItem { RecipeId = recipe.Id, IngredientId = 3, IngredientName = "Huevos de Granja", QuantityRequired = 6m, Unit = "Unidad", UnitCost = 6.50m, SubtotalCost = 39.00m },
                    new RecipeItem { RecipeId = recipe.Id, IngredientId = 5, IngredientName = "Leche Condensada", QuantityRequired = 1m, Unit = "Lata", UnitCost = 55.00m, SubtotalCost = 55.00m },
                    new RecipeItem { RecipeId = recipe.Id, IngredientId = 6, IngredientName = "Leche Evaporada", QuantityRequired = 1m, Unit = "Lata", UnitCost = 48.00m, SubtotalCost = 48.00m },
                    new RecipeItem { RecipeId = recipe.Id, IngredientId = 7, IngredientName = "Crema Chantilly Líquida", QuantityRequired = 0.5m, Unit = "Litros", UnitCost = 140.00m, SubtotalCost = 70.00m }
                };
                await _database.InsertAllAsync(recipeItems);
            }

            // 6. Restaurant Tables
            var tableCount = await _database.Table<RestaurantTable>().CountAsync();
            if (tableCount == 0)
            {
                var tables = new List<RestaurantTable>
                {
                    new RestaurantTable { Name = "Mesa 1 (Terraza Colonial)", Capacity = 4, Status = "Disponible" },
                    new RestaurantTable { Name = "Mesa 2 (Terraza Colonial)", Capacity = 4, Status = "Disponible" },
                    new RestaurantTable { Name = "Mesa 3 (Salón Principal)", Capacity = 2, Status = "Disponible" },
                    new RestaurantTable { Name = "Mesa 4 (Salón Principal)", Capacity = 6, Status = "Disponible" },
                    new RestaurantTable { Name = "Mesa 5 (Jardín)", Capacity = 4, Status = "Disponible" },
                    new RestaurantTable { Name = "Barra 1 (Cafetería)", Capacity = 2, Status = "Disponible" },
                    new RestaurantTable { Name = "Para Llevar / PickUp", Capacity = 1, Status = "Disponible" }
                };
                await _database.InsertAllAsync(tables);
            }

            // 7. Customers
            var custCount = await _database.Table<Customer>().CountAsync();
            if (custCount == 0)
            {
                var customers = new List<Customer>
                {
                    new Customer { FullName = "Cliente General de Mostrador", IdentificationNumber = "000-000000-0000A", Phone = "", Email = "", CreditLimit = 0, CurrentCreditBalance = 0 },
                    new Customer { FullName = "María Fernanda Gómez", IdentificationNumber = "201-150892-0003K", Ruc = "2011508920003K", Phone = "+505 8744-1234", Email = "maria.gomez@gmail.com", Address = "Granada, Calle El Consulado", Birthday = DateTime.Now.AddDays(2), CreditLimit = 3000, CurrentCreditBalance = 0 },
                    new Customer { FullName = "Carlos Alberto Morales", IdentificationNumber = "001-050488-0012W", Ruc = "0010504880012W", Phone = "+505 8899-7711", Email = "carlos.morales@hotmail.com", Address = "Managua, Las Colinas", Birthday = DateTime.Now.AddDays(15), CreditLimit = 5000, CurrentCreditBalance = 1200 },
                    new Customer { FullName = "Sofía Elena Chamorro", IdentificationNumber = "201-120995-0004P", Ruc = "", Phone = "+505 8622-4455", Email = "sofia.chamorro@yahoo.com", Address = "Granada, La Islita", Birthday = DateTime.Now, CreditLimit = 2000, CurrentCreditBalance = 0 }
                };
                await _database.InsertAllAsync(customers);
            }

            // 8. Suppliers
            var supCount = await _database.Table<Supplier>().CountAsync();
            if (supCount == 0)
            {
                var suppliers = new List<Supplier>
                {
                    new Supplier { CompanyName = "Distribuidora Láctea La Perfecta S.A.", ContactName = "Roberto Meza", Ruc = "J0310000888999", Phone = "+505 2255-0000", Email = "ventas@laperfecta.com.ni", Address = "Carretera Norte, Managua", PaymentTermsDays = 30, CurrentDebt = 0 },
                    new Supplier { CompanyName = "Harinas de Nicaragua S.A. (MONISA)", ContactName = "Elena Rocha", Ruc = "J0310000555666", Phone = "+505 2552-3000", Email = "pedidos@monisa.com.ni", Address = "Granada, Km 45", PaymentTermsDays = 15, CurrentDebt = 1500 }
                };
                await _database.InsertAllAsync(suppliers);
            }

            // 9. Initial Cash Register Shift
            var shiftCount = await _database.Table<CashRegisterShift>().CountAsync();
            if (shiftCount == 0)
            {
                var shift = new CashRegisterShift
                {
                    ShiftNumber = 1,
                    OpenedBy = "Maxwell Chacón",
                    OpenedAt = DateTime.Now,
                    InitialCashCordobas = 1000,
                    InitialCashUSD = 20,
                    Status = "Abierto",
                    Notes = "Turno matutino abierto por defecto."
                };
                await _database.InsertAsync(shift);
            }
        }

        #region Export and Import Database
        public async Task<string> ExportDatabaseJsonAsync()
        {
            var db = await GetConnectionAsync();
            var backupData = new Dictionary<string, object>
            {
                ["categories"] = await db.Table<Category>().ToListAsync(),
                ["products"] = await db.Table<Product>().ToListAsync(),
                ["ingredients"] = await db.Table<Ingredient>().ToListAsync(),
                ["recipes"] = await db.Table<Recipe>().ToListAsync(),
                ["recipeItems"] = await db.Table<RecipeItem>().ToListAsync(),
                ["productionOrders"] = await db.Table<ProductionOrder>().ToListAsync(),
                ["productionItemsUsed"] = await db.Table<ProductionItemUsed>().ToListAsync(),
                ["sales"] = await db.Table<Sale>().ToListAsync(),
                ["saleDetails"] = await db.Table<SaleDetail>().ToListAsync(),
                ["customers"] = await db.Table<Customer>().ToListAsync(),
                ["creditMovements"] = await db.Table<CustomerCreditMovement>().ToListAsync(),
                ["tables"] = await db.Table<RestaurantTable>().ToListAsync(),
                ["suppliers"] = await db.Table<Supplier>().ToListAsync(),
                ["purchases"] = await db.Table<Purchase>().ToListAsync(),
                ["purchaseDetails"] = await db.Table<PurchaseDetail>().ToListAsync(),
                ["shifts"] = await db.Table<CashRegisterShift>().ToListAsync(),
                ["settings"] = await db.Table<AppSettings>().ToListAsync(),
                ["exportedAt"] = DateTime.Now
            };

            return JsonSerializer.Serialize(backupData, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<bool> ImportDatabaseJsonAsync(string json)
        {
            try
            {
                var db = await GetConnectionAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("products", out var prods))
                {
                    var products = JsonSerializer.Deserialize<List<Product>>(prods.GetRawText());
                    if (products != null && products.Any())
                    {
                        await db.DeleteAllAsync<Product>();
                        await db.InsertAllAsync(products);
                    }
                }

                if (root.TryGetProperty("categories", out var cats))
                {
                    var categories = JsonSerializer.Deserialize<List<Category>>(cats.GetRawText());
                    if (categories != null && categories.Any())
                    {
                        await db.DeleteAllAsync<Category>();
                        await db.InsertAllAsync(categories);
                    }
                }

                if (root.TryGetProperty("ingredients", out var ings))
                {
                    var ingredients = JsonSerializer.Deserialize<List<Ingredient>>(ings.GetRawText());
                    if (ingredients != null && ingredients.Any())
                    {
                        await db.DeleteAllAsync<Ingredient>();
                        await db.InsertAllAsync(ingredients);
                    }
                }

                if (root.TryGetProperty("recipes", out var recs))
                {
                    var recipes = JsonSerializer.Deserialize<List<Recipe>>(recs.GetRawText());
                    if (recipes != null && recipes.Any())
                    {
                        await db.DeleteAllAsync<Recipe>();
                        await db.InsertAllAsync(recipes);
                    }
                }

                if (root.TryGetProperty("customers", out var custs))
                {
                    var customers = JsonSerializer.Deserialize<List<Customer>>(custs.GetRawText());
                    if (customers != null && customers.Any())
                    {
                        await db.DeleteAllAsync<Customer>();
                        await db.InsertAllAsync(customers);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
