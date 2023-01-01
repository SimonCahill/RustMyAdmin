using RustMyAdmin.Backend.Parsers;

namespace RustMyAdmin;

public partial class AppShell : Shell {

    const string TranslationDirPath = "Resources/Translations/";
    const string LanguageFileExt	= ".lang";

    internal List<FileInfo> LanguageFiles { get; private set; } = default;
    internal List<LanguageParser> Languages { get; set;}

    public AppShell() {
        InitializeComponent();
    }

    /// <summary>
    /// Searches for valid language files in the translation path.
    /// </summary>
    private void SearchAllLanguageFiles() {
        var langDir = new DirectoryInfo(TranslationDirPath);
        LanguageFiles = langDir.EnumerateFiles($"*{ LanguageFileExt }").ToList();

        // Filter out any empty files
        LanguageFiles.RemoveAll(f => f.Length < 10); // 10 bytes is a good number
    }

    /// <summary>
    /// Attempts to load all the languages found in the translation file path
    /// </summary>
    private void LoadLanguages() {
        foreach (var file in LanguageFiles) {
            var langParser = new LanguageParser(file);
            if (!langParser.ParseLangFile()) { continue; }

            Languages.Add(langParser);
        }
    }

    void OnAppShellLoaded(System.Object sender, System.EventArgs e) {
        SearchAllLanguageFiles();
        LoadLanguages(); // possible future optimisation: only load required language on startup
    }
}

