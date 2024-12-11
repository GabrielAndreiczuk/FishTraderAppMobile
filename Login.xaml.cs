using Microsoft.Maui.ApplicationModel.Communication;
using Npgsql;

namespace FishTraderAppMobile;
/// <summary>
/// P�gina de login do aplicativo.
/// </summary>
public partial class Login : ContentPage
{
    string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

    public Login()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Verifica o login do usu�rio.
    /// </summary>
    /// <param name="sender">O objeto que disparou o evento</param>
    /// <param name="e">Informa��es adicionais</param>
    /// <remarks>
    /// O m�todo obtem os valores de 'txtEmail' e 'txtSenha' e realiza uma consulta na tabela 'Usuario' 
    /// atrav�s da biblioteca Npgsql em um banco de dados PostgreSQL.
    /// Verifica se existe registros no banco dados iguais aos valores inseridos.
    /// Caso as informa��es sejam verdadeiras altera a propriedade de <c>App.Current.MainPage</c>, que recebe uma
    /// nova inst�ncia de <c>MainPage</c>,substituindo a p�gina atual pela nova p�gina.
    /// </remarks>
    /// <exception cref="Exception">
    /// Dispara uma excess�o caso haja algum erro de conex�o com o banco de dados ou caso as 
    /// informa��es de login n�o estejam corretas.
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
                    throw new Exception("Usu�rio ou senha incorreta!");
                }
            }
            catch (Exception ex)
            {

                DisplayAlert("", ex.Message, "OK");
            }
        }
    }

    /// <summary>
    /// Redireciona o usu�rio para a tela de cadastro.
    /// </summary>
    /// <param name="sender">Objeto que disparou o evento</param>
    /// <param name="e">Informa��es adicionais</param>
    /// <remarks>
    /// O m�todo atualiza a p�gina atual do aplicativo para a tela 'Cadastro'.
    /// O m�todo altera a propriedade <c>App.Current.MainPage</c>, que recebe uma inst�ncia
    /// de Cadastro.
    /// Substitui a p�gina atual pela nova p�gina.
    /// </remarks>
    private void btnCadastro_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new Cadastro();
    }

    private async void OnConsultarApiClicked(object sender, EventArgs e)
    {
        try
        {
            //string url = "http://localhost:7114/api/v2/biomassa";
            string url = "http://192.168.100.9:7114/api/v2/biomassa";

            // Configura o HttpClient
            using HttpClient client = new HttpClient();

            // Faz a requisi��o GET
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // L� a resposta como string
                string result = await response.Content.ReadAsStringAsync();

                // Exibe o resultado (exemplo: em um DisplayAlert)
                await DisplayAlert("Sucesso", $"Resposta da API: {result}", "OK");
            }
            else
            {
                // Em caso de erro na resposta
                await DisplayAlert("Erro", $"Falha na requisi��o: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            // Trata poss�veis exce��es, como falha de conex�o
            await DisplayAlert("Erro", $"Erro ao consultar API: {ex.Message}", "OK");
        }
    }
}