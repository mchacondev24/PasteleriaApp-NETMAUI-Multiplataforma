using Android.App;
using Android.Runtime;

namespace PasteleriaApp;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
		AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[ANDROID UNHANDLED EXCEPTION] {args.Exception}");
			args.Handled = true;
		};

		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[APPDOMAIN UNHANDLED EXCEPTION] {args.ExceptionObject}");
		};

		TaskScheduler.UnobservedTaskException += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[TASK UNOBSERVED EXCEPTION] {args.Exception}");
			args.SetObserved();
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
