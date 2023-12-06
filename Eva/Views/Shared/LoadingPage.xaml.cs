namespace Eva.Views.Shared;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
	}

    public LoadingPage(string titulo, string mensaje)
    {
        InitializeComponent();

        BindingContext = this;
        this.Title = titulo;
        this.lblMensaje.Text = mensaje;
    }
}