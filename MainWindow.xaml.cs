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

namespace ClientApp
{
    public partial class MainWindow : Window
    {
        string gliderDir = "NW";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RUN_Btn(object sender, RoutedEventArgs e)
        {
            ThreadMaster.RUN(field, this.Dispatcher);
        }

        private void FieldClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            ThreadMaster.Send2Srv((int)p.X, (int)p.Y, gliderDir);
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRadio = (RadioButton)e.Source;
            gliderDir = selectedRadio.Content.ToString();
        }
    }
}