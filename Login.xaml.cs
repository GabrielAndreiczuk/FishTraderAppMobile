namespace FishTraderAppMobile;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
	}

    private void btnEntrar_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new MainPage();
    }

    private void btnCadastro_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Cadastro();
    }    
}