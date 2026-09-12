# 🍰 PasteleriaApp - Sistema POS & Gestión Integral de Pastelería (.NET MAUI Multiplataforma)

> **Sistema de Punto de Venta (POS), Producción, Inventario y Facturación Fiscal DGI adaptado para Pastelerías, Panaderías, Cafeterías y Reposterías.**
> Compatible con **Windows Desktop, Android e iOS** desarrollado con **.NET MAUI Blazor Hybrid (.NET 10)** y diseño **Material UI**.

---

## 👨‍💻 Autoría, Donaciones & Licencia

- **Autor y Desarrollador:** Maxwell Chacón
- **Cédula de Identidad:** `201-290495-0006A`
- **Ubicación:** Granada - Managua, Nicaragua 🇳🇮
- **Correo de Contacto Oficial:** [ing.chacon.maxwell@gmail.com](mailto:ing.chacon.maxwell@gmail.com)

### ☕ ¡Invítame a un Café! - Donaciones Oficiales (Banco LAFISE Bancentro)
*Si este software te resulta de gran utilidad para tu negocio, puedes apoyar su desarrollo:*
- **Banco:** Banco LAFISE Bancentro (Nicaragua)
- **Titular:** Maxwell Chacón
- 🟢 **Cuenta Amigo en Córdobas (C$):** `138027529`
- 🔵 **Cuenta Amigo en Dólares ($):** `133258435`
- 📱 **Cuenta Digital LAFISE:** `133238477`

### ⚖️ Términos de Licencia
- **Uso Permitido:** Libre para su uso en negocios comerciales, pequeñas empresas, panaderías, cafeterías, reposterías, uso propio o fines educativos.
- **Restricciones:** Prohibida su venta, redistribución, comercialización o licenciamiento no autorizado sin el consentimiento expreso y por escrito del autor (**Maxwell Chacón**).

> 💡 *¿Necesitas darle continuidad a esta app, soporte técnico personalizado o agregar nuevas características a la medida? Contáctame al correo:* [ing.chacon.maxwell@gmail.com](mailto:ing.chacon.maxwell@gmail.com)

---

## 🚀 Características Principales

### 1. 🛍️ Punto de Venta Táctil & Rápido (POS)
- Catálogo visual clasificado por categorías (*Pasteles y Tortas, Repostería & Dulces, Panadería Artesanal, Cafetería y Calientes, Batidos & Bebidas Frías, Platillos y Bocadillos*).
- Multimoneda nativa: Moneda predeterminada **Córdoba Nicaragüense (C$)** con soporte y conversión simultánea en **Dólares Americanos (USD)**.
- Múltiples formas de pago: Efectivo C$/USD, Tarjeta Débito/Crédito, Transferencia Banco LAFISE y Venta al Crédito (CxC).
- Asignación rápida de mesas (Terraza Colonial, Salón Principal, Jardín, Barra) o modo Mostrador / Para Llevar.

### 2. 🥣 Control de Elaboración, Recetas & Órdenes de Producción
- Registro de recetas estándar de pastelería (*Pastel Tres Leches, Pastel Fudge de Chocolate, Picos Nicaragüenses, Quesadillas, Tortas*).
- Deducción automática de materia prima (*Harina, Azúcar, Huevos, Mantequilla, Leche condensada, Crema chantilly, Cacao*) al hornear y completar órdenes de producción.
- Trazabilidad por código de lote, costo de mano de obra y costo unitario final.

### 3. ⚖️ Compatibilidad Total con Hardware de Punto de Venta
- **Pistolas Escáner de Código de Barras y QR (USB / Bluetooth / Inalámbricas):** Algoritmo de detección por ráfaga rápida a alta velocidad (**<60ms**) que descarta el tipeo manual humano. Compatible con Honeywell, Zebra, Netum, Datalogic y modelos genéricos.
- **Cámara Web / Dispositivo Móvil:** Escáner óptico integrado.
- **Básculas y Balanzas Digitales de Mostrador:** Protocolo serie **RS-232 / USB HID** con lectura continua, botón de tara, reseteo a cero y cálculo automático de precio por peso (Kg / Lb). Compatible con Torrey, CAS, Toledo y Cardinal.
- **Impresoras Térmicas (58mm / 80mm):** Formateo ESC/POS, impresión directa en red/USB y compatibilidad móvil con **Raw BT Android Intent**.

### 4. 🧾 Facturación Fiscal DGI Nicaragua
- Cálculo y desglose oficial de **IVA 15%** y Retenciones de ley.
- RUC del negocio, Cédula del cliente, Resolución de autorización DGI y numeración consecutiva fiscal oficial.

### 5. 🌐 Arquitectura de Red: Servidor Central & Terminales Hijas (Mesh Local)
- **Servidor Central REST Embebido:** Servidor HTTP Kestrel autónomo con token de seguridad (`X-Auth-Token`) para recibir ventas y sincronizar catálogos en red local.
- **Modo Terminal Hija:** Permite configurar múltiples dispositivos (teléfonos, tablets, terminales táctiles) que transmiten en tiempo real hacia el servidor central.
- **Probador Swagger / OpenAPI Embebido:** Interfaz interactiva integrada en la app para consultar y probar todos los endpoints REST en tiempo real.
- **Sincronización Opcional Firebase:** Conexión opcional a Cloud Firestore.

### 6. ☁️ Respaldos Automáticos en Google Drive & Base de Datos SQLite
- Base de datos SQLite local (`pasteleria.db3`) con migraciones automáticas y semillero de datos iniciales.
- Tarea de respaldo automático programado a una hora específica del día (ej. 22:00) y exportación/importación completa en formato JSON.

