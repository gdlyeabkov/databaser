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
using System.Collections;

namespace DataBaser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public SQLiteConnection connection;
        public SpeechSynthesizer debugger;
        public string currentTable = "";
        public string currentTableMode = "";

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

        private void OpenCreateTableDialogHandler(object sender, RoutedEventArgs e)
        {
            OpenCreateTableDialog();
        }
        public void OpenCreateTableDialog()
        {
            Dialogs.CreateTableDialog dialog = new Dialogs.CreateTableDialog();
            dialog.Closed += CreateTableHandler;
            dialog.Show();
        }

        public void CreateTableHandler(object sender, EventArgs e)
        {
            Dialogs.CreateTableDialog dialog = ((Dialogs.CreateTableDialog)(sender));
            object dialogData = dialog.DataContext;
            bool isDataExists = dialogData != null;
            if (isDataExists)
            {
                string tableName = ((string)(dialogData));
                CreateTable(tableName);
            }
        }

        public void CreateTable(string tableName)
        {
            int tableNameLength = tableName.Length;
            bool isTableNameExists = tableNameLength >= 1;
            if (isTableNameExists)
            {
                string sql = "CREATE TABLE IF NOT EXISTS " + tableName + " (_id INTEGER PRIMARY KEY AUTOINCREMENT)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                GetTables();
            }
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
            tables.Visibility = Visibility.Visible;
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
                ContextMenu tableContextMenu = new ContextMenu();
                tableContextMenu.DataContext = ((string)(name));
                MenuItem tableContextMenuItem = new MenuItem();
                tableContextMenuItem.Header = "Режим конструктора";
                tableContextMenuItem.DataContext = "constructor";
                tableContextMenuItem.Click += SelectTableInConstructorModeHandler;
                tableContextMenu.Items.Add(tableContextMenuItem);
                tableContextMenuItem = new MenuItem();
                tableContextMenuItem.Header = "Режим редактирования";
                tableContextMenuItem.DataContext = "edit";
                tableContextMenuItem.Click += SelectTableInEditModeHandler;
                tableContextMenu.Items.Add(tableContextMenuItem);
                table.ContextMenu = tableContextMenu;                
            };
        }
        public void SelectTableInConstructorModeHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            ContextMenu menu = ((ContextMenu)(menuItem.Parent));
            object tableData = menu.DataContext;
            string tableName = ((string)(tableData));
            object tableModeData = menuItem.DataContext;
            string tableMode = ((string)(tableModeData));
            SelectTable(tableName, tableMode);
            SelectTableInConstructorMode(tableName);
        }

        public void SelectTableInConstructorMode(string tableName)
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
            tableRow = new RowDefinition();
            tableRow.Height = new GridLength(35);
            tableRecords.RowDefinitions.Add(tableRow);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnType = reader.GetDataTypeName(i).ToString();
                ColumnDefinition tableColumn = new ColumnDefinition();
                tableColumn.Width = new GridLength(275);
                tableRecords.ColumnDefinitions.Add(tableColumn);
                ComboBox tableColumnTypeBox = new ComboBox();
                tableColumnTypeBox.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnTypeBox.VerticalAlignment = VerticalAlignment.Center;
                ComboBoxItem tableColumnTypeBoxItem = new ComboBoxItem();
                tableColumnTypeBoxItem.Content = columnType;
                tableColumnTypeBox.Items.Add(   tableColumnTypeBoxItem);
                tableRecords.Children.Add(tableColumnTypeBox);
                tableColumnTypeBox.SelectedIndex = 0;
                Grid.SetRow(tableColumnTypeBox, 0);
                Grid.SetColumn(tableColumnTypeBox, i);

                string columnName = reader.GetName(i);
                TextBlock tableColumnLabel = new TextBlock();
                tableColumnLabel.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnLabel.VerticalAlignment = VerticalAlignment.Center;
                tableColumnLabel.Text = columnName;
                tableRecords.Children.Add(tableColumnLabel);
                Grid.SetRow(tableColumnLabel, 1);
                Grid.SetColumn(tableColumnLabel, i);
                ContextMenu tableColumnLabelContextMenu = new ContextMenu();
                MenuItem tableColumnLabelContextMenuItem = new MenuItem();
                tableColumnLabelContextMenuItem.Header = "Удалить столбец";
                tableColumnLabelContextMenuItem.DataContext = ((int)(i));
                tableColumnLabelContextMenuItem.Click += RemoveColumnHandler;
                tableColumnLabelContextMenu.Items.Add(tableColumnLabelContextMenuItem);
                tableColumnLabel.ContextMenu = tableColumnLabelContextMenu;
            }

            ColumnDefinition newTableColumn = new ColumnDefinition();
            newTableColumn.Width = new GridLength(275);
            tableRecords.ColumnDefinitions.Add(newTableColumn);
            ComboBox tableColumnType = new ComboBox();
            tableColumnType.HorizontalAlignment = HorizontalAlignment.Center;
            tableColumnType.VerticalAlignment = VerticalAlignment.Center;
            ComboBoxItem tableColumnTypeItem = new ComboBoxItem();
            tableColumnTypeItem.Content = "TEXT";
            tableColumnType.Items.Add(tableColumnTypeItem);
            tableColumnTypeItem = new ComboBoxItem();
            tableColumnTypeItem.Content = "INTEGER";
            tableColumnType.Items.Add(tableColumnTypeItem);
            tableColumnType.SelectedIndex = 0;
            tableRecords.Children.Add(tableColumnType);
            Grid.SetRow(tableColumnType, 0);
            Grid.SetColumn(tableColumnType, tableRecords.ColumnDefinitions.IndexOf(newTableColumn));

            TextBox tableColumnBox = new TextBox();
            tableRecords.Children.Add(tableColumnBox);
            Grid.SetRow(tableColumnBox, 1);
            Grid.SetColumn(tableColumnBox, tableRecords.ColumnDefinitions.IndexOf(newTableColumn));
            tableColumnBox.KeyUp += AddColumnHandler;

        }

        public void SelectTableInEditModeHandler (object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            ContextMenu menu = ((ContextMenu)(menuItem.Parent));
            object tableData = menu.DataContext;
            string tableName = ((string)(tableData));
            object tableModeData = menuItem.DataContext;
            string tableMode = ((string)(tableModeData));
            SelectTable(tableName, tableMode);
            SelectTableInEditMode(tableName);
        }

        public void SelectTableInEditMode(string tableName)
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
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                ColumnDefinition tableColumn = new ColumnDefinition();
                tableColumn.Width = new GridLength(275);
                tableRecords.ColumnDefinitions.Add(tableColumn);
                TextBlock tableColumnLabel = new TextBlock();
                tableColumnLabel.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnLabel.VerticalAlignment = VerticalAlignment.Center;
                tableColumnLabel.Text = columnName;
                tableRecords.Children.Add(tableColumnLabel);
                Grid.SetRow(tableColumnLabel, 0);
                Grid.SetColumn(tableColumnLabel, i);
            }

            // command = new SQLiteCommand(sql, connection);
            // reader = command.ExecuteReader();
            while(reader.Read())
            // for (int i = 0; i < reader.StepCount; i++)
            {
                // reader.Read();
                tableRow = new RowDefinition();
                tableRow.Height = new GridLength(35);
                tableRecords.RowDefinitions.Add(tableRow);
                int dataItemCursor = -1;
                for (int j = 0; j < reader.FieldCount; j++)
                {
                    dataItemCursor++;
                    string dataItemContent = "";
                    if (reader.GetDataTypeName(dataItemCursor) == "TEXT")
                    {
                        dataItemContent = reader.GetString(dataItemCursor);
                    }
                    else if (reader.GetDataTypeName(dataItemCursor) == "INTEGER")
                    {
                        // debugger.Speak("вижу число");
                        Int64 intData = reader.GetInt64(dataItemCursor);
                        dataItemContent = intData.ToString();
                    }
                    TextBlock dataItem = new TextBlock();
                    dataItem.HorizontalAlignment = HorizontalAlignment.Center;
                    dataItem.VerticalAlignment = VerticalAlignment.Center;
                    dataItem.Text = dataItemContent;
                    tableRecords.Children.Add(dataItem);
                    Grid.SetRow(dataItem, tableRecords.RowDefinitions.Count - 1);
                    Grid.SetColumn(dataItem, dataItemCursor);
                }
            };

            tableRow = new RowDefinition();
            tableRow.Height = new GridLength(35);
            tableRecords.RowDefinitions.Add(tableRow);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                TextBox tableColumnBox = new TextBox();
                tableRecords.Children.Add(tableColumnBox);
                Grid.SetRow(tableColumnBox, tableRecords.RowDefinitions.Count - 1);
                Grid.SetColumn(tableColumnBox, i); 
                bool isId = i < 1;
                if (isId)
                {
                    tableColumnBox.IsEnabled = false;
                }
            }

        }

        public void SelectTableHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            ContextMenu menu = ((ContextMenu)(menuItem.Parent));
            object tableData = menu.DataContext;
            string tableName = ((string)(tableData));
            SelectTable(tableName, "");
        }

        public void SelectTable(string tableName, string tableMode)
        {
            currentTable = tableName;
            currentTableMode = tableMode;
        }

        private void SaveDBHandler(object sender, RoutedEventArgs e)
        {
            SaveDB();
        }

        public void SaveDB ()
        {
            if (currentTableMode == "constructor")
            {
                string sql = "DROP TABLE " + currentTable;
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                string columns = "";
                foreach (UIElement tableRecord in tableRecords.Children)
                {
                    bool isColumns = Grid.GetRow(tableRecord) == 1;
                    bool isAdded = tableRecord is TextBlock;
                    bool isAddedColumns = isColumns && isAdded;
                    if (isAddedColumns)
                    {

                        string selectedType = "";
                        int columnTypeIndex = Grid.GetColumn(tableRecord);
                        foreach (UIElement someTableRecord in tableRecords.Children)
                        {
                            if (someTableRecord is ComboBox && Grid.GetColumn(someTableRecord) == columnTypeIndex && Grid.GetRow(someTableRecord) == 0)
                            {
                                ComboBox someTableRecordBox = ((ComboBox)(someTableRecord));
                                ComboBoxItem selectedsomeTableRecordBoxItem = ((ComboBoxItem)(someTableRecordBox.Items[someTableRecordBox.SelectedIndex]));
                                string selectedsomeTableRecordBoxItemContent = ((string)(selectedsomeTableRecordBoxItem.Content));
                                selectedType = " " + selectedsomeTableRecordBoxItemContent;
                                break;
                            }
                        }

                        TextBlock tableRecordColumn = tableRecord as TextBlock;
                        string tableRecordColumnContent = tableRecordColumn.Text;
                        bool isNotId = tableRecordColumnContent != "_id";
                        int tableRecordColumnContentLength = tableRecordColumnContent.Length;
                        bool isColumnNameExists = tableRecordColumnContentLength >= 1;
                        bool isTableRecordColumnSetted = isColumnNameExists && isNotId;
                        if (isTableRecordColumnSetted)
                        {
                            columns += tableRecordColumn.Text + selectedType + ", ";

                        }
                    }
                }
                columns = columns.Substring(0, columns.Length - 2);
                sql = "CREATE TABLE IF NOT EXISTS " + currentTable + " (_id INTEGER PRIMARY KEY AUTOINCREMENT, " + columns + ")";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            else if (currentTableMode == "edit")
            {
                string fields = "";
                string sql = "SELECT * FROM " + currentTable;
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                int fieldCursor = -1;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    fieldCursor++;
                    string field = reader.GetName(fieldCursor);
                    bool isNotId = field != "_id";
                    if (isNotId)
                    {
                        fields += field;
                        fields += ", ";
                    }
                }
                fields = fields.Substring(0, fields.Length - 2);
                List<int> detectedRecords = new List<int>();
                int recordsCursor = 1;
                foreach (UIElement tableRecord in tableRecords.Children)
                {
                    bool isRows = Grid.GetRow(tableRecord) == tableRecords.RowDefinitions.Count - 1;
                    bool isRecord = !(detectedRecords.Contains(Grid.GetRow(tableRecord)));
                    bool isAddedRows = isRows && isRecord;
                    if (isAddedRows)
                    {
                        detectedRecords.Add(Grid.GetRow(tableRecord));
                        recordsCursor++;
                        int row = Grid.GetRow(tableRecord);
                        string values = "";
                        int valuesCursor = -1;
                        foreach (UIElement record in tableRecords.Children)
                        {
                            if (row == Grid.GetRow(record) && record is TextBox)
                            {
                                valuesCursor++;
                                bool isNotId = valuesCursor >= 1;
                                if (isNotId)
                                {
                                    string separator = "";
                                    string rawType = reader.GetDataTypeName(valuesCursor);
                                    bool isCircumQuotes = rawType == "TEXT";
                                    if (isCircumQuotes)
                                    {
                                        separator = "\"";
                                    }
                                    TextBox recordBox = record as TextBox;
                                    values += separator + recordBox.Text + separator + ", ";
                                }
                            }
                        }
                        values = values.Substring(0, values.Length - 2);
                        sql = "INSERT INTO " + currentTable + " (" + fields + ") VALUES (" + values + ")";
                        command = new SQLiteCommand(sql, connection);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void OpenDBHandler(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите базу данных";
            ofd.FileName = "db.sqlite";
            ofd.Filter = "Sqlite data bases (.sqlite)|*.sqlite";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                string fullPath = ofd.FileName;
                ConnectToDB(fullPath);
                GetTables();
            }
        }

        public void AddColumnHandler (object sender, KeyEventArgs e)
        {
            Key currentKey = e.Key;
            Key enterKey = Key.Enter;
            bool isEnterKey = currentKey == enterKey;
            if (isEnterKey)
            {
                TextBox column = ((TextBox)(sender));

                ComboBox tableColumnType = new ComboBox();
                tableColumnType.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnType.VerticalAlignment = VerticalAlignment.Center;
                ComboBoxItem tableColumnTypeItem = new ComboBoxItem();
                tableColumnTypeItem.Content = "TEXT";
                tableColumnType.Items.Add(tableColumnTypeItem);
                tableColumnTypeItem = new ComboBoxItem();
                tableColumnTypeItem.Content = "INTEGER";
                tableColumnType.Items.Add(tableColumnTypeItem);
                tableColumnType.SelectedIndex = 0;
                tableRecords.Children.Add(tableColumnType);
                Grid.SetRow(tableColumnType, 0);
                Grid.SetColumn(tableColumnType, Grid.GetColumn(column) + 1);

                TextBlock savedColumn = new TextBlock();
                Grid.SetRow(savedColumn, 1);
                Grid.SetColumn(savedColumn, Grid.GetColumn(column));
                savedColumn.HorizontalAlignment = HorizontalAlignment.Center;
                savedColumn.VerticalAlignment = VerticalAlignment.Center;
                savedColumn.Text = column.Text;
                tableRecords.Children.Add(savedColumn);
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.Width = new GridLength(275);
                tableRecords.ColumnDefinitions.Add(columnDefinition);
                TextBox newTableColumn = new TextBox();
                Grid.SetRow(newTableColumn, 1);
                Grid.SetColumn(newTableColumn, Grid.GetColumn(column) + 1);
                tableRecords.Children.Add(newTableColumn);
                newTableColumn.KeyUp += AddColumnHandler;
                tableRecords.Children.Remove(column);
            }
        }

        private void CloseTableHandler(object sender, RoutedEventArgs e)
        {
            bool isTableOpened = currentTable.Length >= 1;
            if (isTableOpened)
            {
                currentTable = "";
                currentTableMode = "";
                tableRecords.Children.Clear();
                tableRecords.RowDefinitions.Clear();
                tableRecords.ColumnDefinitions.Clear();
            }
        }
        public void RemoveColumnHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            int column = ((int)(menuItem.DataContext));
            RemoveColumn(column);
        }

        public void RemoveColumn(int column)
        {
            for (int i = 0; i < tableRecords.Children.Count; i++)
            {
                if (Grid.GetColumn(tableRecords.Children[i]) == column)
                {
                    tableRecords.Children.RemoveAt(i);
                }
            }
            tableRecords.ColumnDefinitions.RemoveAt(column);
        }

    }
}
