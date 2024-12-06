using Npgsql;

namespace FishTraderAppMobile;

/// <summary>
/// Página de cadastro do aplicativo.
/// </summary>
public partial class Cadastro : ContentPage
{
    readonly string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

    public Cadastro()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Cadastra um novo usuário no aplicativo.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>
    /// Obtém os valores de 'txtUsuario', 'txtEmail' e 'txtSenha' e insere os dados na tabela 'Usuario' 
    /// de um banco de dados PostgreSQL, utilizando a biblioteca npgsql.
    /// Após o registro, armazena o nome do usuário na propriedade <c>SecureStorage</c>.
    /// Altera a propriedade de <c>App.Current.MainPage</c>, que recebe uma
    /// nova instância de <c>MainPage</c>,substituindo a página atual pela nova página.
    /// </remarks>
    /// <exception cref="Exception">
    /// Dispara uma excessão caso haja um erro de conexão com o banco de dados.
    /// </exception>
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

    /// <summary>
    /// Redireciona o usuário para a tela de Login.
    /// </summary>
    /// <param name="sender">Objeto que disparou o evento</param>
    /// <param name="e">Informações adicionais</param>
    /// <remarks>
    /// O método atualiza a página atual do aplicativo para a tela 'Login'.
    /// O método altera a propriedade <c>App.Current.MainPage</c>, que recebe uma instância
    /// de <c>Login</c>.
    /// Substitui a página atual pela nova página.
    /// </remarks>
    private void btnLogin_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }
}