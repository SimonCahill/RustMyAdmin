namespace RustMyAdmin;

public partial class About : ContentPage {

	private int TimesClicked { get; set; } = 0;

	public About() {
		InitializeComponent();
	}

	void OnButtonClicked(System.Object sender, System.EventArgs e) {
		((Button)sender).Text = String.Format("Clicked {0} time{1}", ++TimesClicked, (TimesClicked) == 1 ? "" : "s");
	}
}
