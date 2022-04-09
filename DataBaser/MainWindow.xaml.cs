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
using System.Windows.Controls.Primitives;

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
        public bool isDetectChanges = false;
        public bool isRelationSet = false;
        public Line relation = null;
        public string filterColumn = "";

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

        public void ConnectToDB(string path) {
            string rawConfig = "Data Source=" + path + ";Version=3;";
            connection = new SQLiteConnection(rawConfig);
            connection.Open();
            aside.Visibility = Visibility.Visible;
            screenControl.SelectedIndex = 1;
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
                tableContextMenuItem = new MenuItem();
                tableContextMenuItem.Header = "Удалить таблицу";
                tableContextMenuItem.Click += DropTableHandler;
                tableContextMenu.Items.Add(tableContextMenuItem);
                table.ContextMenu = tableContextMenu;
            };
        }

        public void DropTableHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            object tableData = menuItem.DataContext;
            string tableName = ((string)(tableData));
            DropTable(tableName);
        }

        public void DropTable(string tableName)
        {
            string sql = "DROP TABLE " + tableName;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            GetTables();
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
            ClearArticleContent();
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
                string columnType = "";
                string rawDataTypeName = reader.GetDataTypeName(i);
                bool isTextType = rawDataTypeName == "TEXT";
                bool isIntegerType = rawDataTypeName == "INTEGER";
                bool isBooleanType = rawDataTypeName == "BOOLEAN";
                bool isDateType = rawDataTypeName == "DATETIME";
                if (isTextType)
                {
                    columnType = "Текстовый";
                }
                else if (isIntegerType)
                {
                    columnType = "Числовой";
                }
                else if (isBooleanType)
                {
                    columnType = "Логический";
                }
                else if (isDateType)
                {
                    columnType = "Дата";
                }
                ColumnDefinition tableColumn = new ColumnDefinition();
                tableColumn.Width = new GridLength(275);
                tableRecords.ColumnDefinitions.Add(tableColumn);
                ComboBox tableColumnTypeBox = new ComboBox();
                tableColumnTypeBox.IsEnabled = false;
                tableColumnTypeBox.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnTypeBox.VerticalAlignment = VerticalAlignment.Center;
                ComboBoxItem tableColumnTypeBoxItem = new ComboBoxItem();
                tableColumnTypeBoxItem.Content = columnType;
                tableColumnTypeBoxItem.DataContext = ((string)(rawDataTypeName));
                tableColumnTypeBox.Items.Add(tableColumnTypeBoxItem);
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
            tableColumnTypeItem.Content = "Текстовый";
            tableColumnTypeItem.DataContext = "TEXT";
            tableColumnType.Items.Add(tableColumnTypeItem);
            tableColumnTypeItem = new ComboBoxItem();
            tableColumnTypeItem.Content = "Числовой";
            tableColumnTypeItem.DataContext = "INTEGER";
            tableColumnType.Items.Add(tableColumnTypeItem);
            tableColumnTypeItem = new ComboBoxItem();
            tableColumnTypeItem.Content = "Логический";
            tableColumnTypeItem.DataContext = "BOOLEAN";
            tableColumnType.Items.Add(tableColumnTypeItem);
            tableColumnType.SelectedIndex = 0;
            tableColumnTypeItem = new ComboBoxItem();
            tableColumnTypeItem.Content = "Дата";
            tableColumnTypeItem.DataContext = "DATETIME";
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

        public void SelectTableInEditModeHandler(object sender, RoutedEventArgs e)
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

        public void ClearArticleContent()
        {
            tableRecords.Children.Clear();
            tableRecords.RowDefinitions.Clear();
            tableRecords.ColumnDefinitions.Clear();
        }

        public void SelectTableInEditMode(string tableName)
        {
            ClearArticleContent();
            int filterColumnLength = filterColumn.Length;
            bool isFilterEnabled = filterColumnLength >= 1;
            string filterExpression = "";
            if (isFilterEnabled)
            {
                string filterAction = " ORDER BY ";
                string filterDirection = " DESC";
                filterExpression = filterAction + filterColumn + filterDirection;
            }
            string sql = "SELECT * FROM " + tableName + filterExpression;
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
                StackPanel tableColumnPanel = new StackPanel();
                tableColumnPanel.Orientation = Orientation.Horizontal;
                tableColumnPanel.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnPanel.VerticalAlignment = VerticalAlignment.Center;
                TextBlock tableColumnLabel = new TextBlock();
                tableColumnLabel.Text = columnName;
                tableColumnPanel.Children.Add(tableColumnLabel);
                bool isColumnsMatches = filterColumn == columnName;
                bool isAddFilterIcon = isFilterEnabled && isColumnsMatches;
                if (isAddFilterIcon)
                {
                    PackIcon tableColumnFilterIcon = new PackIcon();
                    tableColumnFilterIcon.Margin = new Thickness();
                    tableColumnFilterIcon.Kind = PackIconKind.ArrowDropDown;
                    tableColumnPanel.Children.Add(tableColumnFilterIcon);
                }
                tableRecords.Children.Add(tableColumnPanel);
                Grid.SetRow(tableColumnPanel, 0);
                Grid.SetColumn(tableColumnPanel, i);
                tableColumnLabel.DataContext = false;
                tableColumnLabel.MouseLeftButtonUp += ToggleFilterHandler;
            }

            while (reader.Read())
            {
                tableRow = new RowDefinition();
                tableRow.Height = new GridLength(35);
                tableRecords.RowDefinitions.Add(tableRow);
                int dataItemCursor = -1;
                for (int j = 0; j < reader.FieldCount; j++)
                {
                    dataItemCursor++;
                    string dataItemContent = "";
                    string rawDataType = reader.GetDataTypeName(dataItemCursor);
                    bool isTextType = rawDataType == "TEXT";
                    bool isIntegerType = rawDataType == "INTEGER";
                    bool isBooleanType = rawDataType == "BOOLEAN";
                    bool isDateType = rawDataType == "DATETIME";
                    if (isTextType)
                    {
                        dataItemContent = reader.GetString(dataItemCursor);
                    }
                    else if (isIntegerType)
                    {
                        Int64 intData = reader.GetInt64(dataItemCursor);
                        dataItemContent = intData.ToString();
                    }
                    else if (isBooleanType)
                    {
                        bool boolData = reader.GetBoolean(dataItemCursor);
                        if (boolData)
                        {
                            dataItemContent = "Да";
                        }
                        else
                        {
                            dataItemContent = "Нет";
                        }
                    }
                    else if (isDateType)
                    {
                        String rawDateData = reader.GetString(dataItemCursor);
                        try
                        {
                            DateTime parsedDate = DateTime.Parse(rawDateData);
                            dataItemContent = parsedDate.ToLongDateString();
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("Вы заполнили не все поля", "Ошибка");
                        }
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
                UIElement tableColumnBox = null;
                string rawDataTypeName = reader.GetDataTypeName(i);
                bool isTextType = rawDataTypeName == "TEXT";
                bool isIntegerType = rawDataTypeName == "INTEGER";
                bool isTextBlock = isTextType || isIntegerType;
                bool isComboBox = rawDataTypeName == "BOOLEAN";
                bool isDatePicker = rawDataTypeName == "DATETIME";
                if (isTextBlock)
                {
                    tableColumnBox = new TextBox();
                    TextBox dataItemLabel = tableColumnBox as TextBox;
                    dataItemLabel.TextChanged += DetectChangesHandler;
                }
                else if (isComboBox)
                {
                    tableColumnBox = new ComboBox();
                    ComboBox dataItemSelector = tableColumnBox as ComboBox;
                    dataItemSelector.HorizontalAlignment = HorizontalAlignment.Center;
                    dataItemSelector.VerticalAlignment = VerticalAlignment.Center;
                    ComboBoxItem dataItemSelectorItem = new ComboBoxItem();
                    dataItemSelectorItem.Content = "Да";
                    dataItemSelectorItem.DataContext = "TRUE";
                    dataItemSelector.Items.Add(dataItemSelectorItem);
                    dataItemSelectorItem = new ComboBoxItem();
                    dataItemSelectorItem.Content = "Нет";
                    dataItemSelectorItem.DataContext = "FALSE";
                    dataItemSelector.Items.Add(dataItemSelectorItem);
                    dataItemSelector.SelectedIndex = 0;
                }
                else if (isDatePicker)
                {
                    tableColumnBox = new DatePicker();
                    DatePicker dataItemPicker = tableColumnBox as DatePicker;
                }
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

        private void DetectChangesHandler(object sender, TextChangedEventArgs e)
        {
            DetectChanges();
        }

        public void DetectChanges()
        {
            isDetectChanges = true;
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

        public void SaveDBInConstructorMode()
        {
            MessageBoxResult result = MessageBox.Show("При сохранении будет очищена вся таблица", "Сохранение", MessageBoxButton.OKCancel);
            switch (result)
            {
                case MessageBoxResult.OK:
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
                                    string selectedsomeTableRecordBoxItemContent = ((string)(selectedsomeTableRecordBoxItem.DataContext));
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
                    int columnsLength = columns.Length;
                    bool isLengthExists = columnsLength >= 1;
                    if (isLengthExists)
                    {
                        columns = ", " + columns;
                        columnsLength = columns.Length;
                        int columnsCutLength = columnsLength - 2;
                        columns = columns.Substring(0, columnsCutLength);
                    }
                    sql = "CREATE TABLE IF NOT EXISTS " + currentTable + " (_id INTEGER PRIMARY KEY AUTOINCREMENT" + columns + ")";
                    command = new SQLiteCommand(sql, connection);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch
                    {
                        MessageBox.Show("Вы заполнили не все поля", "Ошибка");
                    }
                    break;
            }
            isDetectChanges = false;
        }

        public void SaveDBInEditMode()
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
                        int currentRow = Grid.GetRow(record);
                        bool isRowsMatches = row == currentRow;
                        bool isTextBox = record is TextBox;
                        bool isComboBox = record is ComboBox;
                        bool isDatePicker = record is DatePicker;
                        bool isDetectedType = isTextBox || isComboBox || isDatePicker;
                        bool isCanGetValue = isRowsMatches && isDetectedType;
                        if (isCanGetValue)
                        {
                            valuesCursor++;
                            bool isNotId = valuesCursor >= 1;
                            if (isNotId)
                            {
                                string separator = "";
                                string rawType = reader.GetDataTypeName(valuesCursor);
                                bool isCircumQuotes = rawType == "TEXT" || rawType == "DATETIME";
                                if (isCircumQuotes)
                                {
                                    separator = "\"";
                                }
                                string recordBoxContent = "";
                                if (isTextBox)
                                {
                                    TextBox recordBox = record as TextBox;
                                    recordBoxContent = recordBox.Text;
                                }
                                else if (isComboBox)
                                {
                                    ComboBox recordBox = record as ComboBox;
                                    int recordBoxSelectedIndex = recordBox.SelectedIndex;
                                    ItemCollection recordBoxItems = recordBox.Items;
                                    ComboBoxItem recordBoxSelectedItem = ((ComboBoxItem)(recordBoxItems[recordBoxSelectedIndex]));
                                    object recordBoxSelectedItemData = recordBoxSelectedItem.DataContext;
                                    recordBoxContent = ((string)(recordBoxSelectedItemData));
                                }
                                else if (isDatePicker)
                                {
                                    DatePicker recordBox = record as DatePicker;
                                    // object recordBoxData = recordBox.DataContext;
                                    // recordBoxContent = ((string)(recordBoxData));
                                    DateTime? selectedDate = ((DateTime?)(recordBox.SelectedDate));
                                    bool isDateSelected = selectedDate != null;
                                    if (isDateSelected)
                                    {
                                        DateTime date = selectedDate.Value;
                                        // ((DateTime?)(recordBox.SelectedDate));
                                        // recordBoxContent = date.ToLongDateString();
                                        // recordBoxContent = date.Year + "-" + date.Month + "-" + date.Day + " 00:00:00.000";
                                        // recordBoxContent = date.Day + "/" + date.Month + "/" + date.Year;
                                        recordBoxContent = date.Year + "-" + date.Month + "-" + date.Day + " 10:05:23.187";
                                        // recordBoxContent = date.Year + "-" + date.Month + "-" + date.Day;
                                    }
                                }
                                values += separator + recordBoxContent + separator + ", ";
                            }
                        }
                    }
                    values = values.Substring(0, values.Length - 2);
                    sql = "INSERT INTO " + currentTable + " (" + fields + ") VALUES (" + values + ")";
                    command = new SQLiteCommand(sql, connection);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch
                    {
                        MessageBox.Show("Вы заполнили не все поля", "Ошибка");
                    }
                }
            }
            SelectTableInEditMode(currentTable);
            isDetectChanges = false;
        }

        public void SaveDB()
        {
            bool isConstructorMode = currentTableMode == "constructor";
            bool isEditMode = currentTableMode == "edit";
            if (isConstructorMode)
            {
                SaveDBInConstructorMode();
            }
            else if (isEditMode)
            {
                SaveDBInEditMode();
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

        public void AddColumnHandler(object sender, KeyEventArgs e)
        {
            Key currentKey = e.Key;
            Key enterKey = Key.Enter;
            bool isEnterKey = currentKey == enterKey;
            if (isEnterKey)
            {

                isDetectChanges = true;

                TextBox column = ((TextBox)(sender));

                ComboBox tableColumnType = new ComboBox();
                tableColumnType.HorizontalAlignment = HorizontalAlignment.Center;
                tableColumnType.VerticalAlignment = VerticalAlignment.Center;
                ComboBoxItem tableColumnTypeItem = new ComboBoxItem();
                tableColumnTypeItem.Content = "Текстовый";
                tableColumnTypeItem.DataContext = ((string)("TEXT"));
                tableColumnType.Items.Add(tableColumnTypeItem);
                tableColumnTypeItem = new ComboBoxItem();
                tableColumnTypeItem.Content = "Числовой";
                tableColumnTypeItem.DataContext = ((string)("INTEGER"));
                tableColumnType.Items.Add(tableColumnTypeItem);
                tableColumnTypeItem = new ComboBoxItem();
                tableColumnTypeItem.Content = "Логический";
                tableColumnTypeItem.DataContext = ((string)("BOOLEAN"));
                tableColumnType.Items.Add(tableColumnTypeItem);
                tableColumnType.SelectedIndex = 0;
                tableColumnTypeItem = new ComboBoxItem();
                tableColumnTypeItem.Content = "Дата";
                tableColumnTypeItem.DataContext = ((string)("DATETIME"));
                tableColumnType.Items.Add(tableColumnTypeItem);
                tableColumnType.SelectedIndex = 0;
                tableRecords.Children.Add(tableColumnType);
                Grid.SetRow(tableColumnType, 0);
                Grid.SetColumn(tableColumnType, Grid.GetColumn(column) + 1);

                TextBlock savedColumn = new TextBlock();
                Grid.SetRow(savedColumn, 1);
                int columnIndex = Grid.GetColumn(column);
                Grid.SetColumn(savedColumn, columnIndex);
                savedColumn.HorizontalAlignment = HorizontalAlignment.Center;
                savedColumn.VerticalAlignment = VerticalAlignment.Center;
                savedColumn.Text = column.Text;
                ContextMenu savedColumnContextMenu = new ContextMenu();
                MenuItem savedColumnContextMenuItem = new MenuItem();
                savedColumnContextMenuItem.Header = "Удалить";
                savedColumnContextMenuItem.DataContext = ((int)(columnIndex));
                savedColumnContextMenuItem.Click += RemoveColumnHandler;
                savedColumnContextMenu.Items.Add(savedColumnContextMenuItem);
                savedColumn.ContextMenu = savedColumnContextMenu;
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
                tableScroller.ScrollToRightEnd();
                newTableColumn.Focus();
            }
        }

        private void CloseTableHandler(object sender, RoutedEventArgs e)
        {
            bool isTableOpened = currentTable.Length >= 1;
            if (isTableOpened)
            {
                if (isDetectChanges)
                {
                    MessageBoxResult result = MessageBox.Show("Вы собираетесь закрыть несохраненные изменения. Сохранить?", "Сохранение", MessageBoxButton.OKCancel);
                    switch (result)
                    {
                        case MessageBoxResult.OK:
                            bool isConstructorMode = currentTableMode == "constructor";
                            bool isEditMode = currentTableMode == "edit";
                            if (isConstructorMode)
                            {
                                SaveDBInConstructorMode();
                            }
                            else if (isEditMode)
                            {
                                SaveDBInEditMode();
                            }
                            isDetectChanges = false;
                            CloseTable();
                            break;
                    }
                }
                else
                {
                    CloseTable();
                }
            }
        }

        public void CloseTable()
        {
            currentTable = "";
            currentTableMode = "";
            ClearArticleContent();
        }

        public void RemoveColumnHandler(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = ((MenuItem)(sender));
            int column = ((int)(menuItem.DataContext));
            RemoveColumn(column);
        }

        public void RemoveColumn(int column)
        {
            List<UIElement> records = new List<UIElement>();
            UIElementCollection tableItems = tableRecords.Children;
            int countTableItems = tableItems.Count;
            for (int i = 0; i < countTableItems; i++)
            {
                UIElement tableItem = tableItems[i];
                int tableItemColumn = Grid.GetColumn(tableItem);
                bool isColumnMatches = tableItemColumn == column;
                if (isColumnMatches)
                {
                    records.Add(tableItem);
                }
            }
            tableRecords.ColumnDefinitions.RemoveAt(column);
            foreach (var record in records)
            {
                tableRecords.Children.Remove(record);
            }
            foreach (UIElement record in tableRecords.Children)
            {
                int currentColumn = Grid.GetColumn(record);
                bool isColumnAfterModified = currentColumn > column;
                if (isColumnAfterModified)
                {
                    int previousColumn = currentColumn - 1;
                    Grid.SetColumn(record, previousColumn);
                    bool isTextBlock = record is TextBlock;
                    if (isTextBlock)
                    {
                        TextBlock columnLabel = record as TextBlock;
                        ContextMenu menu = columnLabel.ContextMenu;
                        ItemCollection menuItems = menu.Items;
                        object firstMenuItem = menuItems[0];
                        MenuItem removeMenuItem = ((MenuItem)(firstMenuItem));
                        removeMenuItem.DataContext = ((int)(currentColumn));
                    }
                }
            }
        }

        private void CloseDBHandler(object sender, RoutedEventArgs e)
        {
            CloseDB();
        }

        public void CloseDB()
        {
            ClearArticleContent();
            screenControl.SelectedIndex = 0;
            bool isConnectionOpened = connection != null;
            if (isConnectionOpened)
            {
                connection.Close();
            }
        }

        private void OpenRelationShipChartHandler(object sender, MouseButtonEventArgs e)
        {
            OpenRelationShipChart();
        }

        public void OpenRelationShipChart ()
        {
            relationShip.Children.Clear();
            article.SelectedIndex = 1;
            string sql = "SELECT name FROM sqlite_schema WHERE type = \'table\' AND name NOT LIKE \'sqlite_%\'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string tableName = reader.GetString(0);
                Rectangle table = new Rectangle();
                table.Width = 100;
                table.Height = 100;
                VisualBrush tableBrush = new VisualBrush();
                tableBrush.Stretch = Stretch.None;
                StackPanel tableBrushContent = new StackPanel();
                tableBrushContent.Orientation = Orientation.Horizontal;
                tableBrushContent.Width = 100;
                tableBrushContent.Height = 100;
                tableBrushContent.Background = System.Windows.Media.Brushes.Red;
                RadioButton tableBrushJoint = new RadioButton();
                tableBrushJoint.Margin = new Thickness(5, 40, 5, 40);
                TextBlock tableBrushLabel = new TextBlock();
                tableBrushLabel.HorizontalAlignment = HorizontalAlignment.Center;
                tableBrushLabel.Margin = new Thickness(5, 40, 5, 40);
                tableBrushLabel.Text = tableName;
                tableBrushContent.Children.Add(tableBrushJoint);
                tableBrushContent.Children.Add(tableBrushLabel);
                tableBrush.Visual = tableBrushContent;
                table.Fill = tableBrush;
                Canvas.SetLeft(table, 0);
                Canvas.SetTop(table, 0);
                relationShip.Children.Add(table);
                table.MouseMove += MoveTableHandler;
                table.MouseLeftButtonDown += StartDrawRelationHandler;
                table.MouseLeftButtonUp += ResetRelationShipHandler;
                tableBrushJoint.MouseLeftButtonDown += StartDrawRelationHandler;
            };
        }

        public void StartDrawRelationHandler(object sender, MouseEventArgs e)
        {
            // RadioButton joint = ((RadioButton)(sender));
            // StartDrawRelation(joint, e);
            StartDrawRelation(e);
        }

        public void StartDrawRelation(MouseEventArgs e)
        {
            if (isRelationSet)
            {
                Point currentPoint = e.GetPosition(relationShip);
                relation = new Line();
                relation.X1 = currentPoint.X;
                relation.Y1 = currentPoint.Y;
                relation.X2 = currentPoint.X;
                relation.Y2 = currentPoint.Y;
                relationShip.Children.Add(relation);
                relation.Stroke = System.Windows.Media.Brushes.Black;
            }
        }

        public void MoveTableHandler (object sender, MouseEventArgs e)
        {
            Rectangle table = ((Rectangle)(sender));
            MoveTableHandler(table, e);
        }

        public void MoveTableHandler(Rectangle table, MouseEventArgs e)
        {
            MouseButtonState leftMouseBtnState = e.LeftButton;
            MouseButtonState isPressedMouseBtn = MouseButtonState.Pressed;
            bool isLeftMouseBtnPressed = leftMouseBtnState == isPressedMouseBtn;
            bool isCanMove = isLeftMouseBtnPressed && !isRelationSet;
            if (isCanMove)
            {
                double offset = 5;
                Point currentPosition = e.GetPosition(relationShip);
                double coordX = currentPosition.X;
                double coordY = currentPosition.Y;
                double tableXCoord = coordX - offset;
                double tableYCoord = coordY - offset;
                Canvas.SetLeft(table, tableXCoord);
                Canvas.SetTop(table, tableYCoord);
            }
        }

        private void MoveRelationShipHandler(object sender, MouseEventArgs e)
        {
            MouseButtonState leftMouseBtnState = e.LeftButton;
            MouseButtonState isPressedMouseBtn = MouseButtonState.Pressed;
            bool isLeftMouseBtnPressed = leftMouseBtnState == isPressedMouseBtn;
            bool isRelationEnabled = isLeftMouseBtnPressed && isRelationSet;
            if (isRelationEnabled)
            {
                Point currentPosition = e.GetPosition(relationShip);
                relation.X2 = currentPosition.X;
                relation.Y2 = currentPosition.Y;
            }
        }

        private void ResetRelationShipHandler(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SetRelationShipHandler(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)(sender));
            isRelationSet = ((bool)(btn.IsChecked));
            if (isRelationSet)
            {
                Point currentPoint = Mouse.GetPosition(relationShip);
                isRelationSet = true;
                relation = new Line();
                relation.X1 = currentPoint.X;
                relation.Y1 = currentPoint.Y;
                relation.X2 = currentPoint.X;
                relation.Y2 = currentPoint.Y;
                relationShip.Children.Add(relation);
                relation.Stroke = System.Windows.Media.Brushes.Black;
            }
            else
            {
                for (int i = 0; i < relationShip.Children.Count; i++)
                {
                    UIElement element = relationShip.Children[i];
                    bool isRelation = element is Line;
                    if (isRelation)
                    {
                        relationShip.Children.Remove(element);
                    }
                }
            }
        }

        public void ToggleFilterHandler(object sender, MouseEventArgs e)
        {
            TextBlock header = ((TextBlock)(sender));
            string headerTextContent = header.Text;
            ToggleFilter(headerTextContent);
        }

        public void ToggleFilter(string content)
        {
            int filterColumnLength = filterColumn.Length;
            bool isFilterEnabled = filterColumnLength <= 0;
            if (isFilterEnabled)
            {
                filterColumn = content;
            }
            else
            {
                filterColumn = "";
            }
            SelectTableInEditMode(currentTable);
        }

    }
}
