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
using System.Text.RegularExpressions;

namespace AutoSynthesis.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public string Time {
            get { return TXBDelay.Text; }
        }

        public Settings(string initialTime)
        {
            InitializeComponent();
            TXBDelay.Text = initialTime;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BTNConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TXBDelay.Focus();
            TXBDelay.SelectAll();
        }
    }
}
