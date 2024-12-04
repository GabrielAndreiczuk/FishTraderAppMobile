using Npgsql;

namespace FishTraderAppMobile;

public partial class Cadastro : ContentPage
{
    string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

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
        string usuario = txtUsuario.Text;
        string email = txtEmail.Text;
        string senha = txtSenha.Text;

        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                string insertQuery = "INSERT INTO public.\"Usuario\" (\"Nome\", \"Email\", \"Senha\") Values (@Nome,@Email,@Senha)";

                using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Nome",usuario);
                    command.Parameters.AddWithValue("@Email",email);
                    command.Parameters.AddWithValue("@Senha",senha);

                    command.ExecuteNonQuery();
                }

                SecureStorage.Default.SetAsync("Usuario_logado", usuario);

                App.Current.MainPage = new MainPage();
            }
            catch (Exception ex)
            {

                DisplayAlert("", ex.Message, "OK");
            }
        }        
    }
}