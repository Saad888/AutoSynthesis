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
    // keycodes being eaten
    // https://stackoverflow.com/questions/1458748/wpf-onkeydown-not-being-called-for-space-key-in-control-derived-from-wpf-text
    // brush colors:
    // https://stackoverflow.com/questions/979876/set-background-color-of-wpf-textbox-in-c-sharp-code

    // GLITCHES:
    // Double shift kinda acts weird
    

    // TO DO: 
    // Check behaviour when you change windows altogehter



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region hotkey private properties
        private Dictionary<TextBox, HotkeyContainer> HKTContainers { get; set; }
        private enum HotkeyStates
        {
            UNFOCUSED,
            FOCUSED,
            ACTIVE
        }
        private Dictionary<HotkeyStates, System.Windows.Media.SolidColorBrush> HKTBrushes { get; set; }
        #endregion

        #region time private properties
        private Dictionary<TextBox, TimeInputContainer> TimeContainers { get; set; }
        private enum TimeStates
        {
            UNFOCUSED, 
            FOCUSED
        }
        private Dictionary<TimeStates, System.Windows.Media.SolidColorBrush> TimeBrushes { get; set; }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            // Attach all hotkey textboxes
            HKTContainers = new Dictionary<TextBox, HotkeyContainer>();
            HKTContainers.Add(TXBMacro1Key, new HotkeyContainer());

            // Attach all time textboxes
            TimeContainers = new Dictionary<TextBox, TimeInputContainer>();
            TimeContainers.Add(TXBMacro1Timer, new TimeInputContainer());

            // Set all brush dictionaries
            HKTBrushes = new Dictionary<HotkeyStates, SolidColorBrush>();
            HKTBrushes.Add(HotkeyStates.UNFOCUSED, Brushes.White);
            HKTBrushes.Add(HotkeyStates.FOCUSED, Brushes.Yellow);
            HKTBrushes.Add(HotkeyStates.ACTIVE, Brushes.Red);

            TimeBrushes = new Dictionary<TimeStates, SolidColorBrush>();
            TimeBrushes.Add(TimeStates.UNFOCUSED, Brushes.White);
            TimeBrushes.Add(TimeStates.FOCUSED, Brushes.Yellow);

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


        #region hotkey methods
        private void HotkeyKeyDown(object sender, KeyEventArgs e)
        {
            // If key state is not accepting new inputs, do nothing UNLESS its a SPACE or ENTER
            //      Then set the key state to accept inputs 
            // If key state is accepting inputs, process the inputs

            // get textbox and state information
            var textBox = (TextBox)sender;
            var hotkeyContainer = HKTContainers[textBox];
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // check state
            if (hotkeyContainer.AcceptingInputs)
            {
                // add if modifier, send if not
                switch(key)
                {
                    case Key.LeftShift:
                    case Key.RightShift:
                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                    case Key.LeftAlt:
                    case Key.RightAlt:
                        if (!HotkeyProcessor.NumpadKey(hotkeyContainer.LastPressedKey))  //Numpads do not behave well with modifiers
                            if (hotkeyContainer.LastPressedKey != Key.None)  // Update visuals if key is currently stored
                                textBox.Text = HotkeyProcessor.ProcessEventInputs(hotkeyContainer.LastPressedKey, hotkeyContainer.ActiveModKeys);
                        hotkeyContainer.ActiveModKeys.Add(key);
                        break;
                    default:
                        hotkeyContainer.ActiveNonModKeys.Add(key);
                        hotkeyContainer.LastPressedKey = key;
                        textBox.Text = HotkeyProcessor.ProcessEventInputs(key, hotkeyContainer.ActiveModKeys);
                        break;
                }
            } else
            {
                // if ENTER or SPACE, then start accepting inputs
                if (key == Key.Enter || key == Key.Space)
                    ActivateHotkeyTextbox(textBox);
            }
        }

        private void HotkeyKeyUp(object sender, KeyEventArgs e)
        {
            // if not accepting inputs, do nothing
            // if accepting inputs, check if the last non-mod key has been lifted. If that happens, stop accepting inputs

            var textBox = (TextBox)sender;
            var hotkeyContainer = HKTContainers[textBox];
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (!hotkeyContainer.AcceptingInputs)
                return;

            if (hotkeyContainer.ActiveNonModKeys.Contains(key))
            {
                hotkeyContainer.ActiveNonModKeys.Remove(key);
                if (key == hotkeyContainer.LastPressedKey)
                {
                    DeactivateHotkeyTextbox(textBox);
                }
            }
            if (hotkeyContainer.ActiveModKeys.Contains(key))
            {
                // glitch: shift release event doesnt fire if both shifts are being pressed and only one is released
                hotkeyContainer.ActiveModKeys.Remove(key);
                if (hotkeyContainer.LastPressedKey != Key.None)
                {
                    textBox.Text = HotkeyProcessor.ProcessEventInputs(hotkeyContainer.LastPressedKey, hotkeyContainer.ActiveModKeys);
                }
            }
        }

        private void ActivateHotkeyTextbox(TextBox txb)
        {
            HKTContainers[txb] = new HotkeyContainer();
            HKTContainers[txb].AcceptingInputs = true;
            txb.Background = HKTBrushes[HotkeyStates.ACTIVE];
            txb.Text = "Enter Keybind...";

            DisableTabbing();
        }

        private void DeactivateHotkeyTextbox(TextBox txb)
        {
            HKTContainers[txb].AcceptingInputs = false;
            txb.Background = HKTBrushes[HotkeyStates.FOCUSED];
            EnableTabbing();
        }

        private void HotkeyTextboxGainFocus(object sender, RoutedEventArgs e)
        {
            var txb = (TextBox)sender;
            txb.Background = HKTBrushes[HotkeyStates.FOCUSED];
        }

        private void HotkeyTextboxLoseFocus(object sender, RoutedEventArgs e)
        {
            var txb = (TextBox)sender;
            DeactivateHotkeyTextbox(txb);
            txb.Background = HKTBrushes[HotkeyStates.UNFOCUSED];
            EnableTabbing();
        }
        #endregion

        #region timeline methods
        // TAKE FOCUS: Change to focused color
        // LOSE FOCUS: Change to normal color
        // KEY IN: Only accept numbers, up to 2 digits. When taking focus, get ready to reset the number

        private void SetTimeValue(object sender, KeyEventArgs e)
        {
            var tbx = (TextBox)sender;
            var container = TimeContainers[tbx];
            int keyValue = 0;
            if (TimeInputProcessor.TryGetNumber(e.Key, ref keyValue))
            {
                var currentTime = container.TimeInSeconds;
                // if freshly focused, reset the timer on input
                if (container.FreshFocus)
                    currentTime = 0;

                var newTime = (currentTime % 10) * 10 + keyValue;
                container.TimeInSeconds = newTime;
                tbx.Text = newTime.ToString();
                container.FreshFocus = false;
            }
        }

        private void TimeGetFocus(object sender, RoutedEventArgs e)
        {
            var tbx = (TextBox)sender;
            var container = TimeContainers[tbx];
            // Set fresh focus to true
            container.FreshFocus = true;
            // Modify display
            tbx.Background = TimeBrushes[TimeStates.FOCUSED];
        }

        private void TimeLostFocus(object sender, RoutedEventArgs e)
        {
            var tbx = (TextBox)sender;
            // Modify display
            tbx.Background = TimeBrushes[TimeStates.UNFOCUSED];
        }
        #endregion

        #region full window methods
        private void DisableTabbing()
        {
            KeyboardNavigation.SetTabNavigation(MainWindowGrid, KeyboardNavigationMode.None);
        }
        private void EnableTabbing()
        {
            KeyboardNavigation.SetTabNavigation(MainWindowGrid, KeyboardNavigationMode.Continue);
        }

        private void TEST_Click(object sender, RoutedEventArgs e)
        {
            KeyInputEngine.SendKeysToGame(HKTContainers[TXBMacro1Key].Keys(), HKTContainers[TXBMacro1Key].ModKeys());
        }
        #endregion


        //    private void Button_Click(object sender, RoutedEventArgs e)
        //    {
        //        Task.Run(() =>
        //        {
        //            UpdateProgressBar(100);
        //        });
        //    }

        //    private void UpdateProgressBar(int target)
        //    {
        //        Action action = () => { SetBar(target); };
        //        TestBar.Dispatcher.BeginInvoke(action);
        //    }

        //    private void SetBar(int target)
        //    {
        //        while (TestBar.Value < target)
        //        {
        //            var currentValue = TestBar.Value;
        //            var nextValue = Math.Ceiling((target - currentValue) / 2 + currentValue);
        //            TestBar.Value = nextValue;
        //            Thread.Sleep(100);
        //        }
        //    }
        //}
    }
}
