using Microsoft.Extensions.Logging;

namespace PasteleriaApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// PasteleriaApp Core Services
		builder.Services.AddSingleton<Data.DatabaseService>();
		builder.Services.AddSingleton<Services.Hardware.BarcodeScannerService>();
		builder.Services.AddSingleton<Services.Hardware.ScaleWeighingService>();
		builder.Services.AddSingleton<Services.Hardware.ThermalPrinterService>();
		builder.Services.AddSingleton<Services.Fiscal.DgiNicaraguaFiscalService>();
		builder.Services.AddSingleton<Services.Network.LocalServerService>();
		builder.Services.AddSingleton<Services.Network.ChildSyncService>();
		builder.Services.AddSingleton<Services.Cloud.GoogleDriveBackupService>();
		builder.Services.AddSingleton<Services.AI.GeminiAiService>();
		builder.Services.AddSingleton<Services.Reporting.BakeryReportsService>();

		return builder.Build();
	}
}
