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

    /// <summary>
    /// Verifica o login do usuário.
    /// </summary>
    /// <param name="sender">O objeto que disparou o evento</param>
    /// <param name="e">Informações adicionais</param>
    /// <remarks>
    /// O método recebe em variáveis 'email' e 'senha' os valores de 'txtEmail' e 'txtSenha' respectivamente.
    /// Realiza uma consulta na tabela 'Usuario' através da biblioteca Npgsql em um banco de dados PostgreSQL.
    /// Compara se existe cadastrado no banco dados iguais aos inseridos nas textbox.
    /// Caso as informações sejam verdadeiras altera a propriedade de <c>App.Current.MainPage</c>, que recebe uma
    /// nova instância de MainPage.
    /// Substitui a página atual pela nova página.
    /// </remarks>
    /// <exception cref="Exception">
    /// Dispara uma excessão caso haja algum erro de conexão com o banco de dados.
    /// Dispara uma excesão caso as informações de login não estejam corretas.
    /// </exception>
    private void btnEntrar_Clicked(object sender, EventArgs e)
    {
        string email = txtEmail.Text;
        string senha = txtSenha.Text;

        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                bool login = false;

                string selectQuery = "SELECT \"Nome\" FROM public.\"Usuario\" WHERE \"Email\" = @email AND \"Senha\" = @senha";

                using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@senha", senha);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            login = true;
                            string userName = reader["Nome"].ToString();
                            SecureStorage.Default.SetAsync("Usuario_logado", reader["Nome"].ToString());
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

    /// <summary>
    /// Redireciona o usuário para a tela de cadastro.
    /// </summary>
    /// <param name="sender">Objeto que disparou o evento</param>
    /// <param name="e">Informações adicionais</param>
    /// <remarks>
    /// O método atualiza a página atual do aplicativo para a tela 'Cadastro'.
    /// O método altera a propriedade <c>App.Current.MainPage</c>, que recebe uma instância
    /// de Cadastro.
    /// Substitui a página atual pela nova página.
    /// </remarks>
    private void btnCadastro_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Cadastro();
    }    
}