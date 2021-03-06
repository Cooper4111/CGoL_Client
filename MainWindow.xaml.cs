﻿using System;
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
        string gliderDir = null;
        string targetIP  = "127.0.0.1";
        string login     = "";
        string password  = "";

        public MainWindow()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            InitializeComponent();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Connecting to " + targetIP);
            ThreadMaster.RUN(field, Dispatcher, targetIP, login, password);
        }

        private void FieldClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            ThreadMaster.Send2Srv((int)p.X, (int)p.Y, gliderDir);
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRadio = (RadioButton)e.Source;
            gliderDir = selectedRadio.Content.ToString(); // gliderDir в отдельный класс вне WFP
        }

        private void GetIP(object sender, TextChangedEventArgs e)
        {
            targetIP = IP_address.Text;
        }
        private void GetPass(object sender, TextChangedEventArgs e)
        {
            password = Password.Text;
        }
        private void GetLogin(object sender, TextChangedEventArgs e)
        {
            login = Login.Text;
        }
    }
}