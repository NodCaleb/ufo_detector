using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UfoDetector.Services;
using UfoDetector.ViewModels;
using UfoDetector.Views;

namespace UfoDetector;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if ANDROID
			.UseMauiCommunityToolkit()
#endif
			.UseSkiaSharp()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("ShareTechMono.ttf", "ShareTechMono");
			});

		// Services
		builder.Services.AddSingleton<IDispatcherTimer>(
			_ => Application.Current!.Dispatcher.CreateTimer());
		builder.Services.AddSingleton<ISensorTickService, SensorTickService>();
		builder.Services.AddSingleton<ITransitionOrchestrator, TransitionOrchestrator>();

		// ViewModel + Page (singleton so DI resolves them once)
		builder.Services.AddSingleton<DetectorViewModel>();
		builder.Services.AddSingleton<DetectorPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
