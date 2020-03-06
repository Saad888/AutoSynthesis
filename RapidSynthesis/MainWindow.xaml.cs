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
using WindowsInput.Native;
using System.Threading;

namespace RapidSynthesis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var testingHotkeys = new Dictionary<HKType, Hotkey>();
            testingHotkeys.Add(HKType.Macro1, new Hotkey(VirtualKeyCode.VK_1, 3000));
            testingHotkeys.Add(HKType.Macro2, new Hotkey(VirtualKeyCode.VK_2, 3000));
            testingHotkeys.Add(HKType.Macro3, new Hotkey(VirtualKeyCode.VK_3, 3000));
            testingHotkeys.Add(HKType.Confirm, new Hotkey(VirtualKeyCode.VK_4));
            testingHotkeys.Add(HKType.Cancel, new Hotkey(VirtualKeyCode.VK_5));
            testingHotkeys.Add(HKType.Food, new Hotkey(VirtualKeyCode.VK_6, 1000));
            testingHotkeys.Add(HKType.Syrup, new Hotkey(VirtualKeyCode.VK_7, 1000));

            int craftCount = 3;
            bool collectableCraft = true;
            bool fourtyMinuteFood = false;
            int startingFoodTime = 1;
            int startingSyrupTime = 1;
            var settings = new SettingsContainer(craftCount, collectableCraft, fourtyMinuteFood, startingFoodTime, startingSyrupTime);

            CraftingEngine.InitiateCraftingEngine(testingHotkeys, settings);
           
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Text = e.Key.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                UpdateProgressBar(100);
            });
        }

        private void UpdateProgressBar(int target)
        {
            Action action = () => { SetBar(target); };
            TestBar.Dispatcher.BeginInvoke(action);
        }

        private void SetBar(int target)
        {
            while (TestBar.Value < target)
            {
                var currentValue = TestBar.Value;
                var nextValue = Math.Ceiling((target - currentValue) / 2 + currentValue);
                TestBar.Value = nextValue;
                Thread.Sleep(100);
            }
        }
    }
}