### 7. ✨ Inteligencia Artificial Integrada (Google Gemini SDK & Modo Local Offline)
- Integración con **Google Gemini SDK** (`gemini-2.5-flash`) para asesoría de repostería, optimización de costos de recetas y promociones de fidelización.
- Algoritmo heurístico local para **Proyección de Demanda Estimada**: analiza existencias críticas, volumen histórico de ventas y sugiere el tamaño del lote de horneado para evitar quiebres de stock.

---

## 📊 Los 10 Reportes Especializados de Pastelería

El sistema incluye 10 reportes completos con multi-filtros por rango de fechas y exportación a formato CSV:
1. **Ventas y Facturación Diaria / Periódica:** Consolidado de ingresos, IVA DGI, formas de pago (C$, USD, Tarjetas, LAFISE, Crédito).
2. **Costos de Recetas y Margen de Utilidad por Producto:** Análisis de costo de materia prima vs. precio de venta y margen bruto.
3. **Control de Producción y Rendimiento de Lotes:** Registro de órdenes de horneado, insumos consumidos vs. productos obtenidos.
4. **Kardex e Inventario de Materia Prima / Insumos:** Stock de harina, lácteos, huevos, empaques y alertas de reorden.
5. **Proyección de Demanda Estimada con IA (Gemini):** Análisis predictivo para planificar la producción semanal.
6. **Ventas al Crédito y Cuentas por Cobrar (CxC):** Saldos pendientes de clientes, abonos registrados y límites de crédito.
7. **Compras e Insumos al Crédito (Cuentas por Pagar - CxP):** Proveedores de ingredientes, facturas y vencimientos.
8. **Ranking de Productos Más Vendidos (Top ABC):** Los pasteles, panes, cafés y batidos más populares por ingresos y volumen.
9. **Agenda de Cumpleaños de Clientes del Mes & Fidelización:** Cumpleañeros registrados para campañas y pasteles personalizados.
10. **Auditoría de Comandas, Mesas y Rendimiento de Meseros:** Rotación de mesas en terraza/salón y ventas por mesero.

---

## 🛠️ Requisitos e Instalación

### Requisitos del Sistema
- .NET 10 SDK
- Carga de trabajo .NET MAUI (`dotnet workload install maui`)
- Windows 10/11 (para compilación Windows Desktop)
- Android SDK 34+ (para compilación Android)

### Compilación y Ejecución

```bash
# Restaurar dependencias
dotnet restore

# Compilar y ejecutar en Windows Desktop
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0

# Compilar para Android (Generación de APK)
dotnet build -f net10.0-android -c Release
```

---

## 📁 Estructura del Proyecto

```
PasteleriaApp/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor       # Layout Material UI con barra superior y selector de tema
│   │   └── NavMenu.razor          # Menú lateral interactivo con iconos de colores
│   └── Pages/
│       ├── Pos.razor              # Punto de Venta, categorías, báscula y escáner
│       ├── TablesPage.razor       # Mesas, terraza colonial y comandas
│       ├── ProductionPage.razor   # Recetas y órdenes de horneado
│       ├── ProductsPage.razor     # Catálogo de productos y precios
│       ├── InventoryPage.razor    # Insumos y Kardex
│       ├── SalesPage.razor        # Historial de ventas y facturas DGI
│       ├── PurchasesPage.razor    # Compras a proveedores
│       ├── CustomersPage.razor    # Clientes, cumpleaños y abonos
│       ├── ReportsPage.razor      # Los 10 reportes especializados
│       ├── AiAssistantPage.razor  # Asistente Gemini y demanda proyectada
│       ├── SwaggerPage.razor      # Probador interactivo de endpoints REST
│       ├── SettingsPage.razor     # Configuración del negocio y red
│       └── AboutPage.razor        # Autoría de Maxwell Chacón y donaciones LAFISE
├── Data/
│   └── DatabaseService.cs         # SQLite local, migraciones y semillero de datos
├── Models/
│   └── Models.cs                  # Entidades de base de datos
├── Services/
│   ├── AI/GeminiAiService.cs      # Gemini SDK y demanda proyectada
│   ├── Cloud/GoogleDriveBackup.cs # Respaldos automáticos
│   ├── Fiscal/DgiFiscalService.cs # Cálculo IVA 15% DGI Nicaragua
│   ├── Hardware/
│   │   ├── BarcodeScannerService  # Detección de ráfaga rápida <60ms
│   │   ├── ScaleWeighingService   # Balanzas RS-232 / USB con tara y cero
│   │   └── ThermalPrinterService  # Impresión ESC/POS y RawBT
│   ├── Network/
│   │   ├── LocalServerService.cs  # Servidor HTTP Kestrel y Swagger
│   │   └── ChildSyncService.cs    # Sincronizador de terminales hijas
│   └── Reporting/BakeryReports.cs # Generador de los 10 reportes
└── wwwroot/
    ├── css/material.css           # Sistema de diseño Material UI
    └── js/app.js                  # Interop JS, audio beeps e impresión
```

---

## 📄 Créditos Finales
**PasteleriaApp** ha sido diseñado y desarrollado con orgullo por **Maxwell Chacón** en **Granada - Managua, Nicaragua**.
Para soporte, mejoras o proyectos empresariales: [ing.chacon.maxwell@gmail.com](mailto:ing.chacon.maxwell@gmail.com).
