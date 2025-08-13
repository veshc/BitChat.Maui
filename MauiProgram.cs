using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using BitChat.Maui.Core.Interfaces;
using BitChat.Maui.Core.Services;
using BitChat.Maui.Presentation.ViewModels;
using BitChat.Maui.Presentation.Views;

namespace BitChat.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register services
		builder.Services.AddSingleton<ISettingsService, SettingsService>();

		// Register ViewModels
		builder.Services.AddTransient<SettingsViewModel>();

		// Register Views
		builder.Services.AddTransient<SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
