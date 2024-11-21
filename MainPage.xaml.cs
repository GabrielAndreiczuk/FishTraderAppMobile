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

namespace FishTraderAppMobile
{
    public partial class MainPage : ContentPage
    {
        //STRING PARA CONEXÃO COM O BANCO DE DADOS
        string connectionString = "Host=localhost;Username=postgres;Password=root;Database=TesteFishTrader";

        //LISTAS DE ARMAZENAMENTO DE INFORMAÇÕES
        private ObservableCollection<double> idDados = new ObservableCollection<double>();
        private ObservableCollection<double> biomassa = new ObservableCollection<double>();
        private ObservableCollection<double> biomassaEsperada = new ObservableCollection<double>();
        private ObservableCollection<string> meses = new ObservableCollection<string>();

        //VARIÁVEIS PARA UTILIZAÇÃOD DE TIMER
        private Timer checkDataTimer;
        private int previousDataHash = 0;

        public MainPage()
        {
            InitializeComponent();

            //CHAMADA DE MÉTODO QUE PEDE PERMISSÃO DE NOTIFICAÇÃO AO USUÁRIO
            VerificarPerm();

            //CHAMADA MÉTODO RESPONSÁVEL POR CARREGAR INFORMAÇÕES DO BANCO DE DADOS
            CarregarDados();

            //CHAMADA MÉTODO RESPONSÁVEL POR GERAR MÉDIA DE INDICADORES
            GerarIndicadores();

            //CHAMADA MÉTODO RESPONSÁVEL POR APLICAR INFORMAÇÕES AO GRÁFICO LIVECHARTS
            BiomassaVsBiomassaEsp();

            checkDataTimer = new Timer(30000);
            checkDataTimer.Elapsed += (sender, e) =>
            {
                checkDataTimer.Stop();
                CheckForDataChanges();
                checkDataTimer.Start();
            };
            checkDataTimer.Start();
        }

        //MÉTODO QUE VERIFICA PERMISSÃO DE NOTIFICAÇÃO AO USUÁRIO
        private async void VerificarPerm()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }

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
                                biomassaAtual = Convert.ToDouble(reader["Biomassa"]);
                                idAtual = (int) (reader["ID_Biomassa"]);
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
                                    msg = $"O valor de biomassa mês {meses[idAtual]} ultrapassou {valorControle}";
                                    Notificar(msg);
                                }
                                if (biomassaAtual >= biomassaEsperada[idAtual])
                                {
                                    msg = $"O valor de biomassa mês {meses[idAtual]} ultrapassou o valor de biomassa esperado!";
                                    Notificar(msg);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }            
        }

        //MÉTODO RESPONSÁVEL POR CARREGAR INFORMAÇÕES DO BANCO DE DADOS
        private void CarregarDados()
        {
            biomassa.Clear();
            biomassaEsperada.Clear();

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
                            while (reader.Read())
                            {
                                idDados.Add(Convert.ToDouble(reader["ID_Biomassa"]));
                                biomassa.Add(Convert.ToDouble(reader["Biomassa"]));
                                biomassaEsperada.Add(Convert.ToDouble(reader["Biomassa_Esperada"]));
                                meses.Add(reader["ID_Mes"].ToString());
                            }
                        }
                    }
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

        //MÉTODO RESPONSÁVEL POR GERAR MÉDIA DE INDICADORES
        private void GerarIndicadores()
        {
            double mediaBiomassa = biomassa.Average();
            string txtMedia = MainPageViewModel.FormatCurrency(mediaBiomassa);
            lblBiomassa.Text = txtMedia.ToString();
        }

        //MÉTODO RESPONSÁVEL POR APLICAR INFORMAÇÕES AO GRÁFICO LIVECHARTS
        private void BiomassaVsBiomassaEsp()
        {            
            var viewModel = new MainPageViewModel();

            viewModel.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = biomassa,
                    Fill = new SolidColorPaint(new SKColor(93, 206, 190), 4),
                },
                new LineSeries<double>()
                {
                    Values = biomassaEsperada,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Black),
                    GeometryStroke = new SolidColorPaint(SKColors.Black),
                    GeometryFill = new SolidColorPaint(SKColors.Black),
                    GeometrySize = 5,
                    LineSmoothness = 0
                },
            };
            BindingContext = viewModel;                          
        }

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

        private void btnZoo_Clicked(object sender, EventArgs e)
        {
            //UTILZADO ATUALMENTE COMO UM REFRESH DA PÁGINA
            CarregarDados();
            GerarIndicadores();
        }   

        private void btnFin_Clicked(object sender, EventArgs e)
        {
            
        }

        

    }
}
