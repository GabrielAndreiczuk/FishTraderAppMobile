using Microsoft.Maui.ApplicationModel.Communication;
using Npgsql;

namespace FishTraderAppMobile;
/// <summary>
/// Página de login do aplicativo.
/// </summary>
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
    /// O método obtem os valores de 'txtEmail' e 'txtSenha' e realiza uma consulta na tabela 'Usuario' 
    /// através da biblioteca Npgsql em um banco de dados PostgreSQL.
    /// Verifica se existe registros no banco dados iguais aos valores inseridos.
    /// Caso as informações sejam verdadeiras altera a propriedade de <c>App.Current.MainPage</c>, que recebe uma
    /// nova instância de <c>MainPage</c>,substituindo a página atual pela nova página.
    /// </remarks>
    /// <exception cref="Exception">
    /// Dispara uma excessão caso haja algum erro de conexão com o banco de dados ou caso as 
    /// informações de login não estejam corretas.
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

    private async void OnConsultarApiClicked(object sender, EventArgs e)
    {
        try
        {
            //string url = "http://localhost:7114/api/v2/biomassa";
            string url = "http://192.168.100.9:7114/api/v2/biomassa";

            // Configura o HttpClient
            using HttpClient client = new HttpClient();

            // Faz a requisição GET
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Lê a resposta como string
                string result = await response.Content.ReadAsStringAsync();

                // Exibe o resultado (exemplo: em um DisplayAlert)
                await DisplayAlert("Sucesso", $"Resposta da API: {result}", "OK");
            }
            else
            {
                // Em caso de erro na resposta
                await DisplayAlert("Erro", $"Falha na requisição: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            // Trata possíveis exceções, como falha de conexão
            await DisplayAlert("Erro", $"Erro ao consultar API: {ex.Message}", "OK");
        }
    }
}