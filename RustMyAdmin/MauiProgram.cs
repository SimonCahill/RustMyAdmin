using Microsoft.Extensions.Logging;

namespace RustMyAdmin;

using RustMyAdmin.Backend.Config;

public static class MauiProgram {

	public static MauiApp CreateMauiApp() {
		Task.Run(LoadConfigs);

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts => {
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	public static async Task LoadConfigs() {
		var configMan = ConfigManager.Instance;
		await configMan.LoadConfigAsync();
	}

}

