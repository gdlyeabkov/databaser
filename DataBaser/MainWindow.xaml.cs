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

using System.Data.SQLite;
using System.Speech.Synthesis;
using MaterialDesignThemes.Wpf;

namespace DataBaser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public SQLiteConnection connection;
        public SpeechSynthesizer debugger;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        
        }

        public void Initialize()
        {
            debugger = new SpeechSynthesizer();
        }

        private void CreateDBHandler(object sender, RoutedEventArgs e)
        {
            CreateDB();
        }

        public void CreateDB()
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Title = "Выберите место для сохранения базы данных";
            sfd.FileName = "db.sqlite";
            sfd.Filter = "Sqlite data bases (.sqlite)|*.sqlite";
            bool? res = sfd.ShowDialog();
            bool isSaved = res != false;
            if (isSaved)
            {
                string fullPath = sfd.FileName;
                SQLiteConnection.CreateFile(fullPath);
                ConnectToDB(fullPath);
            }
        }

        private void CreateTableHandler(object sender, RoutedEventArgs e)
        {
            CreateTable();
        }
        public void CreateTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS users (_id INTEGER PRIMARY KEY AUTOINCREMENT, login varchar(20), password varchar(20))";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            GetTables();
        }

        private void InsertRecordHandler(object sender, RoutedEventArgs e)
        {
            InsertRecord();
        }
        public void InsertRecord()
        {
            string sql = "INSERT INTO users (login, password) VALUES (\"Rodion\", \"hash_string\")";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public void ConnectToDB (string path) {
            string rawConfig = "Data Source=" + path + ";Version=3;";
            connection = new SQLiteConnection(rawConfig);
            connection.Open();
        }

        private void GetRecordsHandler(object sender, RoutedEventArgs e)
        {
            GetRecords();
        }
        public void GetRecords()
        {
            string sql = "SELECT * FROM users";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                string login = reader.GetString(1);
                string password = reader.GetString(2);
                debugger.Speak("login: " + login + ", password: " + password);
            };
        }

        private void UpdateRecordHandler(object sender, RoutedEventArgs e)
        {
            UpdateRecord();
        }
        public void UpdateRecord()
        {
            string sql = "UPDATE users SET login=\"noidoR\", password=\"hex_code\"";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private void DeleteRecordHandler(object sender, RoutedEventArgs e)
        {
            DeleteRecord();
        }
        public void DeleteRecord()
        {
            string sql = "DELETE FROM users";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public void GetTables()
        {
            string sql = "SELECT name FROM sqlite_schema WHERE type = \'table\' AND name NOT LIKE \'sqlite_%\'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            tables.Children.Clear();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                StackPanel table = new StackPanel();
                table.Orientation = Orientation.Horizontal;
                PackIcon tableIcon = new PackIcon();
                tableIcon.Kind = PackIconKind.Database;
                tableIcon.Margin = new Thickness(15);
                table.Children.Add(tableIcon);
                TextBlock tableNameLabel = new TextBlock();
                tableNameLabel.Text = name;
                tableNameLabel.Margin = new Thickness(15);
                table.Children.Add(tableNameLabel);
                tables.Children.Add(table);
                table.DataContext = ((string)(name));
                table.MouseLeftButtonUp += SelectTableHandler;
            };
        }

        public void SelectTableHandler(object sender, MouseEventArgs e)
        {
            StackPanel table = ((StackPanel)(sender));
            object tableData = table.DataContext;
            string tableName = ((string)(tableData));
            SelectTable(tableName);
        }

        public void SelectTable(string tableName)
        {
            tableRecords.Children.Clear();
            tableRecords.RowDefinitions.Clear();
            tableRecords.ColumnDefinitions.Clear();
            string sql = "SELECT * FROM " + tableName;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            RowDefinition tableRow = new RowDefinition();
            tableRow.Height = new GridLength(35);
            tableRecords.RowDefinitions.Add(tableRow);
            for (int i = 0; i < reader.FieldCount - 1; i++)
            {
                string columnName = "колонка";
                ColumnDefinition tableColumn = new ColumnDefinition();
                tableColumn.Width = new GridLength(275);
                tableRecords.ColumnDefinitions.Add(tableColumn);
                TextBlock tableColumnLabel = new TextBlock();
                tableColumnLabel.Text = columnName;
                tableRecords.Children.Add(tableColumnLabel);
                Grid.SetRow(tableColumnLabel, 0);
                Grid.SetColumn(tableColumnLabel, i);
            }
            while (reader.Read())
            {
                string login = reader.GetString(1);
                string password = reader.GetString(2);
                // debugger.Speak("login: " + login + ", password: " + password);
                tableRow = new RowDefinition();
                tableRow.Height = new GridLength(35);
                tableRecords.RowDefinitions.Add(tableRow);

            };
        }

    }
}
