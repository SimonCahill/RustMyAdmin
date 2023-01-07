using Microsoft.Extensions.Logging;

namespace RustMyAdmin;

using RustMyAdmin.Backend;
using RustMyAdmin.Backend.Config;
using RustMyAdmin.Backend.Parsers;

public static class MauiProgram {

    static readonly DirectoryInfo LanguagesDir = new DirectoryInfo("Resources/Translations");

    public static MauiApp CreateMauiApp() {
        Task.Run(LoadLanguages);
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

    public static void LoadLanguages() {
        var langFiles = LanguagesDir.EnumerateFiles("*.lang");
        Console.WriteLine(langFiles);
        foreach (var lang in langFiles) {
            new LanguageParser(lang).ParseLangFile();
        }
    }

}

