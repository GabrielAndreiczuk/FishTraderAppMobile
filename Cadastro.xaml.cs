using Npgsql;

namespace FishTraderAppMobile;

/// <summary>
/// P�gina de cadastro do aplicativo.
/// </summary>
public partial class Cadastro : ContentPage
{
    readonly string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

    public Cadastro()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Cadastra um novo usu�rio no aplicativo.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>
    /// Obt�m os valores de 'txtUsuario', 'txtEmail' e 'txtSenha' e insere os dados na tabela 'Usuario' 
    /// de um banco de dados PostgreSQL, utilizando a biblioteca npgsql.
    /// Ap�s o registro, armazena o nome do usu�rio na propriedade <c>SecureStorage</c>.
    /// Altera a propriedade de <c>App.Current.MainPage</c>, que recebe uma
    /// nova inst�ncia de <c>MainPage</c>,substituindo a p�gina atual pela nova p�gina.
    /// </remarks>
    /// <exception cref="Exception">
    /// Dispara uma excess�o caso haja um erro de conex�o com o banco de dados.
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
    /// Redireciona o usu�rio para a tela de Login.
    /// </summary>
    /// <param name="sender">Objeto que disparou o evento</param>
    /// <param name="e">Informa��es adicionais</param>
    /// <remarks>
    /// O m�todo atualiza a p�gina atual do aplicativo para a tela 'Login'.
    /// O m�todo altera a propriedade <c>App.Current.MainPage</c>, que recebe uma inst�ncia
    /// de <c>Login</c>.
    /// Substitui a p�gina atual pela nova p�gina.
    /// </remarks>
    private void btnLogin_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Login();
    }
}