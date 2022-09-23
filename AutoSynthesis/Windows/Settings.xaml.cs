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
        public string StartDelay {
            get { return TXBStartDelay.Text; }
        }

        public string EndDelay
        {
            get { return TXBEndDelay.Text; }
        }

        public Settings(string startDelayTime, string endDelayTime)
        {
            InitializeComponent();
            TXBStartDelay.Text = startDelayTime;
            TXBEndDelay.Text = endDelayTime;
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
            TXBStartDelay.Focus();
            TXBStartDelay.SelectAll();
        }
    }
}
