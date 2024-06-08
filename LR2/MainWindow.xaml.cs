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
using System.Windows.Shapes;

using System.Data;
using System.Data.Entity;

using LiveCharts;
using LiveCharts.Wpf;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LR2
{
    public class DBDataViewModel : INotifyPropertyChanged
    {
        private double _onPct;
        private double _maxPct;
        private double _loadPct;
        private SeriesCollection seriesCollection;

        public SeriesCollection SeriesCollection
        {
            get { return seriesCollection; }
            set
            {
                seriesCollection = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SeriesCollection"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double onPct
        {
            get { return _onPct; }
            set
            {
                _onPct = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("onPct"));
                }
            }
        }

        public double loadPct
        {
            get { return _loadPct; }
            set
            {
                _loadPct = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("loadPct"));
                }
            }
        }

        public double maxPct
        {
            get { return _maxPct; }
            set
            {
                _maxPct = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("maxPct"));
                }
            }
        }
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        private Database DB;
        private DataTable table;
        private DataTable table2;
        private MySqlDataAdapter adapter;

        class Database
        {
            MySqlConnection connection = new MySqlConnection("Server=localhost; Database=dashboard; User ID=root; Password=12345");

            public void openConnection()
            {
                if (connection.State == System.Data.ConnectionState.Closed) connection.Open();
            }

            public void closeConnection()
            {
                if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            }

            public MySqlConnection GetConnection()
            {
                return connection;
            }
        }

        

        public MainWindow()
        {
            InitializeComponent();
            DB = new Database();
            table = new DataTable();
            table2 = new DataTable();
            adapter = new MySqlDataAdapter();
        }

        private void FillHeader()
        {
            if ((type_mt != null) && (name_mt != null) && (baseLabel != null))
                baseLabel.Content = type_mt.Text + " " + name_mt.Text;
        }

        private void type_mtDDClosed(object sender, EventArgs e)
        {
            FillHeader();
        }

        private void name_mtDDClosed(Object sender, EventArgs e)
        {
            FillHeader();
        }

        private void baseLabel_loaded(object sender, RoutedEventArgs e)
        {
            FillHeader();
        }

        private void cncButton_Click(object sender, RoutedEventArgs e)
        {
            table.Rows.Clear();
            DB.openConnection();
            MySqlCommand command = new MySqlCommand("select * from dashboard.machine_tool_state", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(table);
            cncData.DataContext = table;
            DB.closeConnection();

        }

        private void load_button_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            DB.openConnection();
            MySqlCommand command = new MySqlCommand("Select * from dashboard.machine_tool_load", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(dt);
            DB.closeConnection();

            double onSum = 0;
            double loadSum = 0;
            double maxSum = 0;

            foreach(DataRow dr in dt.Rows)
            {
                if ((string)dr["status"] == "on")
                {
                    onSum += int.Parse((string)(dr["time_mtl"]));
                }
                if ((string)dr["status"] == "load")
                {
                    loadSum += int.Parse((string)(dr["time_mtl"]));
                }

                if ((string)dr["status"] == "max")
                {
                    maxSum += int.Parse((string)(dr["time_mtl"]));
                }
            }

            model.onPct = Math.Round((double)onSum / 60.0);
            model.loadPct = Math.Round((double)loadSum / 60.0);
            model.maxPct = Math.Round((double)maxSum / 60.0);

            double norm_on = (load_column_chart.Height/24) * model.onPct;
            double norm_load = (load_column_chart.Height / 24) * model.loadPct;
            double norm_off = (load_column_chart.Height / 24) * model.maxPct;

            onButton.Height = norm_on;
            loadButton.Height = norm_load;
            maxButton.Height = norm_off;

            percentage_mt_load.Height = load_column_chart.Height;
        }

        private void workingButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            DB.openConnection();
            MySqlCommand command = new MySqlCommand("Select * from dashboard.machine_tool_load", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(dt);
            DB.closeConnection();

            double v;

            SeriesCollection sc = new SeriesCollection();

            Brush onBrush = new SolidColorBrush(Color.FromRgb(0, 192, 0));
            Brush offBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            Brush loadBrush = new SolidColorBrush(Color.FromRgb(255, 128, 0));
            Brush maxBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            Brush currentBrush = onBrush;

            foreach(DataRow dr in dt.Rows)
            {
                v = (double.Parse((string)(dr["time_mtl"]))) / 60;

                if ((string)dr["status"] == "on")
                    currentBrush = onBrush;

                if ((string)dr["status"] == "off")
                    currentBrush = offBrush;

                if ((string)dr["status"] == "load")
                    currentBrush = loadBrush;

                if ((string)dr["status"] == "max")
                    currentBrush = maxBrush;

                sc.Add(new StackedRowSeries
                {
                    StackMode = StackMode.Values,
                    Values = new ChartValues<double> { v },
                    Fill = currentBrush
                });
            }
            model2.SeriesCollection = sc;
        }

        private void window_load(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            DB.openConnection();
            MySqlCommand command = new MySqlCommand("Select type from dashboard.machine_tool_type", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(dt);
            foreach (DataRow dr in dt.Rows)
            {
                type_mt.Items.Add((string)dr.ItemArray[0]);
            }

            DataTable dt2 = new DataTable();
            command = new MySqlCommand("Select machine_tool_name from dashboard.machine_tool_name", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(dt2);
            foreach(DataRow dr in dt2.Rows)
            {
                name_mt.Items.Add((string)dr.ItemArray[0]);
            }

            table2.Rows.Clear();
            command = new MySqlCommand("select * from dashboard.machine_tool_logData", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(table2);
            cncData2.DataContext = table2;

            DataTable dt4 = new DataTable();
            command = new MySqlCommand("SELECT * FROM dashboard.machine_tool_privod", DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(dt4);

            foreach (DataRow dr4 in dt4.Rows)
            {
                if ((int)dr4[0] == 1)
                {
                    if ((string)dr4["mode"] == "off")
                        privod1.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    if ((string)dr4["mode"] == "warming")
                        privod1.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0));

                    if ((string)dr4["mode"] == "ready")
                        privod1.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }

                if ((int)dr4[0] == 2)
                {
                    if ((string)dr4["mode"] == "off")
                        privod2.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    if ((string)dr4["mode"] == "warming")
                        privod2.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0));

                    if ((string)dr4["mode"] == "ready")
                        privod2.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }

                if ((int)dr4[0] == 3)
                {
                    if ((string)dr4["mode"] == "off")
                        privod3.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    if ((string)dr4["mode"] == "warming")
                        privod3.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0));

                    if ((string)dr4["mode"] == "ready")
                        privod3.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }

                if ((int)dr4[0] == 4)
                {
                    if ((string)dr4["mode"] == "off")
                        privod4.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    if ((string)dr4["mode"] == "warming")
                        privod4.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0));

                    if ((string)dr4["mode"] == "ready")
                        privod4.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }

                if ((int)dr4[0] == 5)
                {
                    if ((string)dr4["mode"] == "off")
                        privod5.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    if ((string)dr4["mode"] == "warming")
                        privod5.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0));

                    if ((string)dr4["mode"] == "ready")
                        privod5.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }

                if ((int)dr4[0] == 6)
                {
                    if ((string)dr4["mode"] == "off")
                        privod6.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    if ((string)dr4["mode"] == "warming")
                        privod6.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0));

                    if ((string)dr4["mode"] == "ready")
                        privod6.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
            }

            DB.closeConnection();
            type_mt.SelectedIndex = 0;
            name_mt.SelectedIndex = 0;
        }
    }
}
