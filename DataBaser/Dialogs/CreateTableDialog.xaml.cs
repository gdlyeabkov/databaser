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
using System.Windows.Shapes;

namespace DataBaser.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для CreateTableDialog.xaml
    /// </summary>
    public partial class CreateTableDialog : Window
    {
        public CreateTableDialog()
        {
            InitializeComponent();
        }

        private void CreateTableHandler(object sender, RoutedEventArgs e)
        {
            string tableNameBoxContent = tableNameBox.Text;
            this.DataContext = ((string)(tableNameBoxContent));
            this.Close();
        }

    }
}
