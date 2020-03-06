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
            testingHotkeys.Add(HKType.Confirm, new Hotkey(VirtualKeyCode.VK_C));
            testingHotkeys.Add(HKType.Cancel, new Hotkey(VirtualKeyCode.VK_B));
            testingHotkeys.Add(HKType.Food, new Hotkey(VirtualKeyCode.VK_F, 1000));
            testingHotkeys.Add(HKType.Syrup, new Hotkey(VirtualKeyCode.VK_S, 1000));

            int craftCount = 5;
            bool collectableCraft = false;
            bool fourtyMinuteFood = false;
            int startingFoodTime = 10;
            int startingSyrupTime = 20;
            var settings = new SettingsContainer(craftCount, collectableCraft, fourtyMinuteFood, startingFoodTime, startingSyrupTime);

            CraftingEngine.InitiateCraftingEngine(testingHotkeys, settings);
           
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
