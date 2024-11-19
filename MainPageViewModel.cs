using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Maui;

namespace FishTraderAppMobile
{
    public partial class MainPageViewModel : INotifyPropertyChanged
    {
        //EVENTO PARA ATUALIZAÇÃO DINÂMICA DOS VALORES
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        //LISTA DE VALORES
        private static ObservableCollection<double> _biomassa;

        public ObservableCollection<double> biomassa
        {
            get { return _biomassa; }
            set {
                _biomassa = value;
                OnPropertyChanged(nameof(biomassa));
            }
        }
        
        //ANTIGO CASO O NOVO NÃO DE CERTO
        
        public ISeries[] Series { get; set; }

        // Definindo o eixo X do gráfico
        public ICartesianAxis[] XAxes { get; set; } = {
            new Axis
            {
                Labels = new string[] { "jan.", "fev.", "mar.", "abr.", "mai.", "jun.", "jul.", "ago.", "set.", "out.", "nov.", "dez." },
                LabelsRotation = 330,                
                TextSize = 8,
                Padding = new LiveChartsCore.Drawing.Padding(-5, 15),
                LabelsAlignment = LiveChartsCore.Drawing.Align.Middle,
                ForceStepToMin = true,
                MinStep = 1
            },
        };

        public ICartesianAxis[] YAxes { get; set; } = {
            new Axis
            {
                //Name = "Vendas projetadas (R$)",
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                TextSize = 7,
                LabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.Black,
                    //FontFamily = "Times New Roman",
                    SKFontStyle = new SKFontStyle(
                        SKFontStyleWeight.Bold,
                        SKFontStyleWidth.Normal,
                        SKFontStyleSlant.Italic)
                },
                //SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#147287")),
                Labeler = value => FormatCurrency(value)
            }
        };

        public ISeries[] Series2 { get; set; }
        public ICartesianAxis[] XAxes2 { get; set; } =
        {
            new Axis
            {
                // Inicialmente, as labels são definidas aqui como placeholders
                Labels = new string[] { "jan.", "fev.", "mar.", "abr.", "mai.", "jun.", "jul.", "ago.", "set.", "out.", "nov.", "dez." },
                LabelsRotation = 90,
                TextSize = 7,
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 0)
            }
        };

        public ICartesianAxis[] YAxes2 { get; set; } = {
            new Axis
            {
                //Name = "Vendas projetadas (R$)",
                NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                TextSize = 7,
                LabelsPaint = new SolidColorPaint
                {
                    Color = SKColors.Black,
                    //FontFamily = "Times New Roman",
                    SKFontStyle = new SKFontStyle(
                        SKFontStyleWeight.Bold,
                        SKFontStyleWidth.Normal,
                        SKFontStyleSlant.Italic)
                },
                SeparatorsPaint = new SolidColorPaint(SKColors.Red),
                Labeler = value => FormatCurrency(value)
            }
        };
        public static string FormatCurrency(double value)
        {
            if (value >= 1_000_000)
            {
                return $"{value / 1_000_000:0.#} mi";  // Exemplo: 1.5 mi
            }
            if (value >= 1_000)
            {
                return $"{value / 1_000:0.#} mil";  // Exemplo: 100 mil
            }
            return value.ToString("0");  // Exemplo: 500
        }

    }//FECHA CLASSE
}//FECHA NAMESPACE

