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

namespace AutoSynthesis.Windows
{
    /// <summary>
    /// Interaction logic for SaveDialog.xaml
    /// </summary>
    public partial class SaveDialog : Window
    {
        public SaveDialog(string initialText)
        {
            InitializeComponent();
            TXBName.Text = initialText;
        }

        public string SaveName
        {
            get { return TXBName.Text; }
            set { TXBName.Text = value; }
        }

        private void BTNConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TXBName.Focus();
            TXBName.SelectAll();
        }

        private void TXBName_Press(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !String.IsNullOrEmpty(TXBName.Text))
                DialogResult = true;
        }
    }
}
