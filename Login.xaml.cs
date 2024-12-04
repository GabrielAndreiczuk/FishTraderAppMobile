using Microsoft.Maui.ApplicationModel.Communication;
using Npgsql;

namespace FishTraderAppMobile;

public partial class Login : ContentPage
{
    string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

    public Login()
	{
		InitializeComponent();
	}

    private void btnEntrar_Clicked(object sender, EventArgs e)
    {
        string email = txtEmail.Text;
        string senha = txtSenha.Text;

        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                string _email = "", _senha = "";
                bool login = false;

                string selectQuery = "SELECT * FROM public.\"Usuario\"";

                using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        
                        while (reader.Read())
                        {
                            _email = reader["Email"].ToString();
                            _senha = reader["Senha"].ToString();

                            if ((email == _email) && (senha == _senha))
                            {
                                login = true;
                                SecureStorage.Default.SetAsync("Usuario_logado", reader["Nome"].ToString());
                            }
                        }
                    }
                }
                if (login)
                {
                    App.Current.MainPage = new MainPage();
                }
                else
                {
                    throw new Exception("Usuário ou senha incorreta!");
                }
            }
            catch (Exception ex)
            {

                DisplayAlert("", ex.Message, "OK");
            }
        }
    }

    private void btnCadastro_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Cadastro();
    }    
}