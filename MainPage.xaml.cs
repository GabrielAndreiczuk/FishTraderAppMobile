using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Maui.Controls.Compatibility.Platform;
using Npgsql;
using SkiaSharp;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.LocalNotification;
using System.Timers;
using Timer = System.Timers.Timer;
using Plugin.LocalNotification.WindowsOption;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using static System.Net.Mime.MediaTypeNames;
using LiveChartsCore.SkiaSharpView.Maui;
using System;

namespace FishTraderAppMobile
{
    /// <summary>
    /// Página pricipal do aplicativo.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        //String para conexão com o banco de dados
        string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

        //Listas para armazenamento local de dados       
        public List<string> mesesLista = ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];
        public List<string> mesesListaAbv = ["jan.", "fev.", "mar.", "abr.", "mai.", "jun.", "jul.", "ago.", "set.", "out.", "nov.", "dez."];
        public List<string> semanasLista = new List<string>();

        private ObservableCollection<double> biomassa = new ObservableCollection<double>();
        private ObservableCollection<double> biomassaEsperada = new ObservableCollection<double>();
        private ObservableCollection<string> biomassaMeses = new ObservableCollection<string>();

        private ObservableCollection<double> sobrevivencia = new ObservableCollection<double>();
        private ObservableCollection<double> dias = new ObservableCollection<double>();
        private ObservableCollection<string> sobrevivenciaMeses = new ObservableCollection<string>();
        
        private readonly MainPageViewModel viewModel;        

        //VARIÁVEIS PARA UTILIZAÇÃOD DE TIMER
        private Timer checkDataTimer;
        private int previousDataHash = 0;

        public MainPage()
        {
            InitializeComponent();

            //Chamada do método que verifica a permissão de recebimento de notificações
            VerificarPerm();

            //Chamada do método que carrega informações do banco de dados
            CarregarDados(string.Empty);

            //Chamada do método que gere média de indicadores
            GerarIndicadores();

            for (int i = 1; i <= 52; i++)
            {
                semanasLista.Add($"Semana {i.ToString()}");
            }
            pickerSemana.ItemsSource = semanasLista;
            pickerMes.ItemsSource = mesesLista;                      
            
            viewModel = new MainPageViewModel();

            //Chamada de método que configura gráfico BiomassaVsBiomassaEsperada
            BiomassaVsBiomassaEsp();
            //Chamada de método que configura gráfico SobrevivenciaVsDias
            SobrevivenciaVsDias();    
            
            //Constrói um contexto para a viewModel
            BindingContext = viewModel;

            /*
            checkDataTimer = new Timer(30000);
            checkDataTimer.Elapsed += (sender, e) =>
            {
                checkDataTimer.Stop();
                CheckForDataChanges();
                checkDataTimer.Start();
            };
            checkDataTimer.Start();*/
        }

        //EVENTOS BOTÕES E ITENS DA TELA
        private void btnZoo_Clicked(object sender, EventArgs e)
        {
            //UTILZADO ATUALMENTE COMO UM REFRESH DA PÁGINA
            CarregarDados(string.Empty);
            GerarIndicadores();
        }
        private void btnFin_Clicked(object sender, EventArgs e)
        {

        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            CarregarDados(string.Empty);
        }
        /// <summary>
        /// Método acionado quando a seleção de 'pickerMes' é alterada.
        /// </summary>
        /// <param name="sender">Objeto que disparou o evento</param>
        /// <param name="e">Informações adicionais</param>
        /// <remarks>
        /// Gera um filtro conforme seleção de 'pickerMes'.
        /// Chama o método 'CarregarDados' passando o filtro como parâmetro.
        /// </remarks>
        private void pickerMes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //DisplayAlert("", mesesListaAbv[pickerMes.SelectedIndex], "OK");
            string filtro = $" WHERE \"mes_nome\" = '{mesesListaAbv[pickerMes.SelectedIndex]}'";
            CarregarDados(filtro);
        }
        private void pickerSemana_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Carrega dados do banco de dados para as listas.
        /// </summary>
        /// <param name="filter">Filtro adicional para consulta SQL.</param>
        /// <remarks>
        /// Limpa os dados das listas que guardam dados para os gráficos.
        /// Utiliza Npgql para conexão com banco de dados PostgreSQL.
        /// Na primeira consulta carrega os dados de BiomassaBioEsperada para as listas 'biomassa','biomassaEsperada' e 'biomassaMeses'.
        /// Na segunda consulta carrega os dados de SobrevivenciaDiaMes para as listas 'sobrevivencia','dias' e 'sobrevivenciaMeses'.
        /// Após as consultas o método chama 'GerarIndicadores' para calcular as novas médias. 
        /// </remarks>
        /// <exception cref="Exception">
        /// Lança uma exceção se houver falhas na conexão com o banco ou na execução da consulta.
        /// </exception>
        private void CarregarDados(string filter)
        {
            biomassa.Clear();
            biomassaEsperada.Clear();
            biomassaMeses.Clear();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string selectQuery = "SELECT * FROM BiomassaBioEsperada" + filter;

                    using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //idDados.Add(Convert.ToDouble(reader["ID_Biomassa"]));
                                biomassa.Add(Convert.ToDouble(reader["Biomassa_Valor"]));
                                biomassaEsperada.Add(Convert.ToDouble(reader["Biomassa_Esperada"]));
                                biomassaMeses.Add(reader["mes_nome"].ToString());
                            }
                        }
                    }
                    sobrevivencia.Clear();
                    dias.Clear();
                    sobrevivenciaMeses.Clear();
                    selectQuery = "SELECT * FROM SobrevivenciaDiaMes" + filter;
                    using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sobrevivencia.Add(double.Parse(reader["Sobrevivencia_Valor"].ToString()));
                                dias.Add(int.Parse(reader["Dias"].ToString()));
                                sobrevivenciaMeses.Add(reader["mes_nome"].ToString());
                            }
                        }
                    }

                    GerarIndicadores();
                }
                catch (Exception ex)
                {
                    //APAGAR PÓS TESTE MOBILE
                    biomassa = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
                    biomassaEsperada = [ 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13];
                    DisplayAlert("", $"{ex.Message}", "OK");
                }
            }
        }

        /// <summary>
        /// Método que calcula a média de indicadores.
        /// </summary>
        /// <remarks>
        /// Utiliza o método Average para obter a média dos valores das listas 'biomassa' e 'sobrevivencia'.
        /// Formata os valores e exibe nas Labels correspondentes
        /// </remarks>
        private void GerarIndicadores()
        {
            string txtMedia = "";
            double mediaBiomassa = biomassa.Average();
            txtMedia = MainPageViewModel.FormatCurrency(mediaBiomassa);
            lblBiomassa.Text = txtMedia.ToString();

            double mediaSobrevivencia = sobrevivencia.Average();
            //txtMedia = MainPageViewModel.FormatCurrency(mediaBiomassa);
            lblSobrevivencia.Text = $"{mediaSobrevivencia:F1}";
        }

        /// <summary>
        /// Configura o gráfico de Biomassa versus Biomassa Esperada, com uma série de colunas e linhas.
        /// </summary>
        ///<remarks>
        /// Utiliza a biblioteca LiveCharts para a geração dos gráficos.
        /// Recebe como valores da série de colunas os dados da lista 'biomassa' representando Biomassa.
        /// Recebe como valores da série de linhas os dados da lista 'biomassaEsperada' representando BiomassaEsperada.
        /// A configuração do eixo X é feita com os dados da lista 'biomassaMeses'.
        /// É registrado um evento de clique nas colunas do gráfico, que acionará a funcão 'BioVsBioEsp_Clicked'.
        /// </remarks>
        private void BiomassaVsBiomassaEsp()
        {          
            var colunas = new ColumnSeries<double>
            {
                Values = biomassa,
                Fill = new SolidColorPaint(new SKColor(93, 206, 190), 4),
            };

            var linhas = new LineSeries<double>()
            {
                Values = biomassaEsperada,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Black),
                GeometryStroke = new SolidColorPaint(SKColors.Black),
                GeometryFill = new SolidColorPaint(SKColors.Black),
                GeometrySize = 5,
                LineSmoothness = 0
            };         

            viewModel.XAxes[0] = new Axis()
            {
                Labels = biomassaMeses,
                LabelsRotation = 330,
                TextSize = 8,
                Padding = new LiveChartsCore.Drawing.Padding(-5, 15),
                LabelsAlignment = LiveChartsCore.Drawing.Align.Middle,
                ForceStepToMin = true,
                MinStep = 1
            };

            viewModel.Series = new ISeries[] { colunas, linhas };
            colunas.ChartPointPointerDown += BioVsBioEsp_Clicked;
        }

        /// <summary>
        /// Configura o gráfico de Sobrevivência versus Dias, com uma série de colunas e linhas.
        /// </summary>
        /// <remarks>
        /// Utiliza a biblioteca LiveCharts para a geração dos gráficos.
        /// Recebe como valores da série de colunas os dados da lista 'dias' representando Dias.
        /// Recebe como valores da série de linhas os dados da lista 'sobrevivencia' representando Sobrevivência.
        /// A configuração do eixo X é feita com os dados da lista 'sobrevivenciaMeses'.
        /// É registrado um evento de clique nas colunas do gráfico, que acionará a funcão 'SobrevivenciaDias_Clicked'.
        /// </remarks>
        private void SobrevivenciaVsDias()
        {
            var colunas = new ColumnSeries<double>
            {
                Values = dias,
                Fill = new SolidColorPaint(new SKColor(93, 206, 190), 4),
                Padding = 1
            };

            var linhas = new LineSeries<double>()
            {
                Values = sobrevivencia,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Black),
                GeometryStroke = new SolidColorPaint(SKColors.Black),
                GeometryFill = new SolidColorPaint(SKColors.Black),
                GeometrySize = 3,
                LineSmoothness = 0
            };

            colunas.ChartPointPointerDown += SobrevivenciaDias_Clicked;

            viewModel.Series2 = new ISeries[] { colunas, linhas };
            viewModel.XAxes2[0] = new Axis()
            {
                Labels = sobrevivenciaMeses,
                LabelsRotation = 330,
                TextSize = 8,
                Padding = new LiveChartsCore.Drawing.Padding(-5, 15),
                LabelsAlignment = LiveChartsCore.Drawing.Align.Middle,
            };
        }

        /// <summary>
        /// Filtra seleção conforme a coluna selecionada no gráfico no evento de clique.
        /// </summary>
        /// <param name="chartView">Gráfico selecionado</param>
        /// <param name="point">Ponto selecionado do gráfico</param>
        /// /// <remarks>
        /// Este método utiliza propriedade 'Coordinate' da biblioteca LiveCharts para obter da localização
        /// do ponto selecionado. A partir do índice obtido, busca o nome correspondente na lista 'biomassaMeses'.
        /// Cria um filtro com o mês selecionado.
        /// Chama o método 'CarregarDados' e passa o parâmetro filtro para atualizar as informações dos gráficos.
        /// </remarks>
        private void BioVsBioEsp_Clicked( IChartView chartView, ChartPoint<double, RoundedRectangleGeometry, LabelGeometry>? point)
        {
            if (point?.Visual is null) return;            

            string month = point.Coordinate.ToString();
            string[] coordenadaSplit = month.Split('(',',',')');
            int index = int.Parse(coordenadaSplit[1]);

            string filtro = $" WHERE \"mes_nome\" = '{biomassaMeses[index]}'";
            CarregarDados(filtro);       
        }

        /// <summary>
        /// Filtra seleção conforme a coluna selecionada no gráfico no evento de clique.
        /// </summary>
        /// <param name="chartView">Gráfico selecionado</param>
        /// <param name="point">Ponto selecionado do gráfico</param>
        /// <remarks>
        /// Este método utiliza propriedade 'Coordinate' da biblioteca LiveCharts para obter da localização
        /// do ponto selecionado. A partir do índice obtido, busca o nome correspondente na lista 'sobrevivenciaMes'.
        /// Cria um filtro com o mês selecionado.
        /// Chama o método 'CarregarDados' e passa o parâmetro filtro para atualizar as informações dos gráficos.
        /// </remarks>
        private void SobrevivenciaDias_Clicked(IChartView chartView, ChartPoint<double, RoundedRectangleGeometry, LabelGeometry>? point)
        {
            if (point?.Visual is null) return;

            string month = point.Coordinate.ToString();
            string[] coordenadaSplit = month.Split('(', ',', ')');
            int index = int.Parse(coordenadaSplit[1]);

            string filtro = $" WHERE \"mes_nome\" = '{sobrevivenciaMeses[index]}'";
            CarregarDados(filtro);          
        }
        
        /// <summary>
        /// Verifica permissão de recebimento de notificação.
        /// </summary>
        /// <remarks>
        /// Utiliza o sistema de permissões para verificar se o aplicativo possui
        /// permissão para exibir notificações.
        /// </remarks>
        private async void VerificarPerm()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }

        /// <summary>
        /// Compara dados atuais do banco de dados com dados obtidos anteriormente.
        /// Envia notificações caso valores ultrapassem limites específicos.
        /// </summary>
        /// <remarks>
        /// Utiliza Npgsql para conexão com banco de dados PostgreSQL.
        /// Faz uma consulta nas tabelas e compara com dados guardados nas listas de armazenamento.
        /// </remarks>
        /// /// <exception cref="Exception">
        /// Lança uma exceção se houver falhas na conexão com o banco ou na execução da consulta.
        /// </exception>
        private async void CheckForDataChanges()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string selectQuery = "SELECT * FROM public.\"Biomassa\" order by \"ID_Mes\";";

                    using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            double biomassaAtual = 0;
                            int idAtual = 0;
                            while (reader.Read())
                            {
                                biomassaAtual = Convert.ToDouble(reader["Biomassa_Valor"]);
                                idAtual = (int)(reader["ID_Biomassa"]);
                                idAtual -= 1;

                                if (biomassa[idAtual] == biomassaAtual)
                                {
                                    return;
                                }

                                biomassa[idAtual] = biomassaAtual;
                                double valorControle = 300000;
                                string msg = "";

                                if (biomassaAtual >= valorControle && !(biomassaAtual >= biomassaEsperada[idAtual]))
                                {
                                    msg = $"O valor de biomassa mês {biomassaMeses[idAtual]} ultrapassou {valorControle}";
                                    Notificar(msg);
                                }
                                if (biomassaAtual >= biomassaEsperada[idAtual])
                                {
                                    msg = $"O valor de biomassa mês {biomassaMeses[idAtual]} ultrapassou o valor de biomassa esperado!";
                                    Notificar(msg);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayAlert("Erro!", ex.Message, "OK");
                }
            }
        }

        /// <summary>
        /// Configura e exibe uma notificação local.
        /// </summary>
        /// <param name="msg">Mensagem a ser exibida na descrição da notificação</param>
        /// <remarks>
        /// Utiliza a biblioteca Plugin.LocalNotification;
        /// Exibe a notificação em 5 segundos 
        /// </remarks>
        private void Notificar(string msg)
        {
            //CONFIGURAÇÃO DE NOTIFICAÇÕES
            var notificacao = new NotificationRequest
            {
                NotificationId = 100,
                Title = "Dados desatualizados!",
                Description = msg,
                ReturningData = "Meus Dados",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(5)
                },
                Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions()
                {
                    AutoCancel = true,
                    IconSmallName = { ResourceName = "fishtrader" },
                    //IconSmallName = Plugin.LocalNotification.AndroidOption.AndroidIcon("appicon")
                }
            };
            LocalNotificationCenter.Current.Show(notificacao);
        }
    }
}
