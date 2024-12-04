namespace FishTraderAppMobile;

public partial class Cadastro : ContentPage
{
	public Cadastro()
	{
		InitializeComponent();
	}

    private void btnLogin_Clicked(object sender, EventArgs e)
    {
		App.Current.MainPage = new Login();
    }

    private void btnCadastrar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new MainPage();
    }
}