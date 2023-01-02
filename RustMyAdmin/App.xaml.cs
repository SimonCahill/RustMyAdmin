namespace RustMyAdmin;

using RustMyAdmin.Backend.Parsers;

public partial class App : Application {

    public App() {
        InitializeComponent();

        MainPage = new AppShell();
    }
}

