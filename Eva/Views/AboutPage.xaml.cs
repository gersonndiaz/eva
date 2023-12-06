namespace Eva.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
	}

    private async void btnAbout_Clicked(object sender, EventArgs e)
    {
        var uri = new Uri("https://www.ckelar.cl/");
        await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}