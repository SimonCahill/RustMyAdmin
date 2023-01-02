namespace RustMyAdmin;

public partial class About : ContentPage {


	public About() {
		InitializeComponent();
	}

	int timesClicked = 0;

	void Button_Clicked(System.Object sender, System.EventArgs e) {
		((Button)sender).Text = $"You clicked { ++timesClicked } time{ (timesClicked == 1 ? "" : "s") }";
	}
}
