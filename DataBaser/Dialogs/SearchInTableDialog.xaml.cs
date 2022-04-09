using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
using System.Windows.Shapes;

namespace DataBaser.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SearchInTableDialog.xaml
    /// </summary>
    public partial class SearchInTableDialog : Window
    {

        public SQLiteConnection connection;
        public string tableName = "";

        public SearchInTableDialog(SQLiteConnection connection, string tableName)
        {
            InitializeComponent();

            Initialize(connection, tableName);
        
        }

        public void Initialize(SQLiteConnection connection, string tableName)
        {
            this.tableName = tableName;
            string sql = "SELECT * FROM " + tableName;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                ComboBoxItem columnSelectorItem = new ComboBoxItem();
                columnSelectorItem.Content = reader.GetName(i);
                columnSelectorItem.DataContext = ((string)(reader.GetDataTypeName(i)));
                columnSelector.Items.Add(columnSelectorItem);
            }
            columnSelector.SelectedIndex = 0;
        }


        private void SearchInTableHandler(object sender, RoutedEventArgs e)
        {
            SearchInTable();
        }

        public void SearchInTable()
        {
            string keywords = keywordsLabel.Text;
            // this.DataContext = ((string)(keywords));
            Dictionary<String, Object> dialogData = new Dictionary<String, Object>();
            ComboBoxItem selectedItem = ((ComboBoxItem)(columnSelector.Items[columnSelector.SelectedIndex]));
            string selectedItemData = ((string)(selectedItem.DataContext));
            string selectedItemContent = ((string)(selectedItem.Content));
            dialogData.Add("column", ((string)(selectedItemContent)));
            bool isCircumQuotes = selectedItemData == "TEXT" || selectedItemData == "DATETIME";
            if (isCircumQuotes)
            {
                keywords = "\"%" + keywords + "%\"";
            }
            dialogData.Add("keywords", ((string)(keywords)));
            this.DataContext = ((Dictionary<String, Object>)(dialogData));
            this.Close();
        }

    }
}
