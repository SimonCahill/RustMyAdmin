namespace RustMyAdmin;

public partial class App : Application {

	const string TranslationDirPath = "Resources/Translations/";
	const string LanguageFileExt	= ".lang";

	internal List<FileInfo> LanguageFiles { get; private set; } = default;

	public App() {
		InitializeComponent();

		MainPage = new AppShell();
	}


	
}

