using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Threading;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.Net;
using hap = HtmlAgilityPack;

namespace WeatherChecker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] fmonths;
        int[] curmonths;
        Dictionary<int, string> months;
        int[] years;
        WebClient wc = new WebClient() { Encoding = Encoding.UTF8};
        string urltext = "https://www.gismeteo.ru/diary/";
        string uparams;
        string htmltext;
        Dictionary<string, int> Cityes;
        int tdate, ttemperature;


        private void DYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dmonth.Items.Clear();
            if(Convert.ToInt32(DYears.SelectedItem) != DateTime.Today.Year)
                foreach (string i in months.Values)
                {
                    Dmonth.Items.Add(i);
                }
            else
                foreach(int i in curmonths)
                {
                    Dmonth.Items.Add(months[i].ToString());
                }
            Dmonth.SelectedItem = "Январь";
        }

        public MainWindow()
        {
            InitializeComponent();
            months = new Dictionary<int, string>()
            {
                {1,"Январь" },
                {2,"Февраль" },
                {3,"Март" },
                {4,"Апрель" },
                {5,"Май" },
                {6,"Июнь" },
                {7,"Июль" },
                {8,"Август" },
                {9,"Сентябрь" },
                {10,"Октябрь" },
                {11,"Ноябрь" },
                {12,"Декабрь" },
            };
            fmonths = new string[12];
            curmonths = new int[(int)DateTime.Today.Month];
            years = new int[20];
            foreach (int i in months.Keys)
            {
                fmonths[i - 1] = months[i];
            }
            for(int i = 1; i <= Convert.ToInt32(DateTime.Today.Month); i++)
            {
                curmonths[i - 1] = i;
            }
            for(int i = Convert.ToInt32(DateTime.Today.Year), j = 0; i > Convert.ToInt32(DateTime.Today.Year) - 20; i--, j++)
            {
                years[j] = i;
            }
            foreach(int i in years)
            {
                DYears.Items.Add(i);
            }
            foreach(int i in curmonths)
            {
                Dmonth.Items.Add(months[i].ToString());
            }
            DYears.SelectedItem = DateTime.Today.Year;
            Dmonth.SelectedItem = Dmonth.Items[Convert.ToInt32(DateTime.Today.Month)-1];
            City.Items.Add("Москва");
            City.Items.Add("Краснодар");
            City.SelectedItem = "Краснодар";
            Cityes = new Dictionary<string, int>
            {
                { "Москва", 4368 },
                { "Краснодар", 5136 }
            };

        }
        // Построение графика температур
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GraphCanvas.Children.Clear();
                int j = 10;
                Dictionary<int, int> Temperature = GetTemperature(Cityes[City.SelectedItem.ToString()], DYears.SelectedItem.ToString(), months.Where(x => x.Value == Dmonth.SelectedItem.ToString()).FirstOrDefault().Key);
                // Ось абсцисс
                Line XAxe = new Line();
                Canvas.SetLeft(XAxe, 0);
                Canvas.SetTop(XAxe, GraphCanvas.Height / 2);
                XAxe.X1 = 0;
                XAxe.Y1 = GraphCanvas.ActualHeight / 2;
                XAxe.X2 = GraphCanvas.ActualWidth;
                XAxe.Y2 = GraphCanvas.ActualHeight / 2;
                XAxe.Stroke = Brushes.Red;
                GraphCanvas.Children.Add(XAxe);
                // построение графика
                foreach (int i in Temperature.Keys)
                {
                    Rectangle TempRec = new Rectangle();
                    TempRec.Width = 10;
                    Canvas.SetLeft(TempRec, 10 + j);
                    Canvas.SetBottom(TempRec, GraphCanvas.ActualHeight / 2);
                    if (Temperature[i] > 0)
                    {
                        TempRec.Fill = Brushes.Red;
                        TempRec.Height = Temperature[i] * 3;
                    }
                    else
                    {
                        TempRec.Fill = Brushes.Blue;
                        Canvas.SetBottom(TempRec, GraphCanvas.ActualHeight / 2 + Temperature[i] * 3);
                        TempRec.Height = -Temperature[i] * 3;
                    }
                    GraphCanvas.Children.Add(TempRec);
                    j += 15;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }
        private Dictionary<int, int> GetTemperature(int city, string dyear, int dmonth)
        {
            Dictionary<int, int> Temperature = new Dictionary<int, int>();
            uparams = $"{city}/{dyear}/{dmonth}";
            htmltext = wc.DownloadString(urltext + uparams);
            hap.HtmlDocument htmlsnippet = new hap.HtmlDocument();
            htmlsnippet.LoadHtml(htmltext);
            foreach (hap.HtmlNode block in htmlsnippet.DocumentNode.SelectNodes("//tbody"))
            {
                foreach (hap.HtmlNode tag in block.ChildNodes)
                {
                    foreach (hap.HtmlNode tag2 in tag.ChildNodes)
                    {
                        if (tag2.Name == "td")
                        {
                            if ((tag2.InnerText.Length >= 1 && tag2.InnerText.Length <= 2) && ((Convert.ToChar(tag2.InnerText[0]) != '+') && (Convert.ToChar(tag2.InnerText[0]) != '-')) && (Convert.ToChar(tag2.InnerText[0]) > '0') && (Convert.ToChar(tag2.InnerText[0]) < '9'))
                                tdate = int.Parse(tag2.InnerText);
                            else if ((tag2.InnerText.Contains('+')) || (tag2.InnerText.Contains('-')))
                            {
                                ttemperature = int.Parse(tag2.InnerText);
                                if (!Temperature.Keys.Contains(tdate))
                                    Temperature.Add(tdate, ttemperature);
                            }
                        }
                    }
                }
            }
            return Temperature;
        }
    }
}
