using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MunApp.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }

        private void fullScreencb_Unchecked(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
        }
    }
}
