using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FishTraderAppMobile
{
    internal class ApiService
    {
        public class LoginRequest
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }
            [JsonPropertyName("password")]
            public string Password { get; set; }
            [JsonPropertyName("twoFactorCode")]
            public string TwoFactorCode { get; set; }
            [JsonPropertyName("twoFactorRecoveryCode")]
            public string TwoFactorRecoveryCode { get; set; }
        }

        public class CadastroRequest
        {
            [JsonPropertyName("nome")]
            public string Nome { get; set; }
            [JsonPropertyName("email")]
            public string Email { get; set; }
            [JsonPropertyName("senha")]
            public string Senha { get; set; }

        }

        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:7114/api/v2")
            };
        }

        public async Task<bool> LoginAsync(string email, string senha)
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = senha,
                TwoFactorCode = string.Empty,
                TwoFactorRecoveryCode = string.Empty
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(loginRequest), // Serializa o objeto em JSON
                System.Text.Encoding.UTF8, // Define a codificação
                "application/json" // Define o tipo de mídia
            );

            //visualização do JSON
            var serializedContent = await content.ReadAsStringAsync(); // Lê o conteúdo como string
            Console.WriteLine($"Conteúdo enviado: {serializedContent}");

            var response = await _httpClient.PostAsync("http://localhost:7114/api/v2/Usuario/Login", content);

            if(response.IsSuccessStatusCode) 
                return true;

            return false;
        }

        public async Task<bool> CadastroAsync(string nome, string email, string senha)
        {
            var cadastroRequest = new CadastroRequest()
            {
                Nome = nome,
                Email = email,
                Senha = senha
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(cadastroRequest), // Serializa o objeto em JSON
                System.Text.Encoding.UTF8, // Define a codificação
                "application/json" // Define o tipo de mídia
            );

            //visualização do JSON
            var serializedContent = await content.ReadAsStringAsync(); // Lê o conteúdo como string
            Console.WriteLine($"Conteúdo enviado: {serializedContent}");

            var response = await _httpClient.PostAsync("http://localhost:7114/api/v2/Usuario/Cadastro", content);

            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }
    }
}
